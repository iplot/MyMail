﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using MyMail.Infrastructure;
using MyMail.Models.CryptoManager;
using MyMail.Models.DBmanager;
using MyMail.Models.DriveManager;
using MyMail.Models.Entities;
using NetWork.MailReciever;
using NetWork.MailSender;
using NHibernate.Mapping;
using System.Security.Cryptography;
using Attachment = MyMail.Models.Entities.Attachment;

namespace MyMail.Models
{
    public class ServiceManager : IServiceManager
    {
        private IDBprovider _dBprovider;
        private IDriveAccesProvider _driveProvider;
        private ISender _mailSender;
        private IResiever _mailResiever;
        private ICryptoProvider _cryptoProvider;

        private Account _curentAccount;
        private List<Message_obj> _localMessages;

        private Thread _backgroundListener;

        public ServiceManager(IDBprovider providerParam, IDriveAccesProvider driveProvider_param, 
            ISender sender, IResiever resiever, ICryptoProvider cryptoProvider_param)
        {
            _dBprovider = providerParam;
            _driveProvider = driveProvider_param;
            _mailSender = sender;
            _mailResiever = resiever;
            _cryptoProvider = cryptoProvider_param;

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

        //!!!!!!!!!!!!!!!!!!!!!!!!!!
        public void ChangeAccount(string email)
        {
            //Прекращаем прослушку
            if (_backgroundListener.IsAlive)
                _backgroundListener.Interrupt();

            _curentAccount = _dBprovider.GetAccount(email);

            //--------------------------------------------------------------------------------
            _customiseAccount();
            //------------------------------------------------------------------------------
        }

        private void _customiseAccount()
        {
            //Задаем ключи
            _cryptoProvider.SetRsaKeys(_curentAccount.Key.ToList()[0].D, _curentAccount.Key.ToList()[0].E,
                _curentAccount.Key.ToList()[0].N, _curentAccount.Key.ToList()[0].DP,
                _curentAccount.Key.ToList()[0].DQ, _curentAccount.Key.ToList()[0].InverseQ,
                _curentAccount.Key.ToList()[0].P, _curentAccount.Key.ToList()[0].Q);

            _cryptoProvider.SetDsaKeys(_curentAccount.Sign.ToList()[0].Counter, _curentAccount.Sign.ToList()[0].G,
                _curentAccount.Sign.ToList()[0].J, _curentAccount.Sign.ToList()[0].P,
                _curentAccount.Sign.ToList()[0].Q, _curentAccount.Sign.ToList()[0].Seed,
                _curentAccount.Sign.ToList()[0].X, _curentAccount.Sign.ToList()[0].Y);

            _customiseServerWorkers();


            //Получаем сообщения
            //Cтоит подумать о асинхронности
            _localMessages = _getSavedMessages(State.Incoming).ToList();
            _localMessages.AddRange(_getSavedMessages(State.Outgoing).ToList());

            //Сортируем письма в акаунте и локальном списке
            _localMessages.Sort(new MailsComparator());
            List<Mail> mails = _curentAccount.Mails.ToList();
            mails.Sort(new MailsComparator());
            _curentAccount.Mails = mails;

            //Если слушатель не работает - включить
            if (!_backgroundListener.IsAlive)
                _backgroundListener.Start();
        }

        public string GetCurentAccountEmail()
        {
            return _curentAccount == null ? "" : _curentAccount.MailAddress;
        }

        public IEnumerable<string> GetUsersAccountEmails(string login)
        {
            return _dBprovider.GetUsersAccounts(login).Select(x => x.MailAddress);
        }

        //Выбрать первый акаунт из списка
        //!!!!!!!!!!!!!!!!!!!!!
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

                //-------------------------------------------------------------------------------------------
                _customiseAccount();
                //---------------------------------------------------------------------------------------------

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

                //Записываем ключи акаунта
                _cryptoProvider.NewRsaKeys();
                AsymmKey key = new AsymmKey
                {
                    D = Convert.ToBase64String(_cryptoProvider.RsaKeys.D),
                    E = Convert.ToBase64String(_cryptoProvider.RsaKeys.Exponent),
                    N = Convert.ToBase64String(_cryptoProvider.RsaKeys.Modulus),
                    DP = Convert.ToBase64String(_cryptoProvider.RsaKeys.DP),
                    DQ = Convert.ToBase64String(_cryptoProvider.RsaKeys.DQ),
                    InverseQ = Convert.ToBase64String(_cryptoProvider.RsaKeys.InverseQ),
                    P = Convert.ToBase64String(_cryptoProvider.RsaKeys.P),
                    Q = Convert.ToBase64String(_cryptoProvider.RsaKeys.Q),
                    AccountOwner = account
                };

                //!!!!!new
                _cryptoProvider.NewDsaKeys();
                SignKey sign = new SignKey
                {
                    Counter = _cryptoProvider.DsaKeys.Counter,
                    G = Convert.ToBase64String(_cryptoProvider.DsaKeys.G),
                    J = Convert.ToBase64String(_cryptoProvider.DsaKeys.J),
                    P = Convert.ToBase64String(_cryptoProvider.DsaKeys.P),
                    Q = Convert.ToBase64String(_cryptoProvider.DsaKeys.Q),
                    Seed = Convert.ToBase64String(_cryptoProvider.DsaKeys.Seed),
                    X = Convert.ToBase64String(_cryptoProvider.DsaKeys.X),
                    Y = Convert.ToBase64String(_cryptoProvider.DsaKeys.Y),
                    AccountOwner = account
                };

                //Записываем ключи в базу
                _dBprovider.SaveObject(key);
                _dBprovider.SaveObject(sign);

                if (account.Key == null)
                {
                    account.Key = new List<AsymmKey>{key};
//                    account.Key.ToList().Add(key);
                }

                //!!!new
                if (account.Sign == null)
                {
                    account.Sign = new List<SignKey> {sign};
                }

                //Если акаунта не было, устанавливаем его
                if (_curentAccount == null)
                {
                    _curentAccount = account;
                    _curentAccount.Mails = new List<Mail>();

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
                Path.Combine(_curentAccount.LocalPath, Enum.GetName(typeof (State), state)),
                uids
                );

            return mails;
        }

        //!!!!
        private void _listenOnMails()
        {
            var uids = _curentAccount.Mails.Select(m => m.Uid).ToArray();

            try
            {
                foreach (Message_obj message in _mailResiever.GetIncomingMails(uids))
                {
                    message.Text = message.Text.Replace("\r\n", "");
                    if (message.KeyLength > 0)
                    {
                        string symm_key = message.Text.Substring(0, message.KeyLength/2);
                        string symm_iv = message.Text.Substring(message.KeyLength/2, message.KeyLength/2);
                        message.Text = message.Text.Remove(0, message.KeyLength);

                        message.Subject = _cryptoProvider.DecryptData(
                            Convert.FromBase64String(message.Subject),
                            Convert.FromBase64String(symm_key),
                            Convert.FromBase64String(symm_iv));

                        message.Text = _cryptoProvider.DecryptData(
                            Convert.FromBase64String(message.Text),
                            Convert.FromBase64String(symm_key),
                            Convert.FromBase64String(symm_iv));
                    }
                    else
                    {
                        message.Text = message.Text.Replace("\r\n", "");
                    }

                    //если подписано, проверяем на целостность
                    if (!String.IsNullOrEmpty(message.Sign))
                    {
                        byte[] bodyHash = _cryptoProvider.ComputHash(
                            Encoding.ASCII.GetBytes(message.Text));

                        var pubKey = _dBprovider.GetSignKey(message.From);

                        bool verify = _cryptoProvider.VerifyMail(Convert.FromBase64String(message.Sign), bodyHash, pubKey);

                        //Испорченое сообщение пропускаем
                        if (!verify)
                            continue;
                    }

                    _saveIncomingMessage(message);
                }
            }
            catch (Exception ex)
            {
                //Надо доделать обработку, но если нет соединения с pop, то пропускаем мимо ушей
            }
        }

        private void _saveIncomingMessage(Message_obj message)
        {
            try
            {
                //Если Uid null, то задаем вручную. Чистим от ненужных знаков
                message.Uid = message.Uid ?? _createUid();
                message.Uid = message.Uid.CleanString();

                //А еще на диск сохрани!
                _driveProvider.SaveMessage(
                    Path.Combine(_curentAccount.LocalPath, Enum.GetName(typeof(State), State.Incoming)),
                    message);


                //Создаем письмо для БД
                Mail m = new Mail
                {
                    MailAccount = _curentAccount,
                    Uid = message.Uid,
                    MailState = State.Incoming,
                    Date = message.Date,
                    Attachments = new List<Attachment>()
                };

                //Сохраняем в базе письмо
                _dBprovider.SaveObject(m);

                List<Mail> temp = _curentAccount.Mails.ToList();
                temp.Insert(0, m);
                _curentAccount.Mails = temp;

                //Аттачи для БД
                if (message.Attachments != null)
                {
                    foreach (var attach in message.Attachments)
                    {
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

                //Добавляем содержимое письма к массиву в программе
                lock (_curentAccount.MailAddress)
                {
                    _localMessages.Insert(0, message);
                }
            }
            catch (Exception ex)
            {
                //!!!
                throw ex;
            }
        }

        //Включить слушатель писем
        private void _startListen()
        {
            do
            {
                _listenOnMails();

                Thread.Sleep(60000);
            } while (true);
        }

        //Получить письма заданного типа
        public IEnumerable<Message_obj> GetMessages(State type)
        {
            if (_curentAccount == null)
                return new List<Message_obj>();

            var uids = _curentAccount.Mails.Where(m => m.MailState == type).Select(m => m.Uid);

            lock (_curentAccount.MailAddress)
            {
                var messages = _localMessages.Where(m => uids.Any(arg => arg == m.Uid)).Select(m => m).ToList();
                return messages;
            }
        }
 
        //----------------------------------------------------------------------------------------------
        //Отправка
        //----------------------------------------------------------------------------------------------
        //Переделать для нескольких отправителей и добавить вложения
        public Mail SendMessage(string text, string subject, string to, bool needSign)
        {
            try
            {
                if (needSign)
                {
                    //Добавить подпись
                    //------------------------------------------------------
                    string sign = _cryptoProvider.SignMail(Encoding.ASCII.GetBytes(text));
                    _mailSender.AddAditionalHeader("Sign", sign);
                    //------------------------------------------------------
                }

                //Отправить письмо
                _mailSender.CreateMessage(text, false, subject);
                _mailSender.AddReceivers(to);
                _mailSender.SendMessage();

                var nmail = _saveOutgoingMessage(text, subject, to);

                return nmail;
            }
            catch (Exception ex)
            {
                //!!!!
//                throw ex;
                return null;
            }
        }

        private Mail _saveOutgoingMessage(string text, string subject, string to)
        {
            //Задаем uid
            var uidStr = _createUid();

            //Создание объекта программы
            Message_obj nmess = new Message_obj
            {
                Date = DateTime.Now.ToLongDateString(),
                From = _curentAccount.MailAddress,
                To = to,
                Text = text,
                Subject = subject,
                Uid = uidStr
            };

            //Добавить в список писем
            lock (_curentAccount.MailAddress)
            {
                _localMessages.Insert(0, nmess);
            }

            //Сохранить на диск
            _driveProvider.SaveMessage(
                Path.Combine(_curentAccount.LocalPath, Enum.GetName(typeof (State), State.Outgoing)),
                nmess);

            //Записываем в БД
            Mail nmail = new Mail
            {
                MailAccount = _curentAccount,
                Uid = uidStr,
                MailState = State.Outgoing,
                Date = nmess.Date,
                Key = new List<SymmKey>()
            };

            _dBprovider.SaveObject(nmail);

            lock (_curentAccount.MailAddress)
            {
                List<Mail> temp = _curentAccount.Mails.ToList();
                temp.Insert(0, nmail);
                _curentAccount.Mails = temp;
            }

            return nmail;
        }

        private string _createUid()
        {
            byte[] uid = new byte[60];
            new Random().NextBytes(uid);
            string uidStr = Convert.ToBase64String(uid);

            uidStr = uidStr.CleanString();
            return uidStr;
        }

        //Отправка шифрованного сообщения
        public bool SendEncryptedMessage(string text, string subject, string to, bool needSign)
        {
//            string ntext = _cryptoProvider.EncrytpData(Encoding.ASCII.GetBytes(text));
            string ntext = _cryptoProvider.EncrytpData(Encoding.UTF8.GetBytes(text));
            string nsubject = _cryptoProvider.EncrytpData(Encoding.UTF8.GetBytes(subject));

            string Key = Convert.ToBase64String(_cryptoProvider.Des.Key);
            string IV = Convert.ToBase64String(_cryptoProvider.Des.IV);


            AsymmKey key = _dBprovider.GetAsymmKey(to);

            if (key == null)
                return false;

            //Сюда передать открытый ключ получаетля!!!!!
            string symmKeys = _cryptoProvider.GetEncryptedSymmKey(key);

            _mailSender.AddAditionalHeader("KeyLen", symmKeys.Length.ToString());

            if (needSign)
            {
                //Добавить подпись
                //------------------------------------------------------
                string sign = _cryptoProvider.SignMail(Encoding.ASCII.GetBytes(text));
                _mailSender.AddAditionalHeader("Sign", sign);
                //------------------------------------------------------
            }

            symmKeys += ntext;

            var nmail = SendMessage(symmKeys, nsubject, to, false);

            //Сохранение ключей
            SymmKey nsymm = new SymmKey
            {
                CipherKey = Key,
                IV = IV,
                EncryptedMail = nmail
            };

            _dBprovider.SaveObject(nsymm);

            nmail.Key.ToList().Add(nsymm);

            return true;
        }

        public Message_obj GetMessage(int index, State type)
        {
            var mails = _curentAccount.Mails.Where(m => m.MailState == type).Select(m => m.Uid).ToList();

            string uid = "";
            if (mails.Count != 0)
            {
                uid = mails[index];
            }

            lock (_curentAccount.MailAddress)
            {
                return _localMessages.Where(m => m.Uid == uid).Select(m => m).FirstOrDefault();
            }
        }
    }
}