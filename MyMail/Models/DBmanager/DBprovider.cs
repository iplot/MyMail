﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyMail.Models.Entities;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Linq;

namespace MyMail.Models.DBmanager
{
    public class DBprovider : IDBprovider
    {
        private ISessionFactory _sessionFactory;

        public DBprovider()
        {
            var configuration = new Configuration();
            configuration.Configure();

            _sessionFactory = configuration.BuildSessionFactory();
        }

        public User GetUser(string login)
        {
            using (var session = _sessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    User[] user = 
                        (from us in session.Query<User>() where us.Login.Equals(login) select us).ToArray();

                    return user.Length == 0 ? null : user.First();
                }
            }
        }

        public Account GetAccount(string email)
        {
            using (var session = _sessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    Account[] account =
                        (from acc in session.Query<Account>() where acc.MailAddress == email select acc).ToArray();

                    return account.Length == 0 ? null : account.First();
                }
            }
        }

        public IEnumerable<Account> GetUsersAccounts(string login)
        {
            using (var session = _sessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    return (from acc in session.Query<Account>() where acc.AccountUser.Login == login select acc).ToArray();
                }
            }
        } 

        public void SaveObject(Object user)
        {
            using (var session = _sessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    session.Save(user);
                    transaction.Commit();
                }
            }
        }
    }
}