using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MyMail.Infrastructure;
using MyMail.Models.DBmanager;

namespace MyMail.Controllers
{
    [MymailAuthorization]
    public class HomeController : Controller
    {
        
        // GET: Home
        public string Index()
        {
            return "Hi!";
        }
    }
}