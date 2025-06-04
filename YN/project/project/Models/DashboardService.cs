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

            // 計算當月 & 上月註冊數 & 百分比
            var monthStats = GetMonthlyRegisterStats();

            var thisMonth = DateTime.Now.ToString("yyyy-MM");
            var lastMonth = DateTime.Now.AddMonths(-1).ToString("yyyy-MM");

            var current = monthStats.FirstOrDefault(x => x.Month == thisMonth);
            var prev = monthStats.FirstOrDefault(x => x.Month == lastMonth);

            stats.CurrentMonthStudentRegister = current?.StudentCount ?? 0;
            stats.CurrentMonthTeacherRegister = current?.TeacherCount ?? 0;

            int lastMonthStudent = prev?.StudentCount ?? 0;
            int lastMonthTeacher = prev?.TeacherCount ?? 0;

            stats.StudentRegisterGrowthPercent = lastMonthStudent == 0 ?
                (stats.CurrentMonthStudentRegister > 0 ? 100 : 0) :
                ((double)(stats.CurrentMonthStudentRegister - lastMonthStudent) / lastMonthStudent * 100);

            stats.TeacherRegisterGrowthPercent = lastMonthTeacher == 0 ?
                (stats.CurrentMonthTeacherRegister > 0 ? 100 : 0) :
                ((double)(stats.CurrentMonthTeacherRegister - lastMonthTeacher) / lastMonthTeacher * 100);

            stats.ActiveStudentCount = GetActiveStudentCount();
            stats.ActiveTeacherCount = GetActiveTeacherCount();

            return stats;
        }

        private string GetServerUptime()
        {
            TimeSpan uptime = DateTime.Now - Process.GetCurrentProcess().StartTime;
            // 轉成「幾天 幾小時 幾分鐘」
            return $"{(int)uptime.TotalDays}天 {uptime.Hours}小時 {uptime.Minutes}分";
        }

        public List<MonthlyRegisterStats> GetMonthlyRegisterStats()
        {
            var result = new List<MonthlyRegisterStats>();
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                var sql = @"SELECT
                        Months.Month,
                        ISNULL(StudentCount, 0) AS StudentCount,
                        ISNULL(TeacherCount, 0) AS TeacherCount
                    FROM (
                        SELECT FORMAT(RegisterDate, 'yyyy-MM') AS Month FROM Student
                        WHERE RegisterDate >= DATEADD(MONTH, -12, GETDATE())
                        UNION
                        SELECT FORMAT(RegisterDate, 'yyyy-MM') AS Month FROM Teacher
                        WHERE RegisterDate >= DATEADD(MONTH, -12, GETDATE())
                    ) Months
                    LEFT JOIN (
                        SELECT FORMAT(RegisterDate, 'yyyy-MM') AS Month, COUNT(*) AS StudentCount
                        FROM Student
                        WHERE RegisterDate >= DATEADD(MONTH, -12, GETDATE())
                        GROUP BY FORMAT(RegisterDate, 'yyyy-MM')
                    ) S ON Months.Month = S.Month
                    LEFT JOIN (
                        SELECT FORMAT(RegisterDate, 'yyyy-MM') AS Month, COUNT(*) AS TeacherCount
                        FROM Teacher
                        WHERE RegisterDate >= DATEADD(MONTH, -12, GETDATE())
                        GROUP BY FORMAT(RegisterDate, 'yyyy-MM')
                    ) T ON Months.Month = T.Month
                    ORDER BY Months.Month";
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new MonthlyRegisterStats
                        {
                            Month = reader.GetString(0),
                            StudentCount = reader.GetInt32(1),
                            TeacherCount = reader.GetInt32(2)
                        });
                    }
                }
            }
            return result;
        }

        public int GetActiveStudentCount()
        {
            using var conn = new SqlConnection(connStr);
            conn.Open();
            string sql = "SELECT COUNT(*) FROM Student WHERE LastLoginTime >= DATEADD(day, -7, GETDATE())";
            using var cmd = new SqlCommand(sql, conn);
            return (int)cmd.ExecuteScalar();
        }

        public int GetActiveTeacherCount()
        {
            using var conn = new SqlConnection(connStr);
            conn.Open();
            string sql = "SELECT COUNT(*) FROM Teacher WHERE LastLoginTime >= DATEADD(day, -7, GETDATE())";
            using var cmd = new SqlCommand(sql, conn);
            return (int)cmd.ExecuteScalar();
        }

        public List<OrderStatusCount> GetThisMonthOrderStatusCounts()
        {
            var result = new List<OrderStatusCount>();
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                string sql = @"
            SELECT OrderStatus, COUNT(*) AS Count
            FROM [Order]
            WHERE CreateTime >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)
              AND CreateTime < DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0)
            GROUP BY OrderStatus";
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new OrderStatusCount
                        {
                            Status = reader.GetInt32(0),
                            Count = reader.GetInt32(1)
                        });
                    }
                }
            }
            return result;
        }

        // 老師挑選熱門科目（Top 5）
        public List<SubjectCountVM> GetTopSubjects(int topN = 5)
        {
            var result = new List<SubjectCountVM>();
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                string sql = @"
                SELECT TOP (@TopN) s.Name, COUNT(*) AS Count
                FROM Teacher ts
                JOIN subject s ON ts.Id = s.Id
                GROUP BY s.Name
                ORDER BY Count DESC";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TopN", topN);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new SubjectCountVM
                            {
                                Name = reader.GetString(0),
                                Count = reader.GetInt32(1)
                            });
                        }
                    }
                }
            }
            return result;
        }

        // 學生城市分布
        public List<CityCountVM> GetStudentCityCounts(int topN = 6)
        {
            var result = new List<CityCountVM>();
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                string sql = @"
                SELECT TOP (@TopN) City, COUNT(*) AS Count
                FROM Student
                GROUP BY City
                ORDER BY Count DESC";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TopN", topN);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new CityCountVM
                            {
                                City = reader.IsDBNull(0) ? "未填寫" : reader.GetString(0),
                                Count = reader.GetInt32(1)
                            });
                        }
                    }
                }
            }
            return result;
        }

        // 老師城市分布
        public List<CityCountVM> GetTeacherCityCounts(int topN = 6)
        {
            var result = new List<CityCountVM>();
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                string sql = @"
                SELECT TOP (@TopN) City, COUNT(*) AS Count
                FROM Teacher
                GROUP BY City
                ORDER BY Count DESC";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TopN", topN);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new CityCountVM
                            {
                                City = reader.IsDBNull(0) ? "未填寫" : reader.GetString(0),
                                Count = reader.GetInt32(1)
                            });
                        }
                    }
                }
            }
            return result;
        }

        public List<SubjectCountVM> GetTopOrderedSubjects(int topN = 5)
        {
            var result = new List<SubjectCountVM>();
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                string sql = @"
            SELECT TOP (@TopN) s.Name, COUNT(*) AS Count
            FROM [Order] o
            JOIN subject s ON o.SubjectID = s.Id
            GROUP BY s.Name
            ORDER BY Count DESC";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TopN", topN);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new SubjectCountVM
                            {
                                Name = reader.GetString(0),
                                Count = reader.GetInt32(1)
                            });
                        }
                    }
                }
            }
            return result;
        }

    }
}
