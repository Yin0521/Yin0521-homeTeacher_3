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
                HttpContext.Session.SetString("AdminRole", user.role); // 在登入成功時
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

        public IActionResult Manager()
        {
            if (HttpContext.Session.GetInt32("AdminID") == null)
                return RedirectToAction("Login");

            adminLoginModel loginModel = new adminLoginModel();
            var accounts = loginModel.getadminAccounts();

            ViewBag.Role = HttpContext.Session.GetString("AdminRole");

            return View(accounts);
        }

        [HttpGet]
        public IActionResult CreateAdmin() => View();

        [HttpPost]
        public IActionResult CreateAdmin(adminAccount model)
        {
            if (ModelState.IsValid)
            {
                // 加入 DB 邏輯
                var result = new adminLoginModel().InsertAdmin(model);
                return RedirectToAction("Manager");
            }
            return View(model);
        }
        public IActionResult AdminDelete(int id)
        {
            var model = new adminLoginModel();
            model.DeleteAdmin(id);
            return RedirectToAction("Manager");
        }

        [HttpGet]
        public IActionResult AdminEdit(int id)
        {
            var model = new adminLoginModel();
            var admin = model.getadminAccounts().FirstOrDefault(a => a.ID == id);

            if (admin == null)
                return NotFound();

            return View(admin); // 傳送單筆資料給 View
        }

        [HttpPost]
        public IActionResult AdminEdit(adminAccount updated)
        {
            
            if (!ModelState.IsValid)
                return View(updated);

            var model = new adminLoginModel();
            model.UpdateAdmin(updated);
            return RedirectToAction("Manager");
        }

        public IActionResult Teacher()
        {
            TeacherModel teacherModel = new TeacherModel();
            var teachers = teacherModel.getadminTeachers();

            ViewBag.Role = HttpContext.Session.GetString("AdminRole");
            return View(teachers);
            
        }

        [HttpGet]
        public IActionResult TeacherEdit(int id)
        {
            var model = new TeacherModel();
            var teacher = model.getadminTeachers().FirstOrDefault(a => a.ID == id);

            if (teacher == null)
                return NotFound();

            return View(teacher); // 傳送單筆資料給 View
        }

        [HttpPost]
        public IActionResult TeacherEdit(adminTeacher updated)
        {
            if (!ModelState.IsValid)
                return View(updated);

            var model = new TeacherModel();
            model.UpdateTeacher(updated);
            return RedirectToAction("Teacher");
        }

        public IActionResult CreateTeacher() => View();

        [HttpPost]
        public IActionResult CreateTeacher(adminTeacher model)
        {
            if (ModelState.IsValid)
            {
                // 加入 DB 邏輯
                var result = new TeacherModel().InsertTeacher(model);
                return RedirectToAction("Teacher");
            }
            return View(model);
        }
        public IActionResult TeacherDelete(int id)
        {
            var model = new TeacherModel();
            model.DeleteTeacher(id);
            return RedirectToAction("Teacher");
        }

        public IActionResult Student()
        {
            return View();
        }
        public IActionResult StudentEdit()
        {
            return View();
        }

    }
}
