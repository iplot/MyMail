using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyMail.Models.Entities
{
    public partial class UserMetaData
    {
        [Required]
        public virtual string Login { get; set; }

        [Required]
        [UIHint("Password")]
        public virtual string Password { get; set; }
    }
}