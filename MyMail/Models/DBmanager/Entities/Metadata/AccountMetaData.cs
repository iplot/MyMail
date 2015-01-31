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
        public virtual string SmtpServerHost { get; set; }

        [Required]
        public virtual int SmtpServerPort { get; set; }

        [Required]
        public virtual string Pop3ServerHost { get; set; }

        [Required]
        public virtual int Pop3ServerPort { get; set; }

        [Required]
        [UIHint("EmailAddress")]
        public virtual string MailAddress { get; set; }

        [Required]
        [UIHint("Password")]
        public virtual string MailPassword { get; set; }
    }
}