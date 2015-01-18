using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyMail.Models.Entities
{
    public partial class AccountMetaData
    {
        [Required]
        public virtual string ServerHost { get; set; }

        [Required]
        public virtual int ServerPort { get; set; }

        [Required]
        public virtual string MailAddress { get; set; }

        [Required]
        [UIHint("Password")]
        public virtual string MailPassword { get; set; }
    }
}