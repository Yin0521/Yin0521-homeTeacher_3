using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Principal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;



namespace project.Models
{
    public class OrderService
    {
        private readonly string connStr;
        public OrderService(IConfiguration configuration)
        {
            connStr = configuration.GetConnectionString("DefaultConnection");
        }


        

        public List<OrderViewModel> GetOrdersByTeacher(int teacherId)
        {
            var list = new List<OrderViewModel>();
            using (var conn = new SqlConnection(connStr))
            {
                string sql = @"
                    SELECT o.*, s.Name AS StudentName, t.Name AS TeacherName, sub.Name AS SubjectName
                    FROM [Order] o
                    LEFT JOIN Student s ON o.StudentID = s.ID
                    LEFT JOIN Teacher t ON o.TeacherID = t.ID
                    LEFT JOIN Subject sub ON o.SubjectID = sub.ID
                    WHERE o.TeacherID = @TeacherID
                    ORDER BY o.CreateTime DESC";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TeacherID", teacherId);
                    conn.Open();
                    var dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        list.Add(ReadOrder(dr));
                    }
                }
            }
            return list;
        }

        // 查詢學生所有訂單
        public List<OrderViewModel> GetOrdersByStudent(int studentId)
        {
            var list = new List<OrderViewModel>();
            using (var conn = new SqlConnection(connStr))
            {
                string sql = @"
                    SELECT o.*, s.Name AS StudentName, t.Name AS TeacherName, sub.Name AS SubjectName
                    FROM [Order] o
                    LEFT JOIN Student s ON o.StudentID = s.ID
                    LEFT JOIN Teacher t ON o.TeacherID = t.ID
                    LEFT JOIN Subject sub ON o.SubjectID = sub.ID
                    WHERE o.StudentID = @StudentID
                    ORDER BY o.CreateTime DESC";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@StudentID", studentId);
                    conn.Open();
                    var dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        list.Add(ReadOrder(dr));
                    }
                }
            }
            return list;
        }

        // 查詢訂單詳細資訊（包含聯絡方式等）
        public OrderViewModel GetOrderDetail(int orderId)
        {
            using (var conn = new SqlConnection(connStr))
            {
                string sql = @"
                SELECT 
                    o.OrderID, o.StudentID, o.TeacherID, o.SubjectID,
                    o.Message, o.OrderStatus, o.CreateTime,
                    o.TeacherConfirmTime, o.StudentConfirmTime,
                    o.FinishTime, o.CancelTime, o.ReserveTime,
                    o.StudentNote, o.TeacherNote,
                    o.Price, o.ContactPhone, o.ContactLine, o.ContactEmail, o.MeetingType,

                    s.Name AS StudentName, s.Phone AS StudentPhone, s.Email AS StudentEmail,

                    t.Name AS TeacherName, t.Phone AS TeacherPhone, t.Email AS TeacherEmail,

                    subj.Name AS SubjectName
                FROM [Order] o
                JOIN Student s ON o.StudentID = s.ID
                JOIN Teacher t ON o.TeacherID = t.ID
                JOIN subject subj ON o.SubjectID = subj.Id
                WHERE o.OrderID = @OrderID;
                ";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@OrderID", orderId);
                conn.Open();
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new OrderViewModel
                    {
                        OrderID = Convert.ToInt32(reader["OrderID"]),
                        StudentID = Convert.ToInt32(reader["StudentID"]),
                        TeacherID = Convert.ToInt32(reader["TeacherID"]),
                        SubjectID = Convert.ToInt32(reader["SubjectID"]),

                        Message = reader["Message"]?.ToString(),
                        OrderStatus = (OrderStatus)Convert.ToInt32(reader["OrderStatus"]),
                        CreateTime = reader["CreateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["CreateTime"]),
                        TeacherConfirmTime = reader["TeacherConfirmTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["TeacherConfirmTime"]),
                        StudentConfirmTime = reader["StudentConfirmTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["StudentConfirmTime"]),
                        FinishTime = reader["FinishTime"] as DateTime?,
                        CancelTime = reader["CancelTime"] as DateTime?,
                        ReserveTime = reader["ReserveTime"] as DateTime?,

                        StudentName = reader["StudentName"] == DBNull.Value ? null : reader["StudentName"].ToString(),
                        StudentPhone = reader["StudentPhone"] == DBNull.Value ? null : reader["StudentPhone"].ToString(),
                        StudentEmail = reader["StudentEmail"] == DBNull.Value ? null : reader["StudentEmail"].ToString(),

                        TeacherName = reader["TeacherName"] == DBNull.Value ? null : reader["TeacherName"].ToString(),
                        TeacherPhone = reader["TeacherPhone"] == DBNull.Value ? null : reader["TeacherPhone"].ToString(),
                        TeacherEmail = reader["TeacherEmail"] == DBNull.Value ? null : reader["TeacherEmail"].ToString(),

                        SubjectName = reader["SubjectName"] == DBNull.Value ? null : reader["SubjectName"].ToString()
                    };
                }
            }

            return null;
        }

        // 共用轉換 function
        private OrderViewModel ReadOrder(SqlDataReader dr)
        {
            return new OrderViewModel
            {
                OrderID = (int)dr["OrderID"],
                StudentID = (int)dr["StudentID"],
                TeacherID = (int)dr["TeacherID"],
                SubjectID = (int)dr["SubjectID"],
                StudentName = dr["StudentName"].ToString(),
                TeacherName = dr["TeacherName"].ToString(),
                SubjectName = dr["SubjectName"].ToString(),
                Message = dr["Message"].ToString(),
                OrderStatus = (OrderStatus)Convert.ToInt32(dr["OrderStatus"]),
                CreateTime = dr["CreateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["CreateTime"]),
                TeacherConfirmTime = dr["TeacherConfirmTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["TeacherConfirmTime"]),
                StudentConfirmTime = dr["StudentConfirmTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["StudentConfirmTime"]),
                FinishTime = dr["FinishTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["FinishTime"]),
                CancelTime = dr["CancelTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["CancelTime"]),
                ReserveTime = dr["ReserveTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["ReserveTime"]),
                StudentNote = dr["StudentNote"].ToString(),
                TeacherNote = dr["TeacherNote"].ToString(),
                Price = dr["Price"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(dr["Price"]),
                ContactPhone = dr["ContactPhone"].ToString(),
                ContactLine = dr["ContactLine"].ToString(),
                ContactEmail = dr["ContactEmail"].ToString(),
                MeetingType = dr["MeetingType"].ToString()
            };
        }

        // 查詢「學生」不同狀態分類的訂單
        public List<OrderViewModel> GetOrdersByStudentAndStatus(int studentId, List<OrderStatus> statusList)
        {
            var list = new List<OrderViewModel>();
            using (var conn = new SqlConnection(connStr))
            {
                // 用IN查詢多個狀態
                string sql = $@"
                SELECT o.*, s.Name AS StudentName, t.Name AS TeacherName, sub.Name AS SubjectName
                FROM [Order] o
                LEFT JOIN Student s ON o.StudentID = s.ID
                LEFT JOIN Teacher t ON o.TeacherID = t.ID
                LEFT JOIN Subject sub ON o.SubjectID = sub.ID
                WHERE o.StudentID = @StudentID
                  AND o.OrderStatus IN ({string.Join(",", statusList.Select(s => ((int)s)).ToArray())})
                ORDER BY o.CreateTime DESC";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@StudentID", studentId);
                    conn.Open();
                    var dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        list.Add(ReadOrder(dr));
                    }
                }
            }
            return list;
        }

        // 查詢「老師」不同狀態分類的訂單
        public List<OrderViewModel> GetOrdersByTeacherAndStatus(int teacherId, List<OrderStatus> statusList)
        {
            var list = new List<OrderViewModel>();
            using (var conn = new SqlConnection(connStr))
            {
                // 用IN查詢多個狀態
                string sql = $@"
                SELECT o.*, s.Name AS StudentName, t.Name AS TeacherName, sub.Name AS SubjectName
                FROM [Order] o
                LEFT JOIN Student s ON o.StudentID = s.ID
                LEFT JOIN Teacher t ON o.TeacherID = t.ID
                LEFT JOIN Subject sub ON o.SubjectID = sub.ID
                WHERE o.TeacherID = @TeacherID
                  AND o.OrderStatus IN ({string.Join(",", statusList.Select(s => ((int)s)).ToArray())})
                ORDER BY o.CreateTime DESC";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TeacherID", teacherId);
                    conn.Open();
                    var dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        list.Add(ReadOrder(dr));
                    }
                }
            }
            return list;
        }

    }
}
