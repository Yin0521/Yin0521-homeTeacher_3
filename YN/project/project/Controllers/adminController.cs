using Microsoft.AspNetCore.Mvc;
using project.Models;
using System.Collections.Generic;
using System.Linq;

namespace project.Controllers
{
    public class adminController : Controller
    {
        private readonly adminLoginModel _adminLoginModel;
        private readonly TeacherModel _teacherModel;
        private readonly StudentModel _studentModel;
        private readonly SubjectModel _subjectModel;
        private readonly OrderService _orderService;
        private readonly AdminOrderService _adminOrderService;
        private readonly AdminOrderDetailService _adminOrderDetailService;
        private readonly DashboardService _dashboardService;


        // 建構子注入所有 Model
        public adminController(
            adminLoginModel adminLoginModel,
            TeacherModel teacherModel,
            StudentModel studentModel,
            SubjectModel subjectModel,
            OrderService orderService,
            AdminOrderService adminOrderService,
            AdminOrderDetailService adminOrderDetailService,
            DashboardService dashboardService
        )
        {
            _adminLoginModel = adminLoginModel;
            _teacherModel = teacherModel;
            _studentModel = studentModel;
            _subjectModel = subjectModel;
            _orderService = orderService;
            _adminOrderService = adminOrderService;
            _adminOrderDetailService = adminOrderDetailService;
            _dashboardService = dashboardService;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("AdminID") == null)
                return RedirectToAction("Login");
            DashboardStats stats = _dashboardService.GetDashboardStats();
            ViewBag.AdminUsername = HttpContext.Session.GetString("AdminUsername");
            return View(stats);
        }

        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var accounts = _adminLoginModel.getadminAccounts();
            var user = accounts.FirstOrDefault(a => a.username == username && a.password == password);
            if (user != null)
            {
                HttpContext.Session.SetInt32("AdminID", user.ID);
                HttpContext.Session.SetString("AdminUsername", user.username);
                HttpContext.Session.SetString("AdminRole", user.role);
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

            var accounts = _adminLoginModel.getadminAccounts();
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
                _adminLoginModel.InsertAdmin(model);
                return RedirectToAction("Manager");
            }
            return View(model);
        }

        public IActionResult AdminDelete(int id)
        {
            _adminLoginModel.DeleteAdmin(id);
            return RedirectToAction("Manager");
        }

        [HttpGet]
        public IActionResult AdminEdit(int id)
        {
            var admin = _adminLoginModel.getadminAccounts().FirstOrDefault(a => a.ID == id);
            if (admin == null)
                return NotFound();
            return View(admin);
        }

        [HttpPost]
        public IActionResult AdminEdit(adminAccount updated)
        {
            if (!ModelState.IsValid)
                return View(updated);
            _adminLoginModel.UpdateAdmin(updated);
            return RedirectToAction("Manager");
        }

        // ====== Teacher 部分 ======

        public IActionResult Teacher()
        {
            var teachers = _teacherModel.getadminTeachers();
            ViewBag.Role = HttpContext.Session.GetString("AdminRole");
            return View(teachers);
        }

        [HttpGet]
        public IActionResult TeacherEdit(int id)
        {
            var teacher = _teacherModel.getadminTeachers().FirstOrDefault(a => a.ID == id);
            if (teacher == null)
                return NotFound();
            return View(teacher);
        }

        [HttpPost]
        public IActionResult TeacherEdit(adminTeacher updated)
        {
            if (!ModelState.IsValid)
                return View(updated);
            _teacherModel.UpdateTeacher(updated);
            return RedirectToAction("Teacher");
        }

        public IActionResult CreateTeacher() => View();

        [HttpPost]
        public IActionResult CreateTeacher(adminTeacher model)
        {
            if (ModelState.IsValid)
            {
                _teacherModel.InsertTeacher(model);
                return RedirectToAction("Teacher");
            }
            return View(model);
        }

        public IActionResult TeacherDelete(int id)
        {
            _teacherModel.DeleteTeacher(id);
            return RedirectToAction("Teacher");
        }

        // ====== Student 部分 ======

        public IActionResult Student()
        {
            var students = _studentModel.getadminStudents();
            ViewBag.Role = HttpContext.Session.GetString("AdminRole");
            return View(students);
        }

        [HttpGet]
        public IActionResult StudentEdit(int id)
        {
            var student = _studentModel.getadminStudents().FirstOrDefault(a => a.ID == id);
            if (student == null)
                return NotFound();
            return View(student);
        }

        [HttpPost]
        public IActionResult StudentEdit(adminStudent updated)
        {
            if (!ModelState.IsValid)
                return View(updated);
            _studentModel.UpdateStudent(updated);
            return RedirectToAction("Student");
        }

        public IActionResult CreateStudent() => View();

        [HttpPost]
        public IActionResult CreateStudent(adminStudent model)
        {
            if (ModelState.IsValid)
            {
                _studentModel.InsertStudent(model);
                return RedirectToAction("Student");
            }
            return View(model);
        }

        public IActionResult StudentDelete(int id)
        {
            _studentModel.DeleteStudent(id);
            return RedirectToAction("Student");
        }

        // ====== Subject 部分 ======

        public IActionResult Subject()
        {
            var subjects = _subjectModel.GetAllSubjects();
            return View(subjects);
        }

        public IActionResult SubjectCreate() => View();

        [HttpPost]
        public IActionResult SubjectCreate(Subject subject)
        {
            if (ModelState.IsValid)
            {
                _subjectModel.InsertSubject(subject);
                return RedirectToAction("Subject");
            }
            return View(subject);
        }

        public IActionResult SubjectEdit(int id)
        {
            var subject = _subjectModel.GetSubjectById(id);
            return View(subject);
        }

        [HttpPost]
        public IActionResult SubjectEdit(Subject subject)
        {
            if (ModelState.IsValid)
            {
                _subjectModel.UpdateSubject(subject);
                return RedirectToAction("Subject");
            }
            return View(subject);
        }

        public IActionResult SubjectDelete(int id)
        {
            var subject = _subjectModel.GetSubjectById(id);
            if (subject == null)
                return NotFound();
            return View(subject);
        }

        [HttpPost, ActionName("SubjectDelete")]
        public IActionResult SubjectDeleteConfirmed(int id)
        {
            _subjectModel.DeleteSubject(id);
            return RedirectToAction("Subject");
        }


        // 案件總覽頁面
        public IActionResult OrderList()
        {
            ViewBag.Role = HttpContext.Session.GetString("AdminRole");
            var orders = _adminOrderService.GetAllOrders();
            return View(orders); // 你的 OrderList.cshtml
        }

        // 訂單明細（可選，若有詳細頁面）
        public IActionResult OrderDetail(int id)
        {
            
            // 1. 取得訂單
            var order = _adminOrderDetailService.GetAdminOrderDetail(id); // 回傳 OrderDetailViewModel
            if (order == null)
            {
                return NotFound(); // 找不到訂單
            }

            ViewBag.Role = HttpContext.Session.GetString("AdminRole");

            return View(order);
        }
        // 待老師確認
        public IActionResult OrderPending()
        => View("OrderList", _adminOrderService.GetPendingOrders());

        // 待學生確認
        public IActionResult OrderAccepted()
            => View("OrderList", _adminOrderService.GetAcceptedOrders());

        // 已成立
        public IActionResult OrderConfirmed() 
            => View("OrderList", _adminOrderService.GetToBeFinishedOrders());
        // 已完成（）
        public IActionResult OrderFinished() 
            => View("OrderList", _adminOrderService.GetFinishedOrders());

        // 已取消
        public IActionResult OrderCancelled()
            => View("OrderList", _adminOrderService.GetCancelledOrders());


        public IActionResult OrderHistory()
        {
            // 初次進入顯示空頁
            return View(new AdminOrderHistoryViewModel());
        }

        [HttpPost]
        public IActionResult OrderHistory(AdminOrderHistoryViewModel vm)
        {
            ViewBag.Role = HttpContext.Session.GetString("AdminRole");
            // 可支援姓名 or ID 查詢（你可自己優化搜尋邏輯）
            var orders = _adminOrderService.SearchOrderHistory(vm.StudentKeyword, vm.TeacherKeyword, vm.OrderKeyword);
            vm.Results = orders;
            return View(vm);
        }
        [HttpPost]
        public IActionResult DeleteOrder(int id)
        {
            ViewBag.Role = HttpContext.Session.GetString("AdminRole");
            if (ViewBag.Role != "superadmin")
            {
                TempData["Error"] = "只有 SuperAdmin 可以刪除訂單！";
                return RedirectToAction("OrderList"); // 或 return Forbid();
            }

            // 執行刪除
            _adminOrderService.DeleteOrder(id); // 你要自己實作刪除
            TempData["Success"] = "訂單已刪除。";
            return RedirectToAction("OrderList");
        }

    }
}
