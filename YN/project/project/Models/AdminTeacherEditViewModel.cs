using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace project.Models
{
    public class AdminTeacherEditViewModel
    {
        public adminTeacher Teacher { get; set; }

        // 所有科目供選擇
        [BindNever]
        public List<Subject>? AllSubjects { get; set; }

        // 被選中的科目 ID 清單
        public List<int> SelectedSubjectIds { get; set; }
    }
}
