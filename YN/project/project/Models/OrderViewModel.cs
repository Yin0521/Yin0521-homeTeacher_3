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

        public string StudentName { get; set; }
        public string StudentPhone { get; set; }
        public string StudentEmail { get; set; }
        public string StudentLine { get; set; }

        public string TeacherName { get; set; }
        public string TeacherPhone { get; set; }
        public string TeacherEmail { get; set; }
        public string TeacherLine { get; set; }
        
        public string SubjectName { get; set; }     // 來自 Subject 資料表 JOIN

        public string Message { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? TeacherConfirmTime { get; set; }
        public DateTime? StudentConfirmTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public DateTime? CancelTime { get; set; }
        public DateTime? ReserveTime { get; set; }

        public int? Price { get; set; } // 學生期望費用
        public int? FinalPrice { get; set; } // 老師確認費用
    }



}
