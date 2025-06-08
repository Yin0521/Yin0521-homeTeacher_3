using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using project.Models;

namespace project.Controllers
{
    public class OrderController : Controller
    {
        private readonly OrderModel _orderModel;
        private readonly OrderService _orderService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // 統一依賴注入
        public OrderController(OrderModel orderModel, OrderService orderService, IHttpContextAccessor httpContextAccessor)
        {
            _orderModel = orderModel;
            _orderService = orderService;
            _httpContextAccessor = httpContextAccessor;
        }

        // 學生下訂單
        [HttpPost]
        public IActionResult Create(int teacherId, int subjectId, string message, int? price)
        {
            var studentId = int.Parse(HttpContext.Session.GetString("StudentId"));
            var order = new Order
            {
                StudentID = studentId,
                TeacherID = teacherId,
                SubjectID = subjectId,
                Message = message,
                Price = price,
                OrderStatus = OrderStatus.Pending
            };
            // 直接用已注入的 _orderModel，不要再 new
            var newOrderId = _orderModel.InsertOrder(order);
            return RedirectToAction("OrderDetail", "Order", new { id = newOrderId });
        }

        

        // 訂單詳細
        public IActionResult OrderDetail(int id)
        {
            var orderVM = _orderService.GetOrderDetail(id); // 這裡要回傳 OrderViewModel
            if (orderVM == null) return NotFound();

            // 讀取 Session
            var role = HttpContext.Session.GetString("Identity");
            ViewBag.Role = role; // "Teacher" or "Student"
            return View(orderVM);
        }

        // 老師後台-接受訂單
        [HttpPost]
        public IActionResult TeacherAcceptWithPrice(int id, int finalPrice)
        {
            _orderModel.TeacherConfirmWithPrice(id, finalPrice);
            return RedirectToAction("OrderDetail", new { id });
        }

        // 老師後台-拒絕訂單
        [HttpPost]
        public IActionResult TeacherReject(int id)
        {
            _orderModel.RejectOrder(id);
            return RedirectToAction("OrderDetail", new { id });
        }

        // 學生-確認訂單
        [HttpPost]
        public IActionResult StudentConfirm(int id)
        {
            _orderModel.StudentConfirm(id);
            return RedirectToAction("OrderDetail", new { id });
        }

        // 學生-取消訂單
        [HttpPost]
        public IActionResult StudentCancel(int id)
        {
            _orderModel.CancelOrder(id);
            return RedirectToAction("OrderDetail", new { id });
        }


        // 完成訂單
        [HttpPost]
        public IActionResult TeacherFinish(int id)
        {
            _orderModel.TeacherFinish(id);
            return RedirectToAction("OrderDetail", new { id });
        }

        [HttpPost]
        public IActionResult StudentFinish(int id)
        {
            _orderModel.StudentFinish(id);
            return RedirectToAction("OrderDetail", new { id });
        }



        // 學生訂單列表
        public IActionResult StudentOrderList(string type = "pending")
        {
            int studentId = int.Parse(HttpContext.Session.GetString("StudentId"));
            List<OrderStatus> statusList = type switch
            {
                "pending" => new List<OrderStatus> { OrderStatus.Pending },
                "accepted" => new List<OrderStatus> { OrderStatus.Accepted },
                "inprogress" => new List<OrderStatus> { OrderStatus.Confirmed, OrderStatus.TeacherCompleted, OrderStatus.StudentCompleted },
                "finished" => new List<OrderStatus> { OrderStatus.Finished },
                "cancelled" => new List<OrderStatus> { OrderStatus.StudentCancelled, OrderStatus.TeacherRejected },
                _ => new List<OrderStatus> { OrderStatus.Pending }
            };

            var orders = _orderService.GetOrdersByStudentAndStatus(studentId, statusList);
            ViewBag.StatusCounts = _orderService.GetOrderStatusCountsByStudent(studentId);
            ViewBag.Type = type; // 傳給View判斷目前Tab
            return View(orders);
        }

        //老師訂單列表
        public IActionResult TeacherOrderList(string type = "pending")
        {
            int teacherId = int.Parse(HttpContext.Session.GetString("TeacherId"));
            List<OrderStatus> statusList = type switch
            {
                "pending" => new List<OrderStatus> { OrderStatus.Pending },
                "accepted" => new List<OrderStatus> { OrderStatus.Accepted },
                "inprogress" => new List<OrderStatus> { OrderStatus.Confirmed, OrderStatus.TeacherCompleted, OrderStatus.StudentCompleted },
                "finished" => new List<OrderStatus> { OrderStatus.Finished },
                "cancelled" => new List<OrderStatus> { OrderStatus.StudentCancelled, OrderStatus.TeacherRejected },
                _ => new List<OrderStatus> { OrderStatus.Pending }
            };

            var orders = _orderService.GetOrdersByTeacherAndStatus(teacherId, statusList);
            ViewBag.Type = type; // 傳給View判斷目前Tab
            ViewBag.StatusCounts = _orderService.GetOrderStatusCountsByTeacher(teacherId);
            return View(orders);
        }
    }
}
