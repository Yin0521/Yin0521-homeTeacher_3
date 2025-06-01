using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Principal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;



namespace project.Models
{
    public class AdminOrderService
    {
        private readonly string connStr;
        public AdminOrderService(IConfiguration configuration)
        {
            connStr = configuration.GetConnectionString("DefaultConnection");
        }
        /// 取得所有訂單資料(後台管理用)
        public List<AdminOrderViewModel> GetAllOrders()
        {
            var list = new List<AdminOrderViewModel>();
            using (var conn = new SqlConnection(connStr))
            {
                string sql = @"
                    SELECT o.OrderID, o.StudentID, o.TeacherID, s.Name AS StudentName, t.Name AS TeacherName, sub.Name AS SubjectName, 
                    o.OrderStatus, o.CreateTime
                    FROM [Order] o
                    LEFT JOIN Student s ON o.StudentID = s.ID
                    LEFT JOIN Teacher t ON o.TeacherID = t.ID
                    LEFT JOIN Subject sub ON o.SubjectID = sub.ID
                    ORDER BY o.CreateTime DESC";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    var dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        list.Add(new AdminOrderViewModel
                        {
                            OrderID = (int)dr["OrderID"],
                            StudentID = dr["StudentID"] == DBNull.Value ? 0 : (int)dr["StudentID"],
                            TeacherID = dr["TeacherID"] == DBNull.Value ? 0 : (int)dr["TeacherID"],
                            StudentName = dr["StudentName"].ToString(),
                            TeacherName = dr["TeacherName"].ToString(),
                            SubjectName = dr["SubjectName"].ToString(),
                            OrderStatus = (OrderStatus)Convert.ToInt32(dr["OrderStatus"]),
                            StatusText = StatusText(Convert.ToInt32(dr["OrderStatus"])),
                            CreateTime = dr["CreateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["CreateTime"])
                        });
                    }
                }
            }
            return list;
        }
        // 未處理（待老師確認）
        public List<AdminOrderViewModel> GetPendingOrders()
        => GetOrdersByStatus(new[] { OrderStatus.Pending });

        // 待確認（待學生確認）
        public List<AdminOrderViewModel> GetAcceptedOrders()
            => GetOrdersByStatus(new[] { OrderStatus.Accepted });
        public List<AdminOrderViewModel> GetToBeFinishedOrders()
            => GetOrdersByStatus(new[] { OrderStatus.Confirmed, OrderStatus.TeacherCompleted, OrderStatus.StudentCompleted });
        //已完成（雙方都完成）
        public List<AdminOrderViewModel> GetFinishedOrders()
            => GetOrdersByStatus(new[] { OrderStatus.Finished });

        // 已取消（學生取消、老師拒絕都算）
        public List<AdminOrderViewModel> GetCancelledOrders()
            => GetOrdersByStatus(new[] { OrderStatus.StudentCancelled, OrderStatus.TeacherRejected });

        // 通用方法
        private List<AdminOrderViewModel> GetOrdersByStatus(OrderStatus[] statusList)
        {
            var list = new List<AdminOrderViewModel>();
            using (var conn = new SqlConnection(connStr))
            {
                string sql = @"
                SELECT o.OrderID, o.StudentID, o.TeacherID, s.Name AS StudentName, t.Name AS TeacherName, sub.Name AS SubjectName, 
                    o.OrderStatus, o.CreateTime
                FROM [Order] o
                LEFT JOIN Student s ON o.StudentID = s.ID
                LEFT JOIN Teacher t ON o.TeacherID = t.ID
                LEFT JOIN Subject sub ON o.SubjectID = sub.ID
            ";

                if (statusList != null && statusList.Length > 0)
                {
                    string inClause = string.Join(", ", statusList.Select(s => ((int)s).ToString()));
                    sql += $" WHERE o.OrderStatus IN ({inClause})";
                }

                sql += " ORDER BY o.CreateTime DESC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    var dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        list.Add(new AdminOrderViewModel
                        {
                            OrderID = (int)dr["OrderID"],
                            StudentID = dr["StudentID"] == DBNull.Value ? 0 : (int)dr["StudentID"],
                            TeacherID = dr["TeacherID"] == DBNull.Value ? 0 : (int)dr["TeacherID"],
                            StudentName = dr["StudentName"].ToString(),
                            TeacherName = dr["TeacherName"].ToString(),
                            SubjectName = dr["SubjectName"].ToString(),
                            OrderStatus = (OrderStatus)Convert.ToInt32(dr["OrderStatus"]),
                            StatusText = StatusText(Convert.ToInt32(dr["OrderStatus"])),
                            CreateTime = dr["CreateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["CreateTime"])
                        });
                    }
                }
            }
            return list;
        }
        //刪除訂單
        public bool DeleteOrder(int orderId)
        {
            using var conn = new SqlConnection(connStr);
            conn.Open();
            var sql = "DELETE FROM [Order] WHERE OrderID = @OrderID";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@OrderID", orderId);
            return cmd.ExecuteNonQuery() > 0;
        }
        // 搜尋訂單歷史紀錄（學生或老師）
        public List<AdminOrderViewModel> SearchOrderHistory(string studentKeyword, string teacherKeyword, string orderKeyword)
        {
            var list = new List<AdminOrderViewModel>();
            using (var conn = new SqlConnection(connStr))
            {
                // 組裝SQL
                string sql = @"
            SELECT o.OrderID, o.StudentID, o.TeacherID, s.Name AS StudentName, t.Name AS TeacherName, sub.Name AS SubjectName, 
                o.OrderStatus, o.CreateTime
            FROM [Order] o
            LEFT JOIN Student s ON o.StudentID = s.ID
            LEFT JOIN Teacher t ON o.TeacherID = t.ID
            LEFT JOIN Subject sub ON o.SubjectID = sub.ID
            WHERE 1=1
        ";

                var cmd = new SqlCommand();
                // Student Keyword
                if (!string.IsNullOrWhiteSpace(studentKeyword))
                {
                    sql += " AND (s.Name LIKE @StudentName";
                    cmd.Parameters.AddWithValue("@StudentName", "%" + studentKeyword + "%");
                    // 嘗試判斷是否為純數字（ID）
                    if (int.TryParse(studentKeyword, out int studentId))
                    {
                        sql += " OR o.StudentID = @StudentId";
                        cmd.Parameters.AddWithValue("@StudentId", studentId);
                    }
                    sql += ")";
                }
                // Teacher Keyword
                if (!string.IsNullOrWhiteSpace(teacherKeyword))
                {
                    sql += " AND (t.Name LIKE @TeacherName";
                    cmd.Parameters.AddWithValue("@TeacherName", "%" + teacherKeyword + "%");
                    if (int.TryParse(teacherKeyword, out int teacherId))
                    {
                        sql += " OR o.TeacherID = @TeacherId";
                        cmd.Parameters.AddWithValue("@TeacherId", teacherId);
                    }
                    sql += ")";
                }
                // Order Keyword
                if (!string.IsNullOrWhiteSpace(orderKeyword))
                {
                    if (int.TryParse(orderKeyword, out int orderId))
                    {
                        sql += " AND o.OrderID = @OrderId";
                        cmd.Parameters.AddWithValue("@OrderId", orderId);
                    }
                    else
                    {
                        // 如果不是數字，這邊可以不用加查詢（或自定義其他查詢方式）
                        // 通常訂單號應該只會查int
                    }
                }

                sql += " ORDER BY o.CreateTime DESC";
                cmd.CommandText = sql;
                cmd.Connection = conn;

                conn.Open();
                var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new AdminOrderViewModel
                    {
                        OrderID = (int)dr["OrderID"],
                        StudentID = dr["StudentID"] == DBNull.Value ? 0 : (int)dr["StudentID"],
                        TeacherID = dr["TeacherID"] == DBNull.Value ? 0 : (int)dr["TeacherID"],
                        StudentName = dr["StudentName"].ToString(),
                        TeacherName = dr["TeacherName"].ToString(),
                        SubjectName = dr["SubjectName"].ToString(),
                        OrderStatus = (OrderStatus)Convert.ToInt32(dr["OrderStatus"]),
                        StatusText = StatusText(Convert.ToInt32(dr["OrderStatus"])),
                        CreateTime = dr["CreateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["CreateTime"])
                    });
                }
            }
            return list;
        }

        private string StatusText(int status)
        {
            return status switch
            {
                0 => "待老師確認",
                1 => "待學生確認",
                2 => "已成立(待完成)",
                3 => "已完成",
                4 => "學生取消",
                5 => "老師拒絕",
                6 => "老師完成待學生",
                7 => "學生完成待老師",
                _ => ""
            };
        }
    }
}
