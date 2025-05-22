using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Principal;



namespace project.Models
{
    public class adminLoginModel
    {
        //private readonly string connStr = "Server=tcp:yindbserver.database.windows.net;Authentication=Active Directory Default;Database=project_db";
        private readonly string connStr = "Server=tcp:yindbserver.database.windows.net,1433;Initial Catalog=project_db;Persist Security Info=False;User ID=yin;Password=1qaz!QAZ;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";



        public List<adminAccount> getadminAccounts()
        {
            List<adminAccount> accounts = new List<adminAccount>();

            SqlConnection sqlConnection = new SqlConnection(connStr);
            SqlCommand sqlCommand = new SqlCommand("SELECT * FROM admin");
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
    }
}
