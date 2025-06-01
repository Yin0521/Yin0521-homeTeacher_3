using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Security.Principal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;



namespace project.Models
{
    public class DashboardService
    {
        private readonly string connStr;

        public DashboardService(IConfiguration configuration)
        {
            connStr = configuration.GetConnectionString("DefaultConnection");
        }

        public DashboardStats GetDashboardStats()
        {
            var stats = new DashboardStats();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // 老師總數
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Teacher", conn))
                    stats.TotalTeachers = (int)cmd.ExecuteScalar();

                // 學生總數
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Student", conn))
                    stats.TotalStudents = (int)cmd.ExecuteScalar();

                // 媒合成功數量（Orders表已完成）
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [Order] WHERE OrderStatus = 5", conn))
                    stats.MatchedOrders = (int)cmd.ExecuteScalar();
            }
            stats.ServerUptime = GetServerUptime();

            return stats;
        }
        private string GetServerUptime()
        {
            TimeSpan uptime = DateTime.Now - Process.GetCurrentProcess().StartTime;
            // 轉成「幾天 幾小時 幾分鐘」
            return $"{(int)uptime.TotalDays}天 {uptime.Hours}小時 {uptime.Minutes}分";
        }
    }
}
