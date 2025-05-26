using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace project.Models
{
    public class adminTeacher
    {
        public int ID { get; set; }
        public int TokenBalance { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Introduction { get; set; }
        public string SubjectSpecialty { get; set; }
        public int ExperienceYears { get; set; }

        public bool IsActive { get; set; }
        public DateTime BirthDate { get; set; }

        public DateTime RegisterDate { get; set; }
        public string City { get; set; }

    }

    

}
