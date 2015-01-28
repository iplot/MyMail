using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMail.Models.Entities
{
    [MetadataType(typeof(AccountMetaData))]
    public partial class Account
    {
        public virtual int Id { get; set; }

        public virtual string SmtpServerHost { get; set; }

        public virtual int SmtpServerPort { get; set; }

        public virtual string Pop3ServerHost { get; set; }

        public virtual int Pop3ServerPort { get; set; }

        public virtual string LocalPath { get; set; }

        public virtual string MailAddress { get; set; }

        public virtual string MailPassword { get; set; }

        public virtual IEnumerable<Mail> Mails { get; set; }

        //Он всего 1. Лучше пока не могу
        public virtual IEnumerable<AsymmKey> Key { get; set; } 

        //Надо для записи
        public virtual User AccountUser { get; set; }
    }
}
