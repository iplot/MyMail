﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task<ActionResult> Index()
        {
            //Если не логинился - залогинься
            if (_serviceManager == null || System.Web.HttpContext.Current.Session["Login"] == null)
                return RedirectToAction("Login", "Account");

            //Если аккаунт не выбран пробуем установить первый из списка
            if (_serviceManager.GetCurentAccountEmail() == "")
                await _serviceManager.TrySetCurentAcount((string)System.Web.HttpContext.Current.Session["Login"]);

            ViewBag.Email = _serviceManager.GetCurentAccountEmail();

            return View();
        }

        public ActionResult GetMails(State mailsType)
        {
            if (Request.IsAjaxRequest())
            {
                return Json(new
                {
                    State = Enum.GetName(typeof (State), mailsType),
                    Mails = _serviceManager.GetMessages(mailsType)
                        .Select(m => new{Email = (mailsType == State.Incoming ? m.From : m.To), Subject = m.Subject, Date = m.Date})
                }, "application/json", JsonRequestBehavior.AllowGet);
            }
            else
            {
                ViewBag.State = Enum.GetName(typeof(State), mailsType);

                return PartialView(_serviceManager.GetMessages(mailsType));
            }
            
        }

        public PartialViewResult PrepareSend()
        {
            return new PartialViewResult();
        }

        public async Task<string> SendMessage(string to, string text, bool needSign, string subject = "")
        {
            if (await _serviceManager.SendMessage(text, subject, to, needSign) != null)
            {
                return "Message has been sent";
            }
            else
            {
                return "Message can not be send";
            }
        }

        public async Task<string> SendEncryptedMessage(string to, string text, bool needSign, string subject = "")
        {
            if (await _serviceManager.SendEncryptedMessage(text, subject, to, needSign))
            {
                return "Message has been sent";
            }
            else
            {
                return "Message can not be send";
            }
        }

        public PartialViewResult GetMessage(int index, string type)
        {
            ViewBag.Index = index;
            ViewBag.Type = type;

            var message = _getMessage(index, type);

            return PartialView(message);
        }

        private Message_obj _getMessage(int index, string type)
        {
            State mailType = (State) Enum.Parse(typeof (State), type);

            Message_obj message = _serviceManager.GetMessage(index - 1, mailType);

            return message;
        }

        public FileResult GetAttachment(int index, string type, string name)
        {
            var message = _getMessage(index, type);

            var attach = message.Attachments.Where(att => att.Name == name).Select(att => att).FirstOrDefault();

            byte[] data = (attach.Data as MemoryStream).ToArray();

            return File(data, "application/octet-stream", attach.Name);
        }
    }
}