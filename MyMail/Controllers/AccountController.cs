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
        public ActionResult Login(User user)
        {
            if (ModelState.IsValid && _provider.IsUserPresent(user.Login, user.Password))
            {
                FormsAuthentication.SetAuthCookie(user.Login, false);
                System.Web.HttpContext.Current.Session["Login"] = user.Login;

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
        public ActionResult SignUp(User user)
        {
            //Если логин уже есть в системе, ничего не делать и вернуть пользователя обратно к форме
            if(ModelState.IsValid && _provider.AddUser(user))
            {
                System.Web.HttpContext.Current.Session["Login"] = user.Login;

                FormsAuthentication.SetAuthCookie(user.Login, false);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View();
            }
        }

        //------------------------------------------------------------
        //Server accounts
        //------------------------------------------------------------

        public ActionResult AddServerAccount()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddServerAccount(Account acc)
        {
            if (!ModelState.IsValid)
                return View();

            if (_provider.AddAccount(acc, (string) System.Web.HttpContext.Current.Session["Login"]))
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Such e-mail is already exists");
                return View();
            }
        }
    }
}