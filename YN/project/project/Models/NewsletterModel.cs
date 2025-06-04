using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace project.Models
{
    public class NewsletterModel
    {
        private readonly string connStr;
        public NewsletterModel(IConfiguration config)
        {
            connStr = config.GetConnectionString("DefaultConnection");
        }

        // 新增一筆訂閱
        public bool AddSubscriber(string email)
        {
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();

                // 防止重複訂閱
                var checkCmd = new SqlCommand("SELECT COUNT(*) FROM NewsletterSubscribers WHERE Email = @Email", conn);
                checkCmd.Parameters.AddWithValue("@Email", email);
                int count = (int)checkCmd.ExecuteScalar();
                if (count > 0) return false;

                var cmd = new SqlCommand("INSERT INTO NewsletterSubscribers (Email) VALUES (@Email)", conn);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.ExecuteNonQuery();
            }
            return true;
        }

        // 取得所有訂閱者（可給後台匯出/檢查用）
        public List<string> GetAllSubscribers()
        {
            var list = new List<string>();
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT Email FROM NewsletterSubscribers", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        list.Add(reader.GetString(0));
                }
            }
            return list;
        }

        public void DeleteSubscriber(int id)
        {
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                var cmd = new SqlCommand("DELETE FROM NewsletterSubscribers WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public List<NewsletterSubscriberViewModel> GetAllSubscribersWithId()
        {
            var list = new List<NewsletterSubscriberViewModel>();
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT Id, Email FROM NewsletterSubscribers", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        list.Add(new NewsletterSubscriberViewModel
                        {
                            Id = reader.GetInt32(0),
                            Email = reader.GetString(1)
                        });
                }
            }
            return list;
        }


    }
}
