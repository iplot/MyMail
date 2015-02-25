using System.Collections.Generic;
using System.Threading.Tasks;
using MyMail.Models.Entities;
using NetWork.MailReciever;

namespace MyMail.Models
{
    public interface IServiceManager
    {
        Task<bool> IsUserPresent(string login, string password);
        Task<bool> AddUser(string login, string password);
        void ChangeAccount(string email);
        string GetCurentAccountEmail();
        Task<IEnumerable<string>> GetUsersAccountEmails(string login);
        Task<bool> TrySetCurentAcount(string login);
        Task<bool> AddAccount(Account account, string login);
        Task<bool> AddUser(User user);
        IEnumerable<Message_obj> GetMessages(State type);
        Task<Mail> SendMessage(string text, string subject, string to, bool needSign);
        Task<bool> SendEncryptedMessage(string text, string subject, string to, bool needSign);
        Message_obj GetMessage(int index, State type);
    }
}