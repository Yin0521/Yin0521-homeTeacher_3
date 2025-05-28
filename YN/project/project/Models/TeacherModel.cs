using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Principal;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using NuGet.Protocol.Plugins;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;



namespace project.Models
{
    public class TeacherModel
    {
        private readonly string connStr = "Data Source=(localdb)\\MSSQLLocalDB;Database=homeandteacher;User ID=yin;Password=Sky213312;Trusted_Connection=True";
        //private readonly string connStr = "Server=tcp:yindbserver.database.windows.net,1433;Initial Catalog=project_db;Persist Security Info=False;User ID=yin;Password=1qaz!QAZ;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";



        public List<adminTeacher> getadminTeachers()
        {
            List<adminTeacher> teachers = new List<adminTeacher>();

            SqlConnection sqlConnection = new SqlConnection(connStr);
            SqlCommand sqlCommand = new SqlCommand("SELECT * FROM Teacher");
            sqlCommand.Connection = sqlConnection;
            sqlConnection.Open();

            SqlDataReader reader = sqlCommand.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    adminTeacher teacher = new adminTeacher
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
                        Gender = reader.IsDBNull(reader.GetOrdinal("Gender")) ? "" : reader.GetString(reader.GetOrdinal("Gender"))
                    };
                    teachers.Add(teacher);
                }
            }
            else
            {
                Console.WriteLine("資料庫為空！");
            }
            sqlConnection.Close();
            return teachers;
        }

        public int InsertTeacher(adminTeacher teacher)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"INSERT INTO Teacher (
                                UserName, Password, Name, Email, Phone, Introduction, SubjectSpecialty, ExperienceYears, RegisterDate, IsActive, BirthDate, TokenBalance, City, Gender
                                )
                                VALUES (
                                @UserName, @Password, @Name, @Email, @Phone, @Introduction, @SubjectSpecialty, @ExperienceYears, @RegisterDate, @IsActive, @BirthDate, @TokenBalance, @City, @Gender
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

        //更新
        public void UpdateTeacher(adminTeacher teacher)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"UPDATE Teacher 
                       SET UserName = @UserName,
                           Password = @Password,
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
                cmd.Parameters.AddWithValue("@Password", teacher.Password);
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
    }
}
