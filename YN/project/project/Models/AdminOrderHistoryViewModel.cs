using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace project.Models
{
    public class AdminOrderHistoryViewModel
    {
        public string StudentKeyword { get; set; }  // 可用姓名或ID
        public string TeacherKeyword { get; set; }  // 可用姓名或ID
        public string OrderKeyword { get; set; } //訂單ID
        public List<AdminOrderViewModel> Results { get; set; }
    }


}
