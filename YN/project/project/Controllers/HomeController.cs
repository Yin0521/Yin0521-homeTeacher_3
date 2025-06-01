using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using project.Models;
using project.Models.Services;
using System.Data.SqlClient;
using Dapper;
using project.ViewModels;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITeacherService _teacherService;
        private readonly TeacherProfile _teacherProfile;

        private readonly IConfiguration _configuration;

        public HomeController(
            ILogger<HomeController> logger,
            ITeacherService teacherService,
            IConfiguration configuration,
            TeacherProfile teacherProfile)
        {
            _logger = logger;
            _teacherService = teacherService;
            _configuration = configuration;
            _teacherProfile = teacherProfile;
        }

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult TeacherDetail(int id)
        {
            string connStr = _configuration.GetConnectionString("DefaultConnection");
            using var conn = new SqlConnection(connStr);

            string sql = "SELECT * FROM Teacher WHERE ID = @id";
            var teacher = conn.QueryFirstOrDefault<adminTeacher>(sql, new { id });

            if (teacher == null)
            {
                return NotFound();
            }

            var service = new TeacherService();
            var subjectIds = service.GetTeacherSubjectIds(teacher.ID); // 取老師開放的科目ID
            var allSubjects = service.GetAllSubjects();
            var teacherSubjects = allSubjects.Where(s => subjectIds.Contains(s.Id)).ToList();

            

            var vm = new TeacherOrderViewModel
            {
                Teacher = teacher,
                Subjects = teacherSubjects
            };
            return View(vm);
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
            var subjectModel = new SubjectModel(_configuration);  // _configuration 用 DI 進來，或 new ConfigurationBuilder 也行
            var allSubjects = subjectModel.GetAllSubjects();

            var vm = new TeacherSearchViewModel
            {
                Subjects = allSubjects
            };

            // 傳遞推薦老師
            ViewBag.RecommendedTeachers = _teacherService.GetRecommendedTeachers();
            // 課程介紹
            ViewBag.SubjectList = _teacherService.GetAllSubjectsWithDescription();
            return View(vm);
        }

        [HttpGet]
        public IActionResult teacher()
        {
            var userName = HttpContext.Session.GetString("UserName");
            var vm = _teacherProfile.GetTeacherProfile(userName);
            if (vm == null)
            {
                TempData["ErrorMessage"] = "找不到教師資料，請重新登入。";
                return RedirectToAction("memberLogin", "TeacherAccount");
            }
            return View(vm);
        }

        [HttpPost]
        public IActionResult teacher(TeacherProfileViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.AllSubjects = _teacherProfile.GetAllSubjects();
                return View(vm);
            }
            _teacherProfile.UpdateTeacherProfile(vm);
            TempData["Success"] = "資料已更新";
            return RedirectToAction("teacher");
        }



    }
}
