using System.Collections.Generic;
using project.Models;

namespace project.ViewModels
{
    public class TeacherSubjectViewModel
    {
        public int TeacherId { get; set; }

        // 傳給 checkbox 顯示用
        public List<Subject> AllSubjects { get; set; }

        // 接收使用者勾選回傳的值（checkbox value 對應）
        public List<int> SelectedSubjectIds { get; set; } = new List<int>();
    }
}
