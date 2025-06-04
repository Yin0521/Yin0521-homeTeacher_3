using System.Data;
using System.Data.SqlClient;
using Dapper;

public class FeedbackModel
{
    private readonly string _connStr;

    public FeedbackModel(IConfiguration config)
    {
        _connStr = config.GetConnectionString("DefaultConnection");
    }

    public List<Feedback> GetAll() =>
        new SqlConnection(_connStr).Query<Feedback>("SELECT * FROM Feedbacks ORDER BY CreatedAt DESC").ToList();

    public void MarkHandled(int id) =>
        new SqlConnection(_connStr).Execute("UPDATE Feedbacks SET IsHandled = 1 WHERE Id = @id", new { id });

    public void Delete(int id) =>
        new SqlConnection(_connStr).Execute("DELETE FROM Feedbacks WHERE Id = @id", new { id });
}

