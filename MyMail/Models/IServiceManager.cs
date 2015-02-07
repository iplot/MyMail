using System.Collections.Generic;
using MyMail.Models.Entities;
using NetWork.MailReciever;

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
        IEnumerable<Message_obj> GetMessages(State type);
        Mail SendMessage(string text, string subject, string to, bool needSign);
        void SendEncryptedMessage(string text, string subject, string to, bool needSign);
        Message_obj GetMessage(int index, State type);
    }
}