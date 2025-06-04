using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Principal;
using NuGet.DependencyResolver;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;



namespace project.Models
{
    public class StudentModel
    {
        private readonly string connStr;
        public StudentModel(IConfiguration configuration)
        {
            connStr = configuration.GetConnectionString("DefaultConnection");
        }

        public List<adminStudent> getadminStudents()
        {
            List<adminStudent> students = new List<adminStudent>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "SELECT * FROM Student";
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    adminStudent student = new adminStudent
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        UserName = reader.IsDBNull(reader.GetOrdinal("UserName")) ? "" : reader.GetString(reader.GetOrdinal("UserName")),
                        Password = reader.IsDBNull(reader.GetOrdinal("Password")) ? "" : reader.GetString(reader.GetOrdinal("Password")),
                        Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? "" : reader.GetString(reader.GetOrdinal("Name")),
                        Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? "" : reader.GetString(reader.GetOrdinal("Email")),
                        Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? "" : reader.GetString(reader.GetOrdinal("Phone")),
                        RegisterDate = reader.IsDBNull(reader.GetOrdinal("RegisterDate")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("RegisterDate")),
                        IsActive = reader.IsDBNull(reader.GetOrdinal("IsActive")) ? false : reader.GetBoolean(reader.GetOrdinal("IsActive")),
                        BirthDate = reader.IsDBNull(reader.GetOrdinal("BirthDate")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("BirthDate")),
                        TokenBalance = reader.IsDBNull(reader.GetOrdinal("TokenBalance")) ? 0 : reader.GetInt32(reader.GetOrdinal("TokenBalance")),
                        City = reader.IsDBNull(reader.GetOrdinal("City")) ? "" : reader.GetString(reader.GetOrdinal("City")),
                        Gender = reader.IsDBNull(reader.GetOrdinal("Gender")) ? "" : reader.GetString(reader.GetOrdinal("Gender"))
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
                string sql = @"INSERT INTO Student (UserName, Password, Name, Email, Phone, BirthDate, TokenBalance, City, Gender)
                       VALUES (@UserName, @Password, @Name, @Email, @Phone, @BirthDate, @TokenBalance, @City, @Gender)";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserName", student.UserName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Name", student.Name ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Password", student.Password ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", student.Email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", student.Phone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@BirthDate", student.BirthDate == DateTime.MinValue ? (object)DBNull.Value : student.BirthDate);
                cmd.Parameters.AddWithValue("@TokenBalance", student.TokenBalance);
                cmd.Parameters.AddWithValue("@City", student.City ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Gender", student.Gender ?? (object)DBNull.Value);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public void UpdateStudent(adminStudent student)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"UPDATE Student SET 
                       UserName = @UserName,
                       Name = @Name,
                       Email = @Email,
                       Phone = @Phone,
                       IsActive = @IsActive,
                       BirthDate = @BirthDate,
                       TokenBalance = @TokenBalance,
                       City = @City,
                       Gender = @Gender
                       WHERE ID = @ID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserName", student.UserName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Name", student.Name ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", student.Email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", student.Phone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", student.IsActive);
                cmd.Parameters.AddWithValue("@ID", student.ID);
                cmd.Parameters.AddWithValue("@BirthDate", student.BirthDate == DateTime.MinValue ? (object)DBNull.Value : student.BirthDate);
                cmd.Parameters.AddWithValue("@TokenBalance", student.TokenBalance);
                cmd.Parameters.AddWithValue("@City", student.City ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Gender", student.Gender ?? (object)DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteStudent(int id)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "DELETE FROM Student WHERE ID = @ID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /// 密碼重設
        public void UpdatePassword(int id, string newPassword)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "UPDATE Student SET Password = @Password WHERE ID = @ID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Password", newPassword);
                cmd.Parameters.AddWithValue("@ID", id);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }
    }
}
