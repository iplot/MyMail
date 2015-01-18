using System;
using MyMail.Models.Entities;

namespace MyMail.Models.DBmanager
{
    public interface IDBprovider
    {
        User GetUser(string login);
        void SaveObject(Object user);
    }
}