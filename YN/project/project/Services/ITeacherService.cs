using System.Collections.Generic;
using project.Models;
using project.ViewModels;

namespace project.Models.Services
{
    public interface ITeacherService
    {
        List<Subject> GetAllSubjects();
        List<int> GetTeacherSubjectIds(int teacherId);
        void SaveTeacherSubjects(int teacherId, List<int> subjectIds);
        List<TeacherSearchResult> SearchTeachers(string subject, string city); // << 只留這個
        List<RecommendedTeacher> GetRecommendedTeachers(); //推薦老師
        List<Subject> GetAllSubjectsWithDescription(); //課程推薦
    }
}