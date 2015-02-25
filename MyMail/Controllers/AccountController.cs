using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        //Проверки на уникальность поля можно вынести на модели!

        // GET: Account
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(User user)
        {
            if (!ModelState.IsValid)
                return View();

            if (await _provider.IsUserPresent(user.Login, user.Password))
            {
                FormsAuthentication.SetAuthCookie(user.Login, false);
                System.Web.HttpContext.Current.Session["Login"] = user.Login;

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "There are no such user in the system. Please, repeat or SignUp");
                return RedirectToAction("Login");
            }
        }

        public ActionResult SignUp()
        {
            return View();
        }

        //Заменить параметры на модель и наложить Validation на свойства (а может и ненадо, решу еще)
        [HttpPost]
        public async Task<ActionResult> SignUp(User user)
        {
            if (!ModelState.IsValid)
                return View();

            //Если логин уже есть в системе, ничего не делать и вернуть пользователя обратно к форме
            if(await _provider.AddUser(user))
            {
                System.Web.HttpContext.Current.Session["Login"] = user.Login;

                FormsAuthentication.SetAuthCookie(user.Login, false);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Login is already exists");
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
        public async Task<ActionResult> AddServerAccount(Account acc)
        {
            if (!ModelState.IsValid)
                return View();

            if (await _provider.AddAccount(acc, (string) System.Web.HttpContext.Current.Session["Login"]))
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