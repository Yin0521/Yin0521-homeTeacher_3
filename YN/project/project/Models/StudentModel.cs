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
                string sql = "SELECT * FROM Student";
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    adminStudent student = new adminStudent
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        UserName = reader.GetString(reader.GetOrdinal("UserName")),
                        Password = reader.GetString(reader.GetOrdinal("Password")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        Phone = reader.GetString(reader.GetOrdinal("Phone")),
                        RegisterDate = reader.GetDateTime(reader.GetOrdinal("RegisterDate")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                        BirthDate = reader.GetDateTime(reader.GetOrdinal("BirthDate")),
                        TokenBalance = reader.GetInt32(reader.GetOrdinal("TokenBalance")),
                        City = reader.IsDBNull(reader.GetOrdinal("City")) ? "" : reader.GetString(reader.GetOrdinal("City"))
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
                string sql = @"INSERT INTO Student (UserName, Password, Name, Email, Phone, BirthDate, TokenBalance, City
                               VALUES (@UserName, @Password, @Name, @Email, @Phone, @BirthDate, @TokenBalance, @City)";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserName", student.UserName);
                cmd.Parameters.AddWithValue("@Password", student.Password);
                cmd.Parameters.AddWithValue("@Name", student.Name);
                cmd.Parameters.AddWithValue("@Email", student.Email);
                cmd.Parameters.AddWithValue("@Phone", student.Phone);
                cmd.Parameters.AddWithValue("@BirthDate", student.BirthDate);
                cmd.Parameters.AddWithValue("@TokenBalance", student.TokenBalance);
                cmd.Parameters.AddWithValue("@City", student.City ?? (object)DBNull.Value); // 如果 City 為 null，則插入 DBNull

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
                               Password = @Password,
                               Name = @Name,
                               Email = @Email,
                               Phone = @Phone,
                               IsActive = @IsActive,
                               BirthDate = @BirthDate,
                               TokenBalance = @TokenBalance,
                               City = @City
                               WHERE ID = @ID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserName", student.UserName);
                cmd.Parameters.AddWithValue("@Password", student.Password);
                cmd.Parameters.AddWithValue("@Name", student.Name);
                cmd.Parameters.AddWithValue("@Email", student.Email);
                cmd.Parameters.AddWithValue("@Phone", student.Phone);
                cmd.Parameters.AddWithValue("@IsActive", student.IsActive);
                cmd.Parameters.AddWithValue("@ID", student.ID);
                cmd.Parameters.AddWithValue("@Age", student.BirthDate);
                cmd.Parameters.AddWithValue("@BirthDate", student.BirthDate);
                cmd.Parameters.AddWithValue("@TokenBalance", student.TokenBalance);
                cmd.Parameters.AddWithValue("@City", student.City ?? (object)DBNull.Value); // 如果 City 為 null，則插入 DBNull
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
    }
}
