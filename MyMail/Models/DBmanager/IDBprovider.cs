using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyMail.Models.Entities;

namespace MyMail.Models.DBmanager
{
    public interface IDBprovider
    {
        Task<User> GetUser(string login);
        Task<Account> GetAccount(string email);
        Task SaveObject(Object obj);
        Task<IEnumerable<Account>> GetUsersAccounts(string login);
        Task<AsymmKey> GetAsymmKey(string email);
        Task<SignKey> GetSignKey(string email);
    }
}