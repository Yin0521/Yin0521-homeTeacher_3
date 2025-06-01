using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace project.Models
{
    public class OrderViewModel
    {
        public int OrderID { get; set; }
        public int StudentID { get; set; }
        public int TeacherID { get; set; }
        public int SubjectID { get; set; }

        public string StudentName { get; set; }     // 來自 Student 資料表 JOIN
        public string TeacherName { get; set; }     // 來自 Teacher 資料表 JOIN
        public string SubjectName { get; set; }     // 來自 Subject 資料表 JOIN

        public string Message { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? TeacherConfirmTime { get; set; }
        public DateTime? StudentConfirmTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public DateTime? CancelTime { get; set; }
        public DateTime? ReserveTime { get; set; }

        public string StudentNote { get; set; }
        public string TeacherNote { get; set; }
        public decimal? Price { get; set; }
        public string ContactPhone { get; set; }
        public string ContactLine { get; set; }
        public string ContactEmail { get; set; }
        public string MeetingType { get; set; }
    }



}
