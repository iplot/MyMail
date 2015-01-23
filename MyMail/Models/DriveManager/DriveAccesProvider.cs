using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using MyMail.Models.Entities;
using NetWork.MailReciever;

namespace MyMail.Models.DriveManager
{
    public class DriveAccesProvider : IDriveAccesProvider
    {
        private readonly string _folderPath;

        public DriveAccesProvider(string rootFolderPath)
        {
            _folderPath = rootFolderPath;

            if(!Directory.Exists(_folderPath))
                Directory.CreateDirectory(_folderPath);
        }

        public string addAccountFolder(string accountEmail)
        {
            string path = Path.Combine(_folderPath, accountEmail);

            IEnumerable<string> states = Enum.GetNames(typeof (State));

            foreach (string state in states)
            {
                Directory.CreateDirectory(Path.Combine(path, state));
            }

            return path;
        }

        //Пока без атачей
        public IEnumerable<Message_obj> getSavedMessages(string folderPath, IEnumerable<string> uids)
        {
//            string realPath = Path.Combine(_folderPath, folderPath);

            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
            List<Message_obj> messages = new List<Message_obj>();

            foreach (string uid in uids.ToArray())
            {
                //Папачка письма
                DirectoryInfo messageDirectory = new DirectoryInfo(
                    Path.Combine(folderPath, uid)
                    );

                FileInfo file = null;
                FileInfo[] files = messageDirectory.GetFiles()
                    .Where(p => p.Name == string.Format("{0}.xml", uid))
                    .Select(p => p).ToArray();

                //Если письмо в базе есть, а физически его нет, то пока просто пропустить его
                //По хорошему надо как то обрабатывать
                if(files.Length == 0)
                    continue;
                else
                {
                    file = files.First();
                }

                StreamReader xmlReader = new StreamReader(file.FullName);

//                StringReader stringReader = new StringReader(xmlReader.ReadToEnd());

                XmlSerializer serializer = new XmlSerializer(typeof(Message_obj));

                Message_obj message = (Message_obj) serializer.Deserialize(xmlReader);

                //Добавить загрузку аттачей

                messages.Add(message);
            }

            return messages;
        }

        public void SaveMessage(string path, Message_obj message)
        {
            path = Path.Combine(path, message.Uid);
            Directory.CreateDirectory(path);

            if (message.Attachments != null)
            {
                byte[] temp = new byte[2000];

                //Записываем все атачи, если они есть
                foreach (NetWork.MailReciever.Attachment attach in message.Attachments)
                {
                    FileInfo fileName = new FileInfo(Path.Combine(path, attach.Name));

                    FileStream writer = new FileStream(fileName.FullName, FileMode.Create);

                    int readBytes = 0;

                    attach.Data.Position = 0;

                    do
                    {
                        readBytes = attach.Data.Read(temp, 0, temp.Length);
                        writer.Write(temp, 0, readBytes);

                    } while (readBytes != 0);

                    writer.Close();
                }
            }

            //Сериализуем письмо
            XmlSerializer serializer = new XmlSerializer(typeof(Message_obj));
            StringWriter stringWriter = new StringWriter();

            serializer.Serialize(stringWriter, message);
            stringWriter.Close();

            //Записываем письмо
            StreamWriter mailWriter = new StreamWriter(Path.Combine(path, message.Uid) + ".xml");
            mailWriter.Write(stringWriter.ToString());
            mailWriter.Close();
        }
    }
}