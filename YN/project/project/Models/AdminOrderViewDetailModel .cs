using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace project.Models
{
    public class AdminOrderDetailViewModel
    {
        public int OrderID { get; set; }
        public int StudentID { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public string StudentPhone { get; set; }
        public int TeacherID { get; set; }
        public string TeacherName { get; set; }
        public string TeacherEmail { get; set; }
        public string TeacherPhone { get; set; }
        public string SubjectName { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public string OrderStatusText { get; set; }
        public string Message { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? TeacherConfirmTime { get; set; }
        public DateTime? StudentConfirmTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public DateTime? CancelTime { get; set; }
        public string StudentNote { get; set; }
        public string TeacherNote { get; set; }
        public int? Price { get; set; }
        public DateTime? TeacherFinishTime { get; set; }
        public DateTime? StudentFinishTime { get; set; }
    }


}
