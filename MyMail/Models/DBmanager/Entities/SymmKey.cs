using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMail.Models.Entities
{
    public class SymmKey
    {
        public virtual int Id { get; set; }

        public virtual string CipherKey { get; set; }

        public virtual string IV { get; set; }

        public virtual Mail EncryptedMail { get; set; }
    }
}