using Microsoft.AspNetCore.Mvc;
using project.Models;
using System.Linq;
using System.Data.SqlClient;

namespace project.Controllers
{
    public class StudentAccountController : Controller
    {
        private readonly StudentModel _studentModel;
        private readonly string connStr;

        // 用 DI 注入 StudentModel
        public StudentAccountController(StudentModel studentModel, IConfiguration configuration)
        {
            _studentModel = studentModel;
            _studentModel = studentModel;
            connStr = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult register() => View();

        [HttpPost]
        public IActionResult register(adminStudent model)
        {
            if (ModelState.IsValid)
            {
                // 檢查帳號是否重複
                var exists = _studentModel.getadminStudents()
                    .Any(t => t.UserName == model.UserName);
                if (exists)
                {
                    ModelState.AddModelError("UserName", "此帳號已存在");
                    return View(model);
                }

                var result = _studentModel.InsertStudent(model);
                return RedirectToAction("memberLogin");
            }
            return View(model);
        }

        public IActionResult memberLogin() => View();

        [HttpPost]
        public IActionResult memberLogin(string UserName, string Password)
        {
            var student = _studentModel.getadminStudents()
                .FirstOrDefault(t => t.UserName == UserName && t.Password == Password);

            if (student != null)
            {
                HttpContext.Session.SetString("UserName", student.UserName);
                HttpContext.Session.SetString("Identity", "Student");
                HttpContext.Session.SetString("StudentId", student.ID.ToString());
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string sql = "UPDATE Student SET LastLoginTime = @now WHERE ID = @id";
                    using var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@now", DateTime.Now);
                    cmd.Parameters.AddWithValue("@id", student.ID); // 根據你的程式寫法調整欄位
                    cmd.ExecuteNonQuery();
                }
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
