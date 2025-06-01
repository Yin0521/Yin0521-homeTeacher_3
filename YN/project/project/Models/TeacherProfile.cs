using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using project.ViewModels;

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
                        SubjectSpecialty = reader["SubjectSpecialty"].ToString()
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
                    SubjectSpecialty = @SubjectSpecialty
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
    }
}
