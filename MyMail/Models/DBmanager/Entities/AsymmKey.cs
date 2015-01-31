using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMail.Models.Entities
{
    public class AsymmKey
    {
        public virtual int Id { get; set; }

        public virtual string D { get; set; }

        public virtual string E { get; set; }

        public virtual string N { get; set; }

        public virtual string DP { get; set; }

        public virtual string DQ { get; set; }

        public virtual string InverseQ { get; set; }

        public virtual string P { get; set; }

        public virtual string Q { get; set; }

        public virtual Account AccountOwner { get; set; }
    }
}