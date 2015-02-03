using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MyMail.Infrastructure;
using MyMail.Models;
using MyMail.Models.DBmanager;
using MyMail.Models.Entities;
using NetWork.MailReciever;

namespace MyMail.Controllers
{
    [MymailAuthorization]
    public class HomeController : Controller
    {
        private IServiceManager _serviceManager;
        private List<int> a = new List<int>();

        public HomeController()
        {
            _serviceManager = (IServiceManager)System.Web.HttpContext.Current.Session["ServiceManager"];
        }

        // GET: Home
        public ActionResult Index()
        {
            //Если не логинился - залогинься
            if (_serviceManager == null || System.Web.HttpContext.Current.Session["Login"] == null)
                return RedirectToAction("Login", "Account");

            //Если аккаунт не выбран пробуем установить первый из списка
            if (_serviceManager.GetCurentAccountEmail() == "")
                _serviceManager.TrySetCurentAcount((string)System.Web.HttpContext.Current.Session["Login"]);

            ViewBag.Email = _serviceManager.GetCurentAccountEmail();

            return View();
        }

        public PartialViewResult GetMails(State mailsType)
        {
            ViewBag.State = Enum.GetName(typeof (State), mailsType);

            return PartialView(_serviceManager.GetMessages(mailsType));
        }

        public PartialViewResult PrepareSend()
        {
            return new PartialViewResult();
        }

        public string SendMessage(string to, string text, string subject = "")
        {
            _serviceManager.SendMessage(text, subject, to);

            return "Message has been sent";
        }

        public string TestSend(string text = "test text", string subject = "Subject",
            string to = "iplotnikov94@gmail.com")
        {
            _serviceManager.SendEncryptedMessage(text, subject, to);

            return "ready";
        }

        public PartialViewResult GetMessage(int index, string type)
        {
            State mailType = (State) Enum.Parse(typeof (State), type);

            Message_obj message = _serviceManager.GetMessage(index - 1, mailType);

            return PartialView(message);
        }
    }
}