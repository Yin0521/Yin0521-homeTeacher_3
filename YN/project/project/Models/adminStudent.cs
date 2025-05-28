using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace project.Models
{
    public class adminStudent
    {
        public int ID { get; set; }
        public int TokenBalance { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime RegisterDate { get; set; }
        public DateTime? BirthDate { get; set; }
        public bool IsActive { get; set; }
        public string City { get; set; }

        public string Gender { get; set; }
    }

    

}
