using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MyMail.Models;
using MyMail.Models.DBmanager;
using MyMail.Models.Entities;

namespace MyMail.Controllers
{
    public class AccountController : Controller
    {
        private readonly IServiceManager _provider;

        public AccountController(IServiceManager providerArg)
        {
            _provider = providerArg;

            System.Web.HttpContext.Current.Session["ServiceManager"] = providerArg;
        }

        // GET: Account
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string login, string password)
        {
            if (_provider.IsUserPresent(login, password))
            {
                FormsAuthentication.SetAuthCookie(login, false);
                System.Web.HttpContext.Current.Session["Login"] = login;

                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        public ActionResult SignUp()
        {
            return View();
        }

        //Заменить параметры на модель и наложить Validation на свойства (а может и ненадо, решу еще)
        [HttpPost]
        public ActionResult SignUp(string login, string password)
        {
            //Если логин уже есть в системе, ничего не делать и вернуть пользователя обратно к форме
            if(_provider.AddUser(login, password))
            {
                System.Web.HttpContext.Current.Session["Login"] = login;

                FormsAuthentication.SetAuthCookie(login, false);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View();
            }
        }
    }
}