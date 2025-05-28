

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using project.Models;
using Microsoft.CodeAnalysis.Scripting;
using NuGet.DependencyResolver;

namespace project.Controllers
{
    public class StudentAccountController : Controller
    {
        public IActionResult register() => View();
        [HttpPost]
        public IActionResult register(adminStudent model)
        {
            if (ModelState.IsValid)
            {
                var StudentModel = new StudentModel();

                // 檢查帳號是否重複
                var exists = StudentModel.getadminStudents()
                    .Any(t => t.UserName == model.UserName);
                if (exists)
                {
                    ModelState.AddModelError("UserName", "此帳號已存在");
                    return View(model);
                }

                var result = StudentModel.InsertStudent(model);
                return RedirectToAction("memberLogin");
            }
            return View(model);
        }

        public IActionResult memberLogin() => View();
        [HttpPost]
        public IActionResult memberLogin(string UserName, string Password)
        {
            StudentModel StudentModel = new StudentModel();
            var Student = StudentModel.getadminStudents()
                .FirstOrDefault(t => t.UserName == UserName && t.Password == Password);

            if (Student != null)
            {
                HttpContext.Session.SetString("UserName", Student.UserName);
                HttpContext.Session.SetString("Identity", "Student");
                HttpContext.Session.SetString("StudentId", Student.ID.ToString());
                return RedirectToAction("student", "Home"); // 或跳轉首頁
            }

            ViewBag.Error = "帳號或密碼錯誤";
            return View();
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("memberLogin");
        }
    }
}

        
    
