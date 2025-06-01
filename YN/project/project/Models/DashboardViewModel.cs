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
    }


}
