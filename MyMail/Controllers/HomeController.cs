using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MyMail.Infrastructure;
using MyMail.Models;
using MyMail.Models.DBmanager;

namespace MyMail.Controllers
{
    [MymailAuthorization]
    public class HomeController : Controller
    {
        private IServiceManager _serviceManager;

        // GET: Home
        public ActionResult Index()
        {
            _serviceManager = (IServiceManager) System.Web.HttpContext.Current.Session["ServiceManager"];

            //Если не логинился - залогинься
            if (_serviceManager == null && System.Web.HttpContext.Current.Session["Login"] == null)
                return RedirectToAction("Login", "Account");

            //Если аккаунт не выбран пробуем установить первый из списка
            if (_serviceManager.GetCurentAccountEmail() == "")
                _serviceManager.TrySetCurentAcount((string)System.Web.HttpContext.Current.Session["Login"]);

            ViewBag.Email = _serviceManager.GetCurentAccountEmail();

            return View();
        }
    }
}