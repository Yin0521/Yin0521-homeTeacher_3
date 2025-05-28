using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using project.Models;
using project.Models.Services;

namespace project.Controllers
{
    public class TeacherAccountController : Controller
    {


        public IActionResult register()
        {
            var service = new TeacherService();
            ViewBag.Subjects = service.GetAllSubjects();
            return View(new adminTeacher());
        }

        [HttpPost]
        public IActionResult register(adminTeacher model, List<int> SubjectIDs)
        {
            var service = new TeacherService();
            ViewBag.Subjects = service.GetAllSubjects();

            if (ModelState.IsValid)
            {
                var teacherModel = new TeacherModel();

                // 檢查帳號是否重複
                var exists = teacherModel.getadminTeachers()
                    .Any(t => t.UserName == model.UserName);
                if (exists)
                {
                    ModelState.AddModelError("UserName", "此帳號已存在");
                    return View(model);
                }

                // 只呼叫一次，拿到新老師的 ID
                var newTeacherId = teacherModel.InsertTeacher(model);

                // 寫入科目關聯
                if (SubjectIDs != null && SubjectIDs.Count > 0)
                {
                    service.SaveTeacherSubjects(newTeacherId, SubjectIDs);
                }

                return RedirectToAction("memberLogin");
            }

            // 驗證沒過，回填表單
            return View(model);
        }

        public IActionResult memberLogin() => View();
        [HttpPost]
        public IActionResult memberLogin(string UserName, string Password)
        {
            TeacherModel teacherModel = new TeacherModel();
            var teacher = teacherModel.getadminTeachers()
                .FirstOrDefault(t => t.UserName == UserName && t.Password == Password);

            if (teacher != null)
            {
                HttpContext.Session.SetString("UserName", teacher.UserName);
                HttpContext.Session.SetString("Identity", "Teacher");
                HttpContext.Session.SetString("TeacherId", teacher.ID.ToString());
                return RedirectToAction("teacher","Home"); // 或跳轉首頁
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
