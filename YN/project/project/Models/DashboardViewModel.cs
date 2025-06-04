using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace project.Models
{
    public class DashboardStats
    {
        public int TotalTeachers { get; set; }
        public int TotalStudents { get; set; }
        public int MatchedOrders { get; set; }
        //public decimal ServerStatus { get; set; } 伺服器狀況需有監控
        public string ServerUptime { get; set; }
        public int CurrentMonthStudentRegister { get; set; }
        public int CurrentMonthTeacherRegister { get; set; }
        public double StudentRegisterGrowthPercent { get; set; }
        public double TeacherRegisterGrowthPercent { get; set; }
        public int ActiveStudentCount { get; set; }
        public int ActiveTeacherCount { get; set; }
    }


}
