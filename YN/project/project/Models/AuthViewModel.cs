using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace project.Models
{
    public class AuthViewModel
    {
        // 登入欄位
        public string LoginName { get; set; }
        public string LoginPassword { get; set; }

        // 註冊欄位
        public string RegisterName { get; set; }
        public string RegisterEmail { get; set; }
        public string RegisterPassword { get; set; }
        public string RegisterPhone { get; set; }

        // 身分切換
        public string Role { get; set; } // "Teacher" or "Student"
        public bool IsLogin { get; set; } // true=登入, false=註冊
    }



}

