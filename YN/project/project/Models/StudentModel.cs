using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Principal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;



namespace project.Models
{
    public class StudentModel
    {
        private readonly string connStr = "Data Source=(localdb)\\MSSQLLocalDB;Database=homeandteacher;User ID=yin;Password=Sky213312;Trusted_Connection=True";
        //private readonly string connStr = "Server=tcp:yindbserver.database.windows.net,1433;Initial Catalog=project_db;Persist Security Info=False;User ID=yin;Password=1qaz!QAZ;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";



        public List<adminStudent> getadminStudents()
        {
            List<adminStudent> students = new List<adminStudent>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "SELECT * FROM Students";
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    adminStudent student = new adminStudent
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        username = reader.GetString(reader.GetOrdinal("UserName")),
                        password = reader.GetString(reader.GetOrdinal("Password")),
                        name = reader.GetString(reader.GetOrdinal("Name")),
                        email = reader.GetString(reader.GetOrdinal("Email")),
                        phone = reader.GetString(reader.GetOrdinal("Phone")),
                        interests = reader.GetString(reader.GetOrdinal("Interests")),
                        registerDate = reader.GetDateTime(reader.GetOrdinal("RegisterDate")),
                        isActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                    };
                    students.Add(student);
                }
            }
            return students;
        }

        public bool InsertStudent(adminStudent student)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"INSERT INTO Students (UserName, Password, Name, Email, Phone, Interests)
                               VALUES (@UserName, @Password, @Name, @Email, @Phone, @Interests)";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserName", student.username);
                cmd.Parameters.AddWithValue("@Password", student.password);
                cmd.Parameters.AddWithValue("@Name", student.name);
                cmd.Parameters.AddWithValue("@Email", student.email);
                cmd.Parameters.AddWithValue("@Phone", student.phone);
                cmd.Parameters.AddWithValue("@Interests", student.interests);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public void UpdateStudent(adminStudent student)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"UPDATE Students SET 
                               UserName = @UserName,
                               Password = @Password,
                               Name = @Name,
                               Email = @Email,
                               Phone = @Phone,
                               Interests = @Interests,
                               IsActive = @IsActive
                               WHERE ID = @ID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserName", student.username);
                cmd.Parameters.AddWithValue("@Password", student.password);
                cmd.Parameters.AddWithValue("@Name", student.name);
                cmd.Parameters.AddWithValue("@Email", student.email);
                cmd.Parameters.AddWithValue("@Phone", student.phone);
                cmd.Parameters.AddWithValue("@Interests", student.interests);
                cmd.Parameters.AddWithValue("@IsActive", student.isActive);
                cmd.Parameters.AddWithValue("@ID", student.ID);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteStudent(int id)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "DELETE FROM Students WHERE ID = @ID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
