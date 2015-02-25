using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;
using MyMail.Infrastructure;
using MyMail.Models.Entities;
using NetWork.MailReciever;
using Attachment = NetWork.MailReciever.Attachment;

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

        public Task<IEnumerable<Message_obj>> getSavedMessages(string folderPath, IEnumerable<string> uids)
        {
            return Task.Factory.StartNew(() =>
            {
                DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
                List<Message_obj> messages = new List<Message_obj>();

//                foreach (string uid in uids.ToArray())
                object locker = new object();
                Parallel.ForEach(uids.ToArray(), uid =>
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
                    if (files.Length == 0)
//                        continue;
                        return;
                    else
                    {
                        file = files.First();
                    }

                    //Грузим из xml файла
                    var message = _loadXmlMessage(file.FullName);

                    //Загрузка атачей
                    FileInfo[] allFiles = messageDirectory.GetFiles();

                    //Если в папке толко письмо
                    if (allFiles.Length > 1)
                        message.Attachments = new List<Attachment>();

                    //Грузим каждый атач и добавляем к письму
                    foreach (FileInfo attach in allFiles)
                    {
                        //Если это письмо - пропускаем
                        if (attach.Name == string.Format("{0}.xml", message.Uid))
                            continue;

                        MemoryStream attachData = new MemoryStream(File.ReadAllBytes(attach.FullName));

                        message.Attachments.Add(new Attachment
                        {
                            Data = attachData,
                            Name = attach.Name
                        });
                    }

                    lock (locker)
                    {
                        messages.Add(message);
                    }
                });

                return messages.AsEnumerable();
            });
        }

        private Message_obj _loadXmlMessage(string fileName)
        {
            StreamReader xmlReader = new StreamReader(fileName);

            XmlSerializer serializer = new XmlSerializer(typeof (Message_obj));

            Message_obj message = (Message_obj) serializer.Deserialize(xmlReader);
            xmlReader.Close();

            return message;
        }

        public Task SaveMessage(string path, Message_obj message)
        {
            return Task.Factory.StartNew(() =>
            {
                path = Path.Combine(path, message.Uid);
                Directory.CreateDirectory(path);

                if (message.Attachments != null)
                {
                    byte[] temp = new byte[2000];

                    //Записываем все атачи, если они есть
//                    foreach (NetWork.MailReciever.Attachment attach in message.Attachments)
                    Parallel.ForEach(message.Attachments, attach =>
                    {
                        attach.Name = attach.Name.CleanString();

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
                    });
                }

                //Сериализуем и сохраняем письмо
                _saveXmlMessage(path, message);
            });
        }

        private void _saveXmlMessage(string path, Message_obj message)
        {
            XmlSerializer serializer = new XmlSerializer(typeof (Message_obj));
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