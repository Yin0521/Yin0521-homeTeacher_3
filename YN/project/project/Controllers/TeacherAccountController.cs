using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using project.Models;
using project.Models.Services;
using System.Data.SqlClient;
using project.Models.Teacher;

namespace project.Controllers
{
    public class TeacherAccountController : Controller
    {
        private readonly TeacherService _teacherService;
        private readonly TeacherModel _teacherModel;
        private readonly string connStr;

        public TeacherAccountController(TeacherService teacherService, TeacherModel teacherModel, IConfiguration configuration)
        {
            _teacherService = teacherService;
            _teacherModel = teacherModel;
            connStr = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult register()
        {
            ViewBag.Subjects = _teacherService.GetAllSubjects();
            return View(new adminTeacher());
        }

        [HttpPost]
        public IActionResult register(adminTeacher model, List<int> SubjectIDs)
        {
            ViewBag.Subjects = _teacherService.GetAllSubjects();

            if (ModelState.IsValid)
            {
                // 檢查帳號是否重複
                var exists = _teacherModel.getadminTeachers()
                    .Any(t => t.UserName == model.UserName);
                if (exists)
                {
                    ModelState.AddModelError("UserName", "此帳號已存在");
                    return View(model);
                }

                // 只呼叫一次，拿到新老師的 ID
                var newTeacherId = _teacherModel.InsertTeacher(model);

                // 寫入科目關聯
                if (SubjectIDs != null && SubjectIDs.Count > 0)
                {
                    _teacherService.SaveTeacherSubjects(newTeacherId, SubjectIDs);
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
            var teacher = _teacherModel.getadminTeachers()
                .FirstOrDefault(t => t.UserName == UserName && t.Password == Password);

            if (teacher != null)
            {
                HttpContext.Session.SetString("UserName", teacher.UserName);
                HttpContext.Session.SetString("Identity", "Teacher");
                HttpContext.Session.SetString("TeacherId", teacher.ID.ToString());
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string sql = "UPDATE Teacher SET LastLoginTime = @now WHERE ID = @id";
                    using var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@now", DateTime.Now);
                    cmd.Parameters.AddWithValue("@id", teacher.ID);
                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction("teacher", "Home"); // 或跳轉首頁
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
