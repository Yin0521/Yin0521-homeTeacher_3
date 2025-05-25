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
        public string username { get; set; }
        public string password { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string interests { get; set; }
        public DateTime registerDate { get; set; }
        public bool isActive { get; set; }
    }

    

}
