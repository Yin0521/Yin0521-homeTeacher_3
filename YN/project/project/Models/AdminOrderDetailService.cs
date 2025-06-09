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
                        // 先抓出資料
                        var vm = new AdminOrderDetailViewModel
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
                            Message = dr["Message"].ToString(),

                            CreateTime = dr["CreateTime"] == DBNull.Value ? null : (DateTime?)dr["CreateTime"],
                            TeacherConfirmTime = dr["TeacherConfirmTime"] == DBNull.Value ? null : (DateTime?)dr["TeacherConfirmTime"],
                            StudentConfirmTime = dr["StudentConfirmTime"] == DBNull.Value ? null : (DateTime?)dr["StudentConfirmTime"],
                            FinishTime = dr["FinishTime"] == DBNull.Value ? null : (DateTime?)dr["FinishTime"],
                            CancelTime = dr["CancelTime"] == DBNull.Value ? null : (DateTime?)dr["CancelTime"],
                            TeacherFinishTime = dr["TeacherFinishTime"] == DBNull.Value ? null : (DateTime?)dr["TeacherFinishTime"],
                            StudentFinishTime = dr["StudentFinishTime"] == DBNull.Value ? null : (DateTime?)dr["StudentFinishTime"],
                            Price = dr["Price"] == DBNull.Value ? null : (int?)dr["Price"]
                        };

                        // 自動修正狀態
                        vm.OrderStatus = DetectOrderStatus(vm);

                        // 轉換為文字
                        vm.OrderStatusText = StatusText((int)vm.OrderStatus);

                        return vm;
                    }
                }
            }
            return null;
        }

        private OrderStatus DetectOrderStatus(AdminOrderDetailViewModel vm)
        {
            if (vm.CancelTime != null)
            {
                // 你可以另外補 vm.CancelReason 來判斷是學生取消還是老師拒絕
                return OrderStatus.StudentCancelled; // 或 TeacherRejected
            }

            if (vm.FinishTime != null) return OrderStatus.Finished;
            if (vm.StudentFinishTime != null) return OrderStatus.StudentCompleted;
            if (vm.TeacherFinishTime != null) return OrderStatus.TeacherCompleted;
            if (vm.StudentConfirmTime != null) return OrderStatus.Confirmed;
            if (vm.TeacherConfirmTime != null) return OrderStatus.Accepted;

            return OrderStatus.Pending;
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
