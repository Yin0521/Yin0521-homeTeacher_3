using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using project.Models;
using project.Models.Services;
using System.Data.SqlClient;
using Dapper;
using project.ViewModels;

namespace project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITeacherService _teacherService;

        public HomeController(ILogger<HomeController> logger, ITeacherService teacherService)
        {
            _logger = logger;
            _teacherService = teacherService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult teacher()
        {
            if (HttpContext.Session.GetString("Identity") != "Teacher")
            {
                TempData["ErrorMessage"] = "請先登入教師帳號才能進入此頁面。";
                return RedirectToAction("memberLogin", "TeacherAccount");
            }

            int teacherId = int.Parse(HttpContext.Session.GetString("TeacherId"));

            var vm = new TeacherSubjectViewModel
            {
                TeacherId = teacherId,
                AllSubjects = _teacherService.GetAllSubjects(),
                SelectedSubjectIds = _teacherService.GetTeacherSubjectIds(teacherId)
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult teacher(TeacherSubjectViewModel vm)
        {
            if (HttpContext.Session.GetString("Identity") != "Teacher")
            {
                TempData["ErrorMessage"] = "請先登入教師帳號才能進入此頁面。";
                return RedirectToAction("memberLogin", "TeacherAccount");
            }

            _teacherService.SaveTeacherSubjects(vm.TeacherId, vm.SelectedSubjectIds);
            TempData["Success"] = "已儲存科目";

            return RedirectToAction("teacher");
        }

        public IActionResult TeacherDetail(int id)
        {
            var connStr = "Data Source=(localdb)\\MSSQLLocalDB;Database=homeandteacher;Trusted_Connection=True";
            using var conn = new SqlConnection(connStr);

            string sql = "SELECT * FROM Teacher WHERE ID = @id";
            var teacher = conn.QueryFirstOrDefault<adminTeacher>(sql, new { id });

            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        [HttpGet("/api/teacher/search")]
        public IActionResult Search(string subject, string city)
        {
            var list = _teacherService.SearchTeachers(subject, city);
            return Json(list);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult student()
        {
            var identity = HttpContext.Session.GetString("Identity");
            if (string.IsNullOrEmpty(identity) || identity != "Student")
            {
                TempData["ErrorMessage"] = "請先登入學生帳號才能進入此頁面。";
                return RedirectToAction("memberLogin", "StudentAccount");
            }
            return View();
        }
    }
}
