using System.Collections.Generic;

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
    }
}