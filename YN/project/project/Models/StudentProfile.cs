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
    public class StudentProfile
    {
        private readonly string connStr;
        private readonly IWebHostEnvironment _env;

        public StudentProfile(IConfiguration config, IWebHostEnvironment env)
        {
            connStr = config.GetConnectionString("DefaultConnection");
            _env = env;
        }

        // 1. 取得 Teacher Profile
        public StudentProfileViewModel GetStudentProfile(string userName)
        {
            StudentProfileViewModel vm = null;


            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // 抓教師基本資料
                SqlCommand cmd = new SqlCommand("SELECT * FROM Student WHERE UserName = @UserName", conn);
                cmd.Parameters.AddWithValue("@UserName", userName);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    vm = new StudentProfileViewModel
                    {
                        ID = (int)reader["ID"],
                        Name = reader["Name"].ToString(),
                        Email = reader["Email"].ToString(),
                        Phone = reader["Phone"].ToString(),
                        Gender = reader["Gender"].ToString(),
                        City = reader["City"].ToString(),

                    };
                }
                else
                {
                    return null; // 找不到
                }
            }



            return vm;
        }

        // 2. 更新 Teacher Profile
        public void UpdateStudentProfile(StudentProfileViewModel vm)
        {


            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // 更新 Teacher
                string sql = @"
                UPDATE Student SET
                    Name = @Name,
                    Email = @Email,
                    Phone = @Phone,
                    Gender = @Gender,
                    City = @City
                   
                WHERE ID = @ID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", vm.Name);
                cmd.Parameters.AddWithValue("@Email", vm.Email);
                cmd.Parameters.AddWithValue("@Phone", vm.Phone);
                cmd.Parameters.AddWithValue("@Gender", vm.Gender);
                cmd.Parameters.AddWithValue("@City", vm.City);

                cmd.Parameters.AddWithValue("@ID", vm.ID);

                cmd.ExecuteNonQuery();


            }
        }
    }
}