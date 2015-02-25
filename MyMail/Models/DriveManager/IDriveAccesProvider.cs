using System.Collections.Generic;
using System.Threading.Tasks;
using NetWork.MailReciever;

namespace MyMail.Models.DriveManager
{
    public interface IDriveAccesProvider
    {
        string addAccountFolder(string accountEmail);
        Task<IEnumerable<Message_obj>> getSavedMessages(string folderPath, IEnumerable<string> uids);
        Task SaveMessage(string path, Message_obj message);
    }
}