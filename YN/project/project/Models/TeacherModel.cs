using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Security.Principal;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using NuGet.Protocol.Plugins;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using project.ViewModels;
using project.Models.Teacher;



namespace project.Models
{
    public class TeacherModel
    {
        private readonly string connStr;
        public TeacherModel(IConfiguration configuration)
        {
            connStr = configuration.GetConnectionString("DefaultConnection");
        }


        public List<adminTeacher> getadminTeachers()
        {
            List<adminTeacher> teachers = new List<adminTeacher>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT * FROM Teacher";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var teacher = new adminTeacher
                            {
                                ID = reader.GetInt32(reader.GetOrdinal("ID")),
                                UserName = reader.GetString(reader.GetOrdinal("UserName")),
                                Password = reader.GetString(reader.GetOrdinal("Password")),
                                Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? "" : reader.GetString(reader.GetOrdinal("Name")),
                                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? "" : reader.GetString(reader.GetOrdinal("Email")),
                                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? "" : reader.GetString(reader.GetOrdinal("Phone")),
                                Introduction = reader.IsDBNull(reader.GetOrdinal("Introduction")) ? "" : reader.GetString(reader.GetOrdinal("Introduction")),
                                SubjectSpecialty = reader.IsDBNull(reader.GetOrdinal("SubjectSpecialty")) ? "" : reader.GetString(reader.GetOrdinal("SubjectSpecialty")),
                                ExperienceYears = reader.IsDBNull(reader.GetOrdinal("ExperienceYears")) ? 0 : reader.GetInt32(reader.GetOrdinal("ExperienceYears")),
                                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                                RegisterDate = reader.GetDateTime(reader.GetOrdinal("RegisterDate")),
                                BirthDate = reader.IsDBNull(reader.GetOrdinal("BirthDate")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("BirthDate")),
                                TokenBalance = reader.IsDBNull(reader.GetOrdinal("TokenBalance")) ? 0 : reader.GetInt32(reader.GetOrdinal("TokenBalance")),
                                City = reader.IsDBNull(reader.GetOrdinal("City")) ? "" : reader.GetString(reader.GetOrdinal("City")),
                                Gender = reader.IsDBNull(reader.GetOrdinal("Gender")) ? "" : reader.GetString(reader.GetOrdinal("Gender")),
                                PhotoPath = reader.IsDBNull(reader.GetOrdinal("PhotoPath")) ? "" : reader.GetString(reader.GetOrdinal("PhotoPath")),
                                Recommend = !reader.IsDBNull(reader.GetOrdinal("Recommend")) && reader.GetBoolean(reader.GetOrdinal("Recommend")),
                                SubjectIDs = new List<int>() // 初始化 SubjectIDs
                            };
                            teachers.Add(teacher);
                        }
                    }
                }
            }

            // 針對每位老師補上 SubjectIDs
            foreach (var teacher in teachers)
            {
                teacher.SubjectIDs = GetSubjectIdsByTeacherId(teacher.ID);
            }

            return teachers;
        }

        // 拆成獨立方法，取某老師的科目 IDs
        public List<int> GetSubjectIdsByTeacherId(int teacherId)
        {
            List<int> ids = new();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string sql = "SELECT SubjectId FROM TeacherSubjects WHERE TeacherId = @TeacherId";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TeacherId", teacherId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ids.Add(reader.GetInt32(0));
                        }
                    }
                }
            }
            return ids;
        }



        public int InsertTeacher(adminTeacher teacher)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"INSERT INTO Teacher (
                                UserName, Password, Name, Email, Phone, Introduction, SubjectSpecialty, ExperienceYears, RegisterDate, IsActive, BirthDate, TokenBalance, City, Gender
                                )
                                VALUES (
                                @UserName, @Password, @Name, @Email, @Phone, @Introduction, @SubjectSpecialty, @ExperienceYears, @RegisterDate, @IsActive, @BirthDate, @TokenBalance, @City,@Gender
                                );
                                SELECT SCOPE_IDENTITY()";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserName", teacher.UserName);
                cmd.Parameters.AddWithValue("@Password", teacher.Password);
                cmd.Parameters.AddWithValue("@Name", teacher.Name);
                cmd.Parameters.AddWithValue("@Email", teacher.Email);
                cmd.Parameters.AddWithValue("@Phone", teacher.Phone ?? "");
                cmd.Parameters.AddWithValue("@Introduction", teacher.Introduction ?? "");
                cmd.Parameters.AddWithValue("@SubjectSpecialty", teacher.SubjectSpecialty ?? "");
                cmd.Parameters.AddWithValue("@ExperienceYears", teacher.ExperienceYears);
                cmd.Parameters.AddWithValue("@RegisterDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@IsActive", true);
                cmd.Parameters.AddWithValue("@TokenBalance", 0); // 預設 TokenBalance 為 0
                cmd.Parameters.AddWithValue("@BirthDate", teacher.BirthDate);
                cmd.Parameters.AddWithValue("@City", teacher.City ?? "");
                cmd.Parameters.AddWithValue("@Gender", teacher.Gender ?? "");


                conn.Open();

                var newIdObj = cmd.ExecuteScalar();
                int newId = Convert.ToInt32(newIdObj);

                conn.Close();

                return newId; // 回傳新老師ID
            }
        }
        public List<Subject> GetAllSubjects()
        {
            List<Subject> list = new List<Subject>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "SELECT ID, Name FROM Subject";
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new Subject
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1)
                    });
                }
                conn.Close();
            }
            return list;
        }

        public void UpdateTeacherSubjects(int teacherId, List<int> subjectIds)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // 刪除舊資料
                SqlCommand deleteCmd = new SqlCommand("DELETE FROM TeacherSubjects WHERE TeacherId = @TeacherId", conn);
                deleteCmd.Parameters.AddWithValue("@TeacherId", teacherId);
                deleteCmd.ExecuteNonQuery();

                // 新增資料
                foreach (int subjectId in subjectIds)
                {
                    SqlCommand insertCmd = new SqlCommand("INSERT INTO TeacherSubjects (TeacherId, SubjectId) VALUES (@TeacherId, @SubjectId)", conn);
                    insertCmd.Parameters.AddWithValue("@TeacherId", teacherId);
                    insertCmd.Parameters.AddWithValue("@SubjectId", subjectId);
                    insertCmd.ExecuteNonQuery();
                }

                conn.Close();
            }
        }


        //更新
        public void UpdateTeacher(adminTeacher teacher)
        {
            Console.WriteLine("進入 UpdateTeacher 方法");
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"UPDATE Teacher 
                       SET UserName = @UserName,
                           Name = @Name,
                           Email = @Email,
                           Phone = @Phone, 
                           Introduction = @Introduction,
                           SubjectSpecialty = @SubjectSpecialty,
                           ExperienceYears = @ExperienceYears,
                           IsActive = @IsActive,
                           BirthDate = @BirthDate,
                           TokenBalance = @TokenBalance,
                           City = @City,
                           Gender = @Gender
                            
                            
                            
    
                       WHERE ID = @ID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserName", teacher.UserName);
                cmd.Parameters.AddWithValue("@Name", teacher.Name ?? "");
                cmd.Parameters.AddWithValue("@Email", teacher.Email ?? "");
                cmd.Parameters.AddWithValue("@Phone", teacher.Phone ?? "");
                cmd.Parameters.AddWithValue("@Introduction", teacher.Introduction ?? "");
                cmd.Parameters.AddWithValue("@SubjectSpecialty", teacher.SubjectSpecialty ?? "");
                cmd.Parameters.AddWithValue("@ExperienceYears", teacher.ExperienceYears);
                cmd.Parameters.AddWithValue("@IsActive", teacher.IsActive);
                cmd.Parameters.AddWithValue("@ID", teacher.ID);
                cmd.Parameters.AddWithValue("@BirthDate", teacher.BirthDate);
                cmd.Parameters.AddWithValue("@TokenBalance", teacher.TokenBalance);
                cmd.Parameters.AddWithValue("@City", teacher.City ?? "");
                cmd.Parameters.AddWithValue("@Gender", teacher.Gender ?? "");


                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }



        //刪除
        public void DeleteTeacher(int id)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "DELETE FROM Teacher WHERE ID = @ID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", id);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        //更新密碼
        public void UpdatePassword(int id, string newPassword)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "UPDATE Teacher SET Password = @Password WHERE ID = @ID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Password", newPassword);
                cmd.Parameters.AddWithValue("@ID", id);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        // 查詢：取得教師可用時段
        public List<TeacherAvailableSlot> GetAvailableSlotsByTeacherId(int teacherId)
        {
            var slots = new List<TeacherAvailableSlot>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string sql = "SELECT Id, TeacherId, DayOfWeek, TimeSlot FROM TeacherAvailableSlot WHERE TeacherId = @TeacherId";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TeacherId", teacherId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            slots.Add(new TeacherAvailableSlot
                            {
                                Id = reader.GetInt32(0),
                                TeacherId = reader.GetInt32(1),
                                DayOfWeek = reader.GetString(2),
                                TimeSlot = reader.GetString(3)
                            });
                        }
                    }
                }
            }
            return slots;
        }

        // 儲存/更新教師可用時段
        public void UpdateTeacherAvailableSlots(int teacherId, List<TeacherAvailableSlot> slots)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // 先刪除所有舊的時段
                string deleteSql = "DELETE FROM TeacherAvailableSlot WHERE TeacherId = @TeacherId";
                using (SqlCommand delCmd = new SqlCommand(deleteSql, conn))
                {
                    delCmd.Parameters.AddWithValue("@TeacherId", teacherId);
                    delCmd.ExecuteNonQuery();
                }

                // 新增新的時段
                foreach (var slot in slots)
                {
                    string insertSql = "INSERT INTO TeacherAvailableSlot (TeacherId, DayOfWeek, TimeSlot) VALUES (@TeacherId, @DayOfWeek, @TimeSlot)";
                    using (SqlCommand insCmd = new SqlCommand(insertSql, conn))
                    {
                        insCmd.Parameters.AddWithValue("@TeacherId", teacherId);
                        insCmd.Parameters.AddWithValue("@DayOfWeek", slot.DayOfWeek);
                        insCmd.Parameters.AddWithValue("@TimeSlot", slot.TimeSlot);
                        insCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        // 搜尋：取得所有教師可用時段
        public List<TeacherAvailableSlot> GetAllAvailableSlots()
        {
            var slots = new List<TeacherAvailableSlot>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string sql = "SELECT Id, TeacherId, DayOfWeek, TimeSlot FROM TeacherAvailableSlot";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        slots.Add(new TeacherAvailableSlot
                        {
                            Id = reader.GetInt32(0),
                            TeacherId = reader.GetInt32(1),
                            DayOfWeek = reader.GetString(2),
                            TimeSlot = reader.GetString(3)
                        });
                    }
                }
            }
            return slots;
        }
    }
}
