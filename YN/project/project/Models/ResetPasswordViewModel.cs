using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace project.Models
{
    public class ResetPasswordViewModel
    {
        public int ID { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public string UserName { get; set; }
    }
}

