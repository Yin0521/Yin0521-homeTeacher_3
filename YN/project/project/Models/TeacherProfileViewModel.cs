using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using project.Models;

namespace project.ViewModels
{
    public class TeacherProfileViewModel
    {
        public int ID { get; set; }

        
        public string Name { get; set; }

        public string Email { get; set; }
        public string Phone { get; set; }
        public string Gender { get; set; }
        public string City { get; set; }
        public int ExperienceYears { get; set; }
        public string Introduction { get; set; }

        public IFormFile? Photo { get; set; }
        [BindNever]
        public string? PhotoPath { get; set; }

        public List<Subject> AllSubjects { get; set; } = new();
        public List<int> SelectedSubjectIds { get; set; } = new();
        public string SubjectSpecialty { get; set; }
    }

}


