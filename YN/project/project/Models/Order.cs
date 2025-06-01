using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace project.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public int StudentID { get; set; }
        public int TeacherID { get; set; }
        public int SubjectID { get; set; }
        public string Message { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? TeacherConfirmTime { get; set; }
        public DateTime? StudentConfirmTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public DateTime? CancelTime { get; set; }
        public DateTime? TeacherFinishTime { get; set; }
        public DateTime? StudentFinishTime { get; set; }
    }



}
