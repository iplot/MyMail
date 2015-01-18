using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMail.Models.Entities
{
    public class Account
    {
        public virtual int Id { get; set; }

        public virtual string ServerHost { get; set; }

        public virtual int ServerPort { get; set; }

        public virtual string LocalPath { get; set; }

        public virtual string MailAddress { get; set; }

        public virtual string MailPassword { get; set; }

        public virtual IEnumerable<Mail> Mails { get; set; }

        //Надо для записи
        public virtual User AccountUser { get; set; }
    }
}
