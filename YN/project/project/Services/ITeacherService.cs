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
        List<TeacherSearchResult> SearchTeachers(string subject, string city, string timeslot);
        List<RecommendedTeacher> GetRecommendedTeachers(); //推薦老師
        List<Subject> GetAllSubjectsWithDescription(); //課程推薦




    }
}