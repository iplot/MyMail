using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMail.Models.Entities
{
    public class Attachment
    {
        public virtual int Id { get; set; }

        public virtual string FileName { get; set; }

        //Для записи
        public virtual Mail MailOwner { get; set; }
    }
}
