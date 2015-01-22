using System.Collections.Generic;
using NetWork.MailReciever;

namespace MyMail.Models.DriveManager
{
    public interface IDriveAccesProvider
    {
        string addAccountFolder(string accountEmail);
        IEnumerable<Message_obj> getSavedMessages(string folderPath, IEnumerable<string> uids);
        void SaveMessage(string path, Message_obj message);
    }
}