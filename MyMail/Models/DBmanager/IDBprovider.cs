using System;
using System.Collections.Generic;
using MyMail.Models.Entities;

namespace MyMail.Models.DBmanager
{
    public interface IDBprovider
    {
        User GetUser(string login);
        Account GetAccount(string email);
        void SaveObject(Object user);
        IEnumerable<Account> GetUsersAccounts(string login);
    }
}