using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using MyMail.Models.DBmanager;
using MyMail.Models.DriveManager;
using MyMail.Models.Entities;
using NetWork.MailReciever;
using NetWork.MailSender;
using Attachment = MyMail.Models.Entities.Attachment;

namespace MyMail.Models
{
    public class ServiceManager : IServiceManager
    {
        private IDBprovider _dBprovider;
        private IDriveAccesProvider _driveProvider;
        private ISender _mailSender;
        private IResiever _mailResiever;

        private Account _curentAccount;
        private List<Message_obj> _localMessages;

        private Thread _backgroundListener;

        public ServiceManager(IDBprovider providerParam, IDriveAccesProvider driveProvider_param, 
            ISender sender, IResiever resiever)
        {
            _dBprovider = providerParam;
            _driveProvider = driveProvider_param;
            _mailSender = sender;
            _mailResiever = resiever;

            //По другому пока не знаю
            _backgroundListener = new Thread(new ThreadStart(_startListen));
            _backgroundListener.IsBackground = true;
        }

        public bool IsUserPresent(string login, string password)
        {
            var user = _dBprovider.GetUser(login);

            if (user != null && user.Password == password)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AddUser(string login, string password)
        {
            User user = new User
            {
                Login = login,
                Password = password
            };

            try
            {
                _dBprovider.SaveObject(user);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool AddUser(User user)
        {
            try
            {
                _dBprovider.SaveObject(user);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void ChangeAccount(string email)
        {
            _curentAccount = _dBprovider.GetAccount(email);
        }

        public string GetCurentAccountEmail()
        {
            return _curentAccount == null ? "" : _curentAccount.MailAddress;
        }

        public IEnumerable<string> GetUsersAccountEmails(string login)
        {
            return _dBprovider.GetUsersAccounts(login).Select(x => x.MailAddress);
        }

        //!!!!
        public bool TrySetCurentAcount(string login)
        {
            var accounts = _dBprovider.GetUsersAccounts(login).ToArray();

            //Если уже есть аккаунт, то новый не надо
            if (accounts.Length == 0 || _curentAccount != null)
            {
                return false;
            }
            else
            {
                _curentAccount = accounts.First();

                _customiseServerWorkers();

                //Cтоит подумать о асинхронности
                _localMessages = _getSavedMessages(State.Incoming).ToList();

                //Если слушатель не работает - включить
                if(!_backgroundListener.IsAlive)
                    _backgroundListener.Start();

                return true;
            }
        }

        private void _customiseServerWorkers()
        {
            _mailSender.SetServer(_curentAccount.SmtpServerHost, _curentAccount.SmtpServerPort);
            _mailSender.SetCredentials(_curentAccount.MailAddress, _curentAccount.MailPassword);

            _mailResiever.SetServer(_curentAccount.Pop3ServerHost, _curentAccount.Pop3ServerPort);
            _mailResiever.SetCredentials(_curentAccount.MailAddress, _curentAccount.MailPassword);
        }

        public bool AddAccount(Account account, string login)
        {
            try
            {
                User user = _dBprovider.GetUser(login);

                account.AccountUser = user;

                account.LocalPath = _driveProvider.addAccountFolder(account.MailAddress);

                _dBprovider.SaveObject(account);

                if (_curentAccount == null)
                {
                    _curentAccount = account;

                    _customiseServerWorkers();

                    //async
                    _localMessages = _getSavedMessages(State.Incoming).ToList();

                    //Возможно лишнее
                    if(!_backgroundListener.IsAlive)
                        _backgroundListener.Start();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //Возможно сделать асинхронным
        //Внутреннее получение писем. Когда выбирается аккаунт надо загрузить уже записанные письма
        //Не путать с получением писем для выдачи пользователю
        private IEnumerable<Message_obj> _getSavedMessages(State state)
        {
            if (_curentAccount.Mails.ToArray().Length == 0)
                return new List<Message_obj>();

            var uids = _curentAccount.Mails.Where(m => m.MailState == state).Select(m => m.Uid).ToArray();

            IEnumerable<Message_obj> mails = _driveProvider.getSavedMessages(
                Path.Combine(_curentAccount.LocalPath, Enum.GetName(typeof (State), State.Incoming)),
                uids
                );

            return mails;
        }

        private void _listenOnMails()
        {
            var uids = _curentAccount.Mails.Where(m => m.MailState == State.Incoming).Select(m => m.Uid);

            IEnumerable<Message_obj> incoming = _mailResiever.GetIncomingMails(uids);

            int i = 0;
            foreach (Message_obj message in incoming)
            {
                i++;
                try
                {
                    //Создаем письмо для БД
                    Mail m = new Mail
                    {
                        MailAccount = _curentAccount,
                        Uid = message.Uid,
                        MailState = State.Incoming,
                        Attachments = new List<Attachment>()
                    };

                    //Сохраняем в базе письмо
                    _dBprovider.SaveObject(m);

                    _curentAccount.Mails.ToList().Add(m);

                    //Аттачи для БД
                    if (message.Attachments != null)
                    {
                        foreach (var attach in message.Attachments)
                        {
                            //избавляюсь от нежелательных симаолов в имени атача
                            attach.Name = attach.Name.Replace('/', '.');
                            attach.Name = attach.Name.Replace('?', '1');

                            Attachment att = new Attachment
                            {
                                FileName = attach.Name,
                                MailOwner = m
                            };

                            //Сохраням атач и прикрепляем к письму
                            _dBprovider.SaveObject(att);

                            m.Attachments.ToList().Add(att);
                        }
                    }

                    //А еще на диск сохрани!
                    _driveProvider.SaveMessage(
                        Path.Combine(_curentAccount.LocalPath, Enum.GetName(typeof(State), State.Incoming)),
                        message);

                    //Добавляем содержимое письма к массиву в программе
                    lock (_curentAccount.MailAddress)
                    {
                        _localMessages.Add(message);
                    }
                }
                catch (Exception ex)
                {
                    //!!!
                    throw ex;
                }
            }
        }

        private void _startListen()
        {
            do
            {
                _listenOnMails();

                Thread.Sleep(60000);
            } while (true);
        }
    }
}