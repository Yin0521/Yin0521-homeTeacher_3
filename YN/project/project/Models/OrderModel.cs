using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Principal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;



namespace project.Models
{
    public class OrderModel
    {
        private readonly string connStr;
        public OrderModel(IConfiguration configuration)
        {
            connStr = configuration.GetConnectionString("DefaultConnection");
        }

        // 新增訂單（學生下單）
        public int InsertOrder(Order order)
        {
            using (var conn = new SqlConnection(connStr))
            {
                string sql = @"INSERT INTO [Order]
                        (StudentID, TeacherID, SubjectID, Message, OrderStatus, CreateTime)
                        VALUES (@StudentID, @TeacherID, @SubjectID, @Message, 0, GETDATE());
                        SELECT SCOPE_IDENTITY();";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@StudentID", order.StudentID);
                cmd.Parameters.AddWithValue("@TeacherID", order.TeacherID);
                cmd.Parameters.AddWithValue("@SubjectID", order.SubjectID);
                cmd.Parameters.AddWithValue("@Message", order.Message ?? "");
                conn.Open();
                int newId = Convert.ToInt32(cmd.ExecuteScalar());
                return newId;
            }
        }

        // 查詢某位老師所有訂單
        public List<Order> GetOrdersByTeacher(int teacherId)
        {
            var list = new List<Order>();
            using (var conn = new SqlConnection(connStr))
            {
                string sql = "SELECT * FROM [Order] WHERE TeacherID = @TeacherID ORDER BY CreateTime DESC";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@TeacherID", teacherId);
                conn.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(ParseOrder(reader));
                }
            }
            return list;
        }

        // 查詢某位學生所有訂單
        public List<Order> GetOrdersByStudent(int studentId)
        {
            var list = new List<Order>();
            using (var conn = new SqlConnection(connStr))
            {
                string sql = "SELECT * FROM [Order] WHERE StudentID = @StudentID ORDER BY CreateTime DESC";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@StudentID", studentId);
                conn.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(ParseOrder(reader));
                }
            }
            return list;
        }

        // 根據訂單 ID 查詢
        public Order GetOrder(int orderId)
        {
            using (var conn = new SqlConnection(connStr))
            {
                string sql = "SELECT * FROM [Order] WHERE OrderID = @OrderID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@OrderID", orderId);
                conn.Open();
                var reader = cmd.ExecuteReader();
                if (reader.Read()) return ParseOrder(reader);
                return null;
            }
        }

        // 這個 ParseOrder 支援所有狀態
        private Order ParseOrder(SqlDataReader reader)
        {
            return new Order
            {
                OrderID = Convert.ToInt32(reader["OrderID"]),
                StudentID = Convert.ToInt32(reader["StudentID"]),
                TeacherID = Convert.ToInt32(reader["TeacherID"]),
                SubjectID = Convert.ToInt32(reader["SubjectID"]),
                Message = reader["Message"]?.ToString(),
                OrderStatus = (OrderStatus)Convert.ToInt32(reader["OrderStatus"]),
                CreateTime = Convert.ToDateTime(reader["CreateTime"]),
                TeacherConfirmTime = reader["TeacherConfirmTime"] as DateTime?,
                StudentConfirmTime = reader["StudentConfirmTime"] as DateTime?,
                FinishTime = reader["FinishTime"] as DateTime?,
                CancelTime = reader["CancelTime"] as DateTime?
            };
        }

        // 老師同意訂單
        public void TeacherConfirm(int orderId)
        {
            using (var conn = new SqlConnection(connStr))
            {
                string sql = "UPDATE [Order] SET OrderStatus = @Status, TeacherConfirmTime = GETDATE() WHERE OrderID = @OrderID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@OrderID", orderId);
                cmd.Parameters.AddWithValue("@Status", (int)OrderStatus.Accepted);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // 學生確認訂單
        public void StudentConfirm(int orderId)
        {
            using (var conn = new SqlConnection(connStr))
            {
                string sql = "UPDATE [Order] SET OrderStatus = @Status, StudentConfirmTime = GETDATE() WHERE OrderID = @OrderID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@OrderID", orderId);
                cmd.Parameters.AddWithValue("@Status", (int)OrderStatus.Confirmed);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ====== 雙方完成訂單 ======
        // 老師完成
        public void TeacherFinish(int orderId)
        {
            var order = GetOrder(orderId);
            OrderStatus nextStatus;
            if (order.OrderStatus == OrderStatus.Confirmed)
                nextStatus = OrderStatus.TeacherCompleted;
            else if (order.OrderStatus == OrderStatus.StudentCompleted)
                nextStatus = OrderStatus.Finished;
            else
                return;

            using (var conn = new SqlConnection(connStr))
            {
                string sql;
                if (nextStatus == OrderStatus.TeacherCompleted)
                {
                    sql = "UPDATE [Order] SET OrderStatus = @Status, TeacherFinishTime = GETDATE() WHERE OrderID = @OrderID";
                }
                else
                {
                    sql = "UPDATE [Order] SET OrderStatus = @Status, TeacherFinishTime = GETDATE(), FinishTime = GETDATE() WHERE OrderID = @OrderID";
                }

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@OrderID", orderId);
                cmd.Parameters.AddWithValue("@Status", (int)nextStatus);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // 學生完成
        public void StudentFinish(int orderId)
        {
            var order = GetOrder(orderId);
            OrderStatus nextStatus;
            if (order.OrderStatus == OrderStatus.Confirmed)
                nextStatus = OrderStatus.StudentCompleted;
            else if (order.OrderStatus == OrderStatus.TeacherCompleted)
                nextStatus = OrderStatus.Finished;
            else
                return;

            using (var conn = new SqlConnection(connStr))
            {
                string sql;
                if (nextStatus == OrderStatus.StudentCompleted)
                {
                    sql = "UPDATE [Order] SET OrderStatus = @Status, StudentFinishTime = GETDATE() WHERE OrderID = @OrderID";
                }
                else
                {
                    sql = "UPDATE [Order] SET OrderStatus = @Status, StudentFinishTime = GETDATE(), FinishTime = GETDATE() WHERE OrderID = @OrderID";
                }

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@OrderID", orderId);
                cmd.Parameters.AddWithValue("@Status", (int)nextStatus);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // 學生取消
        public void CancelOrder(int orderId)
        {
            using (var conn = new SqlConnection(connStr))
            {
                string sql = "UPDATE [Order] SET OrderStatus = @Status, CancelTime = GETDATE() WHERE OrderID = @OrderID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@OrderID", orderId);
                cmd.Parameters.AddWithValue("@Status", (int)OrderStatus.StudentCancelled);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // 老師拒絕
        public void RejectOrder(int orderId)
        {
            using (var conn = new SqlConnection(connStr))
            {
                string sql = "UPDATE [Order] SET OrderStatus = @Status, CancelTime = GETDATE() WHERE OrderID = @OrderID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@OrderID", orderId);
                cmd.Parameters.AddWithValue("@Status", (int)OrderStatus.TeacherRejected);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }

}
