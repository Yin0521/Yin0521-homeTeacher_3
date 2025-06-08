using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace project.Models.Teacher
{
    public class TeacherAvailableSlot
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public string DayOfWeek { get; set; } // Mon~Sun
        public string TimeSlot { get; set; }  // Morning, Afternoon, Night

    }

    

}
