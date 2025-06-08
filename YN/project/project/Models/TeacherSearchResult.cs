using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace project.Models
{
    public class TeacherSearchResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public string City { get; set; }
        public string Description { get; set; }
        public List<string> Subjects { get; set; }
        public List<string> AvailableSlots { get; set; } = new(); // 如果要顯示也可以加
    }

}
