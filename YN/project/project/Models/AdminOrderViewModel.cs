using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace project.Models
{
    public class AdminOrderViewModel
    {
        public int OrderID { get; set; }
        public int StudentID { get; set; }
        public string StudentName { get; set; }
        public int TeacherID { get; set; }
        public string TeacherName { get; set; }
        public int SubjectID { get; set; }
        public string SubjectName { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public string StatusText { get; set; }
        public string Message { get; set; }
        public DateTime? CreateTime { get; set; }

        // 其他你覺得畫面上會用到的欄位
        public string ContactPhone { get; set; }
        public string ContactLine { get; set; }
        public string ContactEmail { get; set; }
        public string MeetingType { get; set; }
        public decimal? Price { get; set; }

        // 其他狀態時間
        public DateTime? TeacherConfirmTime { get; set; }
        public DateTime? StudentConfirmTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public DateTime? CancelTime { get; set; }

        // 備註、留言等
        public string StudentNote { get; set; }
        public string TeacherNote { get; set; }
    }


}
