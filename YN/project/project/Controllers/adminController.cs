using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using project.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace project.Controllers
{
    public class adminController : Controller
    {
        
        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("AdminID") == null)
                return RedirectToAction("Login");

            ViewBag.AdminUsername = HttpContext.Session.GetString("AdminUsername");
            return View();
        }
        public IActionResult login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            adminLoginModel loginModel = new adminLoginModel();
            var accounts = loginModel.getadminAccounts();

            var user = accounts.FirstOrDefault(a => a.username == username && a.password == password);
            if (user != null)
            {
                HttpContext.Session.SetInt32("AdminID", user.ID);
                HttpContext.Session.SetString("AdminUsername", user.username);
                return RedirectToAction("Index");
            }

            ViewBag.Error = "帳號或密碼錯誤";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }


    }
}
