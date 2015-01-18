using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyMail.Models.DBmanager;
using MyMail.Models.DriveManager;
using MyMail.Models.Entities;

namespace MyMail.Models
{
    public class ServiceManager : IServiceManager
    {
        private IDBprovider _dBprovider;
        private IDriveAccesProvider _driveProvider;

        private Account _curentAccount;

        public ServiceManager(IDBprovider providerParam, IDriveAccesProvider driveProvider_param)
        {
            _dBprovider = providerParam;
            _driveProvider = driveProvider_param;
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

        public bool TrySetCurentAcount(string login)
        {
            var accounts = _dBprovider.GetUsersAccounts(login).ToArray();

            if (accounts.Length == 0)
            {
                return false;
            }
            else
            {
                _curentAccount = accounts.First();
                return true;
            }
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
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}