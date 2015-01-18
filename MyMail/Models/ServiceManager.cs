using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyMail.Models.DBmanager;
using MyMail.Models.Entities;

namespace MyMail.Models
{
    public class ServiceManager : IServiceManager
    {
        private IDBprovider _dBprovider;

        private Account _curentAccount;

        public ServiceManager(IDBprovider providerParam)
        {
            _dBprovider = providerParam;
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
    }
}