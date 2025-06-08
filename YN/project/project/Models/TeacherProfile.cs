using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using project.ViewModels;
using project.Models.Teacher;

namespace project.Models
{
    public class TeacherProfile
    {
        private readonly string connStr;
        private readonly IWebHostEnvironment _env;

        public TeacherProfile(IConfiguration config, IWebHostEnvironment env)
        {
            connStr = config.GetConnectionString("DefaultConnection");
            _env = env;
        }

        // 1. 取得 Teacher Profile
        public TeacherProfileViewModel GetTeacherProfile(string userName)
        {
            TeacherProfileViewModel vm = null;
            List<Subject> allSubjects = new();
            List<int> selectedIds = new();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // 抓教師基本資料
                SqlCommand cmd = new SqlCommand("SELECT * FROM Teacher WHERE UserName = @UserName", conn);
                cmd.Parameters.AddWithValue("@UserName", userName);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    vm = new TeacherProfileViewModel
                    {
                        ID = (int)reader["ID"],
                        Name = reader["Name"].ToString(),
                        Email = reader["Email"].ToString(),
                        Phone = reader["Phone"].ToString(),
                        Gender = reader["Gender"].ToString(),
                        City = reader["City"].ToString(),
                        ExperienceYears = (int)reader["ExperienceYears"],
                        Introduction = reader["Introduction"].ToString(),
                        PhotoPath = reader["PhotoPath"].ToString(),
                        SubjectSpecialty = reader["SubjectSpecialty"].ToString(),
                        HourlyRate = reader["HourlyRate"] != DBNull.Value ? (int?)reader["HourlyRate"] : null
                    };
                }
                else
                {
                    return null; // 找不到
                }
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // 抓所有科目
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM Subject", conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        allSubjects.Add(new Subject
                        {
                            Id = (int)reader["Id"],
                            Name = reader["Name"].ToString()
                        });
                    }
                }

                // 抓已選擇的科目
                using (SqlCommand cmd = new SqlCommand("SELECT SubjectId FROM TeacherSubjects WHERE TeacherId = @ID", conn))
                {
                    cmd.Parameters.AddWithValue("@ID", vm.ID);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            selectedIds.Add((int)reader["SubjectId"]);
                        }
                    }
                }
            }

            vm.AllSubjects = allSubjects;
            vm.SelectedSubjectIds = selectedIds;
            vm.AvailableSlots = GetAvailableSlotsByTeacherId(vm.ID);

            return vm;
        }

        // 2. 更新 Teacher Profile
        public void UpdateTeacherProfile(TeacherProfileViewModel vm)
        {
            // 儲存圖片，如果有新圖就覆蓋 PhotoPath，否則保留原來的
            if (vm.Photo != null && vm.Photo.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetFileName(vm.Photo.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                vm.Photo.CopyTo(stream);
                vm.PhotoPath = "/uploads/" + uniqueFileName;
            }
            else
            {
                // 若沒選擇新圖片，保留原本的 PhotoPath
                if (string.IsNullOrEmpty(vm.PhotoPath))
                {
                    using (var conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        var cmd = new SqlCommand("SELECT PhotoPath FROM Teacher WHERE ID = @ID", conn);
                        cmd.Parameters.AddWithValue("@ID", vm.ID);
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            vm.PhotoPath = result.ToString();
                        }
                    }
                }
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // 更新 Teacher
                string sql = @"
                UPDATE Teacher SET
                    Name = @Name,
                    Email = @Email,
                    Phone = @Phone,
                    Gender = @Gender,
                    City = @City,
                    ExperienceYears = @Exp,
                    Introduction = @Intro,
                    PhotoPath = @PhotoPath,
                    SubjectSpecialty = @SubjectSpecialty,
                    HourlyRate = @HourlyRate    
                WHERE ID = @ID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", vm.Name);
                cmd.Parameters.AddWithValue("@Email", vm.Email);
                cmd.Parameters.AddWithValue("@Phone", vm.Phone);
                cmd.Parameters.AddWithValue("@Gender", vm.Gender);
                cmd.Parameters.AddWithValue("@City", vm.City);
                cmd.Parameters.AddWithValue("@Exp", vm.ExperienceYears);
                cmd.Parameters.AddWithValue("@Intro", vm.Introduction);
                cmd.Parameters.AddWithValue("@PhotoPath", string.IsNullOrEmpty(vm.PhotoPath) ? (object)DBNull.Value : vm.PhotoPath);
                cmd.Parameters.AddWithValue("@ID", vm.ID);
                cmd.Parameters.AddWithValue("@SubjectSpecialty", vm.SubjectSpecialty);
                cmd.Parameters.AddWithValue("@HourlyRate", vm.HourlyRate.HasValue ? (object)vm.HourlyRate.Value : DBNull.Value);
                cmd.ExecuteNonQuery();

                // 更新 TeacherSubjects
                SqlCommand deleteCmd = new SqlCommand("DELETE FROM TeacherSubjects WHERE TeacherId = @ID", conn);
                deleteCmd.Parameters.AddWithValue("@ID", vm.ID);
                deleteCmd.ExecuteNonQuery();

                foreach (var sid in vm.SelectedSubjectIds.Distinct())
                {
                    SqlCommand insertCmd = new SqlCommand("INSERT INTO TeacherSubjects (TeacherId, SubjectId) VALUES (@Tid, @Sid)", conn);
                    insertCmd.Parameters.AddWithValue("@Tid", vm.ID);
                    insertCmd.Parameters.AddWithValue("@Sid", sid);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }

        // 3. 取得所有科目
        public List<Subject> GetAllSubjects()
        {
            var subjects = new List<Subject>();
            using var conn = new SqlConnection(connStr);
            conn.Open();
            using var cmd = new SqlCommand("SELECT * FROM Subject", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                subjects.Add(new Subject
                {
                    Id = (int)reader["Id"],
                    Name = reader["Name"].ToString()
                });
            }
            return subjects;
        }

        // 查詢：取得教師可用時段
        public List<string> GetAvailableSlotsByTeacherId(int teacherId)
        {
            var result = new List<string>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT DayOfWeek, TimeSlot FROM TeacherAvailableSlot WHERE TeacherId = @TeacherId", conn);
                cmd.Parameters.AddWithValue("@TeacherId", teacherId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var day = reader.GetString(0);
                        var slot = reader.GetString(1);
                        result.Add($"{day}_{slot}");
                    }
                }
            }
            return result;
        }

        // 儲存/更新教師可用時段
        public void UpdateTeacherAvailableSlots(int teacherId, List<string> slotKeys)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                // 先刪除所有舊的
                var del = new SqlCommand("DELETE FROM TeacherAvailableSlot WHERE TeacherId = @TeacherId", conn);
                del.Parameters.AddWithValue("@TeacherId", teacherId);
                del.ExecuteNonQuery();
                // 新增
                foreach (var key in slotKeys)
                {
                    var parts = key.Split('_');
                    var day = parts[0]; // Mon
                    var slot = parts[1]; // Morning
                    var ins = new SqlCommand("INSERT INTO TeacherAvailableSlot (TeacherId, DayOfWeek, TimeSlot) VALUES (@TeacherId,@Day,@Slot)", conn);
                    ins.Parameters.AddWithValue("@TeacherId", teacherId);
                    ins.Parameters.AddWithValue("@Day", day);
                    ins.Parameters.AddWithValue("@Slot", slot);
                    ins.ExecuteNonQuery();
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
