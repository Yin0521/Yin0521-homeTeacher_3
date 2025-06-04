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
        private readonly FeedbackModel _feedbackModel; //意見回饋0603
        private readonly NewsletterModel _newsletterModel; //新增訂閱0603


        // 建構子注入所有 Model
        public adminController(
            adminLoginModel adminLoginModel,
            TeacherModel teacherModel,
            StudentModel studentModel,
            SubjectModel subjectModel,
            OrderService orderService,
            AdminOrderService adminOrderService,
            AdminOrderDetailService adminOrderDetailService,
            DashboardService dashboardService,
            FeedbackModel feedbackModel,
            NewsletterModel newsletterModel //新增訂閱0603
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
            _feedbackModel = feedbackModel;
            _newsletterModel = newsletterModel; //新增訂閱0603
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("AdminID") == null)
                return RedirectToAction("Login");
            DashboardStats stats = _dashboardService.GetDashboardStats();
            ViewBag.AdminUsername = HttpContext.Session.GetString("AdminUsername");

            // dashboard 統計數據
            // 折線圖（每月註冊數量）
            var monthlyStats = _dashboardService.GetMonthlyRegisterStats();
            ViewBag.MonthlyRegisterStats = monthlyStats;

            // 訂單狀態統計圖表
            var statusCounts = _dashboardService.GetThisMonthOrderStatusCounts();
            ViewBag.OrderStatusCounts = System.Text.Json.JsonSerializer.Serialize(statusCounts);

            // 老師熱門科目排行
            var topSubjects = _dashboardService.GetTopSubjects();
            ViewBag.TopSubjects = System.Text.Json.JsonSerializer.Serialize(topSubjects);

            // 學生城市分布
            var studentCities = _dashboardService.GetStudentCityCounts();
            ViewBag.StudentCities = System.Text.Json.JsonSerializer.Serialize(studentCities);

            // 老師城市分布
            var teacherCities = _dashboardService.GetTeacherCityCounts();
            ViewBag.TeacherCities = System.Text.Json.JsonSerializer.Serialize(teacherCities);

            // 熱門科目（以學生下單數量統計）
            var topOrderSubjects = _dashboardService.GetTopOrderedSubjects();
            ViewBag.TopOrderSubjects = System.Text.Json.JsonSerializer.Serialize(topOrderSubjects);


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
            ModelState.Remove("password"); // <-- 關鍵
            if (!ModelState.IsValid)
                return View(updated);

            _adminLoginModel.UpdateAdmin(updated);
            return RedirectToAction("Manager");
        }

        [HttpGet]
        public IActionResult AdminResetPassword(int id)
        {
            var admin = _adminLoginModel.getadminAccounts().FirstOrDefault(a => a.ID == id);
            if (admin == null)
                return NotFound();
            // 只需要 ID 跟帳號資訊，可自定一個簡單 ViewModel
            return View(new ResetPasswordViewModel { ID = admin.ID, UserName = admin.username });
        }


        // 管理員重設密碼處理
        [HttpPost]
        public IActionResult AdminResetPassword(int ID, string NewPassword, string ConfirmPassword)
        {
            bool hasError = false;

            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                ViewBag.NewPasswordError = "請輸入新密碼";
                hasError = true;
            }
            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ViewBag.ConfirmPasswordError = "請再次輸入新密碼";
                hasError = true;
            }
            else if (NewPassword != ConfirmPassword)
            {
                ViewBag.ConfirmPasswordError = "兩次密碼輸入不一致";
                hasError = true;
            }

            if (hasError)
            {
                // 傳一個簡單 model 回去
                return View(new ResetPasswordViewModel { ID = ID });
            }

            _adminLoginModel.UpdatePassword(ID, NewPassword);
            TempData["msg"] = "密碼重設成功";
            return RedirectToAction("AdminEdit", new { id = ID });
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
            var vm = new AdminTeacherEditViewModel
            {
                Teacher = teacher,
                AllSubjects = _subjectModel.GetAllSubjects(),
                SelectedSubjectIds = _teacherModel.GetSubjectIdsByTeacherId(id)
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult TeacherEdit(AdminTeacherEditViewModel model)
        {
            ModelState.Remove("Teacher.Password"); // <-- 關鍵

            if (!ModelState.IsValid)
            {
                model.AllSubjects = _subjectModel.GetAllSubjects();
                return View(model);
            }
            model.Teacher.SubjectIDs = model.SelectedSubjectIds;
            _teacherModel.UpdateTeacher(model.Teacher);
            _teacherModel.UpdateTeacherSubjects(model.Teacher.ID, model.SelectedSubjectIds);
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



        // GET: 顯示老師重設密碼頁面
        [HttpGet]
        public IActionResult TeacherResetPassword(int id)
        {
            var teacher = _teacherModel.getadminTeachers().FirstOrDefault(t => t.ID == id);
            if (teacher == null)
                return NotFound();
            return View(new ResetPasswordViewModel { ID = teacher.ID, UserName = teacher.UserName });
        }

        // POST: 提交重設密碼
        [HttpPost]
        public IActionResult TeacherResetPassword(ResetPasswordViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.NewPassword) || model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "請輸入相同的新密碼");
                return View(model);
            }
            _teacherModel.UpdatePassword(model.ID, model.NewPassword); // 你要自己加這個方法
            TempData["msg"] = "密碼重設成功";
            return RedirectToAction("TeacherEdit", new { id = model.ID });
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
            ModelState.Remove("Password"); // <-- 關鍵
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

        // GET: 顯示學生重設密碼頁面
        [HttpGet]
        public IActionResult StudentResetPassword(int id)
        {
            var student = _studentModel.getadminStudents().FirstOrDefault(s => s.ID == id);
            if (student == null)
                return NotFound();
            return View(new ResetPasswordViewModel { ID = student.ID, UserName = student.UserName });
        }
        // POST: 提交重設密碼
        [HttpPost]
        public IActionResult StudentResetPassword(ResetPasswordViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.NewPassword) || model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "請輸入相同的新密碼");
                return View(model);
            }
            _studentModel.UpdatePassword(model.ID, model.NewPassword); // 你要自己加這個方法
            TempData["msg"] = "密碼重設成功";
            return RedirectToAction("StudentEdit", new { id = model.ID });
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

        //意見回饋0603
        public IActionResult FeedbackList()
        {
            var feedbacks = _feedbackModel.GetAll();
            return View(feedbacks);
        }

        public IActionResult MarkFeedbackHandled(int id)
        {
            _feedbackModel.MarkHandled(id);
            return RedirectToAction("FeedbackList");
        }

        public IActionResult DeleteFeedback(int id)
        {
            _feedbackModel.Delete(id);
            return RedirectToAction("FeedbackList");
        }


        public IActionResult AdminList()
        {
            var list = _newsletterModel.GetAllSubscribersWithId();
            return View(list); // 用一個 View 顯示
        }

        [HttpPost]
        public IActionResult DeleteSubscriber(int id)
        {
            _newsletterModel.DeleteSubscriber(id);
            return RedirectToAction("AdminList");
        }

    }
}
