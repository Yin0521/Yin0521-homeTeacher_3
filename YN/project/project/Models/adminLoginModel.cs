using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Principal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;



namespace project.Models
{
    public class adminLoginModel
    {
        private readonly string connStr;
        public adminLoginModel(IConfiguration configuration)
        {
            connStr = configuration.GetConnectionString("DefaultConnection");
        }


        public List<adminAccount> getadminAccounts()
        {
            List<adminAccount> accounts = new List<adminAccount>();

            SqlConnection sqlConnection = new SqlConnection(connStr);
            SqlCommand sqlCommand = new SqlCommand("SELECT * FROM admin ORDER BY " +
                     "CASE WHEN role = 'superadmin' THEN 0 ELSE 1 END, ID");
            sqlCommand.Connection = sqlConnection;
            sqlConnection.Open();

            SqlDataReader reader = sqlCommand.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    adminAccount account = new adminAccount
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("id")),
                        username = reader.GetString(reader.GetOrdinal("username")),
                        password = reader.GetString(reader.GetOrdinal("password")),
                        phone = reader.GetString(reader.GetOrdinal("phone")),
                        name = reader.GetString(reader.GetOrdinal("name")),
                        role = reader.GetString(reader.GetOrdinal("role")),
                        is_active = reader.GetBoolean(reader.GetOrdinal("is_active")),
                        created_at = reader.GetDateTime(reader.GetOrdinal("created_at"))
                    };
                    accounts.Add(account);
                }
            }
            else
            {
                Console.WriteLine("資料庫為空！");
            }
            sqlConnection.Close();
            return accounts;
        }

        public bool InsertAdmin(adminAccount admin)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"INSERT INTO admin (username, password, name, phone, role, is_active, created_at)
                         VALUES (@username, @password, @name, @phone, @role, @is_active, @created_at)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", admin.username);
                cmd.Parameters.AddWithValue("@password", admin.password);
                cmd.Parameters.AddWithValue("@name", admin.name);
                cmd.Parameters.AddWithValue("@phone", admin.phone);
                cmd.Parameters.AddWithValue("@role", admin.role);
                cmd.Parameters.AddWithValue("@is_active", admin.is_active);
                cmd.Parameters.AddWithValue("@created_at", DateTime.Now);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        //更新
        public void UpdateAdmin(adminAccount admin)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"UPDATE admin 
                    SET username = @username,
                        name = @name,
                        phone = @phone,
                        role = @role,
                        is_active = @is_active
                    WHERE ID = @ID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@username", admin.username);
                cmd.Parameters.AddWithValue("@name", admin.name ?? "");
                cmd.Parameters.AddWithValue("@phone", admin.phone ?? "");
                cmd.Parameters.AddWithValue("@role", admin.role);
                cmd.Parameters.AddWithValue("@is_active", admin.is_active);
                cmd.Parameters.AddWithValue("@ID", admin.ID);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        //刪除
        public void DeleteAdmin(int id)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "DELETE FROM admin WHERE id = @id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public void UpdatePassword(int id, string newPassword)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"UPDATE admin SET password = @password WHERE ID = @ID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@password", newPassword);
                cmd.Parameters.AddWithValue("@ID", id);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }
    }
}
