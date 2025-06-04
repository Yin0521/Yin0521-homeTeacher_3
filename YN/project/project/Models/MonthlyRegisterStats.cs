using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace project.Models
{
    public class MonthlyRegisterStats
    {
        public string Month { get; set; }
        public int StudentCount { get; set; }
        public int TeacherCount { get; set; }
    }


}
