using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Type;

namespace MyMail.Models.Entities
{
    public class Mail
    {
        public virtual int Id { get; set; }

        public virtual string Uid { get; set; }

        public virtual State MailState { get; set; }

        public virtual IEnumerable<Attachment> Attachments { get; set; }

        //Для записи
        public virtual Account MailAccount { get; set; }
    }

    public enum State
    {
        Incoming,
        Outgoing
    }

    public class StateType : EnumStringType<State>
    {

    }
}
