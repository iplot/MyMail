using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyMail.Models.Entities;

namespace MyMail.Models.Entities
{
    public class SignKey
    {
        public virtual int Id { get; set; }

        public virtual int Counter { get; set; }

        public virtual string G { get; set; }

        public virtual string J { get; set; }

        public virtual string P { get; set; }

        public virtual string Q { get; set; }

        public virtual string Seed { get; set; }

        public virtual string X { get; set; }

        public virtual string Y { get; set; }

        public virtual Account AccountOwner { get; set; }
    }
}