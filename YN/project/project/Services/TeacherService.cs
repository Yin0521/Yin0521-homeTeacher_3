using System.Collections.Generic;
using System.Data.SqlClient;
using project.Models;
using project.ViewModels;
using Dapper;
using System.Linq;

namespace project.Models.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly string connStr = "Data Source=(localdb)\\MSSQLLocalDB;Database=homeandteacher;User ID=yin;Password=Sky213312;Trusted_Connection=True";
        //private readonly string connStr = "Server=tcp:yindbserver.database.windows.net,1433;Initial Catalog=project_db;Persist Security Info=False;User ID=yin;Password=1qaz!QAZ;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        public List<Subject> GetAllSubjects()
        {
            var subjects = new List<Subject>();

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT Id, Name FROM Subject", conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    subjects.Add(new Subject
                    {
                        Id = (int)reader["Id"],
                        Name = reader["Name"].ToString()
                    });
                }
            }

            return subjects;
        }

        // 取得老師已選的科目 ID
        public List<int> GetTeacherSubjectIds(int teacherId)
        {
            var ids = new List<int>();

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT SubjectId FROM TeacherSubjects WHERE TeacherId = @teacherId", conn);
                cmd.Parameters.AddWithValue("@teacherId", teacherId);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ids.Add((int)reader["SubjectId"]);
                }
            }

            return ids;
        }

        // 儲存老師選的科目（先清除再新增）
        public void SaveTeacherSubjects(int teacherId, List<int> subjectIds)
        {
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();

                var deleteCmd = new SqlCommand("DELETE FROM TeacherSubjects WHERE TeacherId = @teacherId", conn);
                deleteCmd.Parameters.AddWithValue("@teacherId", teacherId);
                deleteCmd.ExecuteNonQuery();

                foreach (var subjectId in subjectIds)
                {
                    var insertCmd = new SqlCommand("INSERT INTO TeacherSubjects (TeacherId, SubjectId) VALUES (@teacherId, @subjectId)", conn);
                    insertCmd.Parameters.AddWithValue("@teacherId", teacherId);
                    insertCmd.Parameters.AddWithValue("@subjectId", subjectId);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }


        public List<TeacherSearchResult> SearchTeachers(string subjectId, string city)
        {
            var list = new List<TeacherSearchResult>();
            var teacherMap = new Dictionary<int, TeacherSearchResult>();

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                var sql = @"
    SELECT t.ID, t.Name, t.Gender, t.BirthDate, t.City, t.Introduction,
           s.Name AS SubjectName
    FROM Teacher t
    INNER JOIN TeacherSubjects ts ON t.ID = ts.TeacherId
    INNER JOIN Subject s ON ts.SubjectId = s.Id
    WHERE (@city = '' OR t.City LIKE '%' + @city + '%')
      AND (@subjectId IS NULL OR s.Id = @subjectId)
      AND t.IsActive = 1
    ORDER BY t.ID
";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@city", city ?? "");

                    // 判斷 subjectId 是否為空（如果是空就傳 NULL）
                    if (int.TryParse(subjectId, out var sid))
                        cmd.Parameters.AddWithValue("@subjectId", sid);
                    else
                        cmd.Parameters.AddWithValue("@subjectId", DBNull.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int teacherId = (int)reader["ID"];
                            if (!teacherMap.TryGetValue(teacherId, out var teacher))
                            {
                                teacher = new TeacherSearchResult
                                {
                                    Id = teacherId,
                                    Name = reader["Name"].ToString(),
                                    Gender = reader["Gender"]?.ToString(),
                                    Age = GetAge(reader["BirthDate"] as DateTime?),
                                    City = reader["City"].ToString(),
                                    Description = reader["Introduction"].ToString(),
                                    Subjects = new List<string>()
                                };
                                teacherMap[teacherId] = teacher;
                                list.Add(teacher);
                            }
                            // 加入科目
                            string subjectName = reader["SubjectName"].ToString();
                            if (!teacher.Subjects.Contains(subjectName))
                                teacher.Subjects.Add(subjectName);
                        }
                    }
                }
            }

            return list;
        }


        // 補一個小工具：計算年齡
        private int GetAge(DateTime? birthDate)
        {
            if (birthDate == null || birthDate == DateTime.MinValue) return 0;
            var today = DateTime.Today;
            var age = today.Year - birthDate.Value.Year;
            if (today < birthDate.Value.AddYears(age)) age--;
            return age;
        }


        //推薦老師
        public List<RecommendedTeacher> GetRecommendedTeachers()
        {
            using var conn = new SqlConnection(connStr);

            string sql = @"
                SELECT t.Id, t.Name, t.Email, t.Phone, t.PhotoPath, s.Name AS SubjectName
                FROM Teacher t
                LEFT JOIN TeacherSubjects ts ON t.Id = ts.TeacherId
                LEFT JOIN Subject s ON ts.SubjectId = s.Id
                WHERE t.Recommend = 1 AND t.IsActive = 1
                ORDER BY NEWID()";

            var teacherDict = new Dictionary<int, RecommendedTeacher>();

            conn.Query<RecommendedTeacher, string, RecommendedTeacher>(
                sql,
                (t, subjectName) =>
                {
                    if (!teacherDict.TryGetValue(t.Id, out var teacher))
                    {
                        teacher = t;
                        teacher.Subjects = new List<string>();
                        teacherDict.Add(teacher.Id, teacher);
                    }

                    if (!string.IsNullOrEmpty(subjectName))
                        teacher.Subjects.Add(subjectName);

                    return teacher;
                },
                splitOn: "SubjectName"
            );

            return teacherDict.Values.ToList();
        }
        //課程介紹
        public List<Subject> GetAllSubjectsWithDescription()
        {
            using var conn = new SqlConnection(connStr);
            string sql = @"SELECT Id, Name, Description FROM Subject";
            return conn.Query<Subject>(sql).ToList();
        }
    }
}
