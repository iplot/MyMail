using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MyMail.Models.DBmanager;
using MyMail.Models.Entities;

namespace MyMail.Controllers
{
    public class AccountController : Controller
    {
        private readonly IDBprovider _provider;

        public AccountController(IDBprovider providerArg)
        {
            _provider = providerArg;
        }

        // GET: Account
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string login, string password)
        {
            var user = _provider.GetUser(login);

            if (user != null && user.Password == password)
            {
                FormsAuthentication.SetAuthCookie(login, false);
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
            try
            {
                User user = new User
                {
                    Login = login,
                    Password = password
                };

                _provider.SaveObject(user);

                FormsAuthentication.SetAuthCookie(login, false);

                return RedirectToAction("Index", "Home");
            }
            catch(Exception)
            {
                return View();
            }
        }
    }
}