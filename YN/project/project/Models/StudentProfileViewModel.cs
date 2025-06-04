using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using project.Models;

namespace project.ViewModels
{
    public class StudentProfileViewModel
    {
        public int ID { get; set; }


        public string Name { get; set; }

        public string Email { get; set; }
        public string Phone { get; set; }
        public string Gender { get; set; }
        public string City { get; set; }
    }
}
