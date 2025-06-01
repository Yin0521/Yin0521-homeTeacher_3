using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace project.Models
{
    public class SubjectModel
    {
        private readonly string connStr;
        public SubjectModel(IConfiguration configuration)
        {
            connStr = configuration.GetConnectionString("DefaultConnection");
        }

        // 取得所有科目
        public List<Subject> GetAllSubjects()
        {
            List<Subject> subjects = new List<Subject>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "SELECT * FROM subject";
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Subject subject = new Subject
                    {
                        Id = (int)reader["Id"],
                        Name = reader["Name"].ToString(),
                        Description = reader["Description"].ToString()
                    };
                    subjects.Add(subject);
                }
            }
            return subjects;
        }

        // 新增科目
        public bool InsertSubject(Subject subject)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "INSERT INTO Subject (Name, Description) VALUES (@Name, @Description)";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", subject.Name);
                cmd.Parameters.AddWithValue("@Description", subject.Description ?? "");
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // 更新科目
        public bool UpdateSubject(Subject subject)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "UPDATE subject SET Name = @Name, Description = @Description WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", subject.Name);
                cmd.Parameters.AddWithValue("@Id", subject.Id);
                cmd.Parameters.AddWithValue("@Description", subject.Description ?? "");
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // 刪除科目
        public bool DeleteSubject(int id)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "DELETE FROM subject WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // 根據 ID 取得科目
        public Subject GetSubjectById(int id)
        {
            Subject subject = null;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "SELECT * FROM subject WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    subject = new Subject
                    {
                        Id = (int)reader["Id"],
                        Name = reader["Name"].ToString(),
                        Description = reader["Description"].ToString()
                    };
                }
            }
            return subject;
        }
    }
}
