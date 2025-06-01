using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Principal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;



namespace project.Models
{
    public class TokenModel
    {
        private readonly string connStr;
        public TokenModel(IConfiguration configuration)
        {
            connStr = configuration.GetConnectionString("DefaultConnection");
        }

        // 初始化錢包，首次註冊送 300 點
        public void InitializeWallet(int userId, string userType)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // 建立 TokenWallet 並初始化 300 點
                var cmdWallet = new SqlCommand(@"
                    INSERT INTO TokenWallet (UserID, UserType, Balance)
                    VALUES (@UserID, @UserType, 300)", conn);

                cmdWallet.Parameters.AddWithValue("@UserID", userId);
                cmdWallet.Parameters.AddWithValue("@UserType", userType);
                cmdWallet.ExecuteNonQuery();

                // 建立交易紀錄
                var cmdTx = new SqlCommand(@"
                    INSERT INTO TokenTransaction (UserID, UserType, ChangeAmount, Reason)
                    VALUES (@UserID, @UserType, 300, N'首次註冊贈送')", conn);

                cmdTx.Parameters.AddWithValue("@UserID", userId);
                cmdTx.Parameters.AddWithValue("@UserType", userType);
                cmdTx.ExecuteNonQuery();
            }
        }
    }
}
