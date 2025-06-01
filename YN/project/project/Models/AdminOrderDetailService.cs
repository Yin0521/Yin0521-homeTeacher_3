using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Principal;
using Microsoft.AspNetCore.Http.HttpResults;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;



namespace project.Models
{
    public class AdminOrderDetailService
    {
        private readonly string connStr;
        public AdminOrderDetailService(IConfiguration configuration)
        {
            connStr = configuration.GetConnectionString("DefaultConnection");
        }

        public AdminOrderDetailViewModel GetAdminOrderDetail(int orderId)
        {
            using (var conn = new SqlConnection(connStr))
            {
                string sql = @"
            SELECT o.*, 
                   s.Name AS StudentName, s.Email AS StudentEmail, s.Phone AS StudentPhone,
                   t.Name AS TeacherName, t.Email AS TeacherEmail, t.Phone AS TeacherPhone,
                   sub.Name AS SubjectName
            FROM [Order] o
            LEFT JOIN Student s ON o.StudentID = s.ID
            LEFT JOIN Teacher t ON o.TeacherID = t.ID
            LEFT JOIN Subject sub ON o.SubjectID = sub.ID
            WHERE o.OrderID = @OrderID";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@OrderID", orderId);
                    conn.Open();
                    var dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        return new AdminOrderDetailViewModel
                        {
                            OrderID = (int)dr["OrderID"],
                            StudentID = (int)dr["StudentID"],
                            StudentName = dr["StudentName"].ToString(),
                            StudentEmail = dr["StudentEmail"].ToString(),
                            StudentPhone = dr["StudentPhone"].ToString(),
                            TeacherID = (int)dr["TeacherID"],
                            TeacherName = dr["TeacherName"].ToString(),
                            TeacherEmail = dr["TeacherEmail"].ToString(),
                            TeacherPhone = dr["TeacherPhone"].ToString(),
                            SubjectName = dr["SubjectName"].ToString(),
                            OrderStatusText = StatusText((int)dr["OrderStatus"]),
                            Message = dr["Message"].ToString(),
                            CreateTime = dr["CreateTime"] == DBNull.Value ? null : (DateTime?)dr["CreateTime"],
                            TeacherConfirmTime = dr["TeacherConfirmTime"] == DBNull.Value ? null : (DateTime?)dr["TeacherConfirmTime"],
                            StudentConfirmTime = dr["StudentConfirmTime"] == DBNull.Value ? null : (DateTime?)dr["StudentConfirmTime"],
                            FinishTime = dr["FinishTime"] == DBNull.Value ? null : (DateTime?)dr["FinishTime"],
                            CancelTime = dr["CancelTime"] == DBNull.Value ? null : (DateTime?)dr["CancelTime"],
                            StudentNote = dr["StudentNote"].ToString(),
                            TeacherNote = dr["TeacherNote"].ToString(),
                            TeacherFinishTime = dr["TeacherFinishTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["TeacherFinishTime"]),
                            StudentFinishTime = dr["StudentFinishTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["StudentFinishTime"]),

                            Price = dr["Price"] == DBNull.Value ? null : (decimal?)dr["Price"]
                        };
                    }
                }
            }
            return null;
        }

        private string StatusText(int status)
        {
            switch (status)
            {
                case 0: return "待老師確認";
                case 1: return "待學生確認";
                case 2: return "進行中";
                case 3: return "老師已完成";
                case 4: return "學生已完成";
                case 5: return "訂單已完成";
                case 6: return "學生取消";
                case 7: return "老師拒絕";
                default: return "未知";
            }
        }
    }
}
