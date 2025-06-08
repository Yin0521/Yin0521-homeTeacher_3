using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace project.Models
{
    public class TeacherOrderViewModel
    {
        public adminTeacher Teacher { get; set; }          // 老師基本資料
        public List<Subject> Subjects { get; set; }  // 所有科目
        public int SubjectID { get; set; }                 // 學生想預約哪個科目
        public string Message { get; set; }// 學生下單留言
        public List<string> AvailableSlots { get; set; } = new();
        public int? HourlyRate { get; set; }
    }



}
