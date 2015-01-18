using System.Collections.Generic;
using MyMail.Models.Entities;

namespace MyMail.Models
{
    public interface IServiceManager
    {
        bool IsUserPresent(string login, string password);
        bool AddUser(string login, string password);
        void ChangeAccount(string email);
        string GetCurentAccountEmail();
        IEnumerable<string> GetUsersAccountEmails(string login);
        bool TrySetCurentAcount(string login);
        bool AddAccount(Account account, string login);
        bool AddUser(User user);
    }
}