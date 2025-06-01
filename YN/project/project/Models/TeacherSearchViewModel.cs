using project.Models;

public class TeacherSearchViewModel
{
    public string Subject { get; set; }
    public string City { get; set; }

    public int? SelectedSubjectId { get; set; }
    public string SelectedCity { get; set; }

    // 下拉選單資料來源
    public List<Subject> Subjects { get; set; } = new List<Subject>();
    public List<string> Cities { get; set; } = new List<string>(); // 如果要支援城市下拉

    // 搜尋結果
    public List<TeacherResult> Results { get; set; } = new List<TeacherResult>();
}
public class TeacherResult
{
    public int TeacherId { get; set; }
    public string Name { get; set; }
    public string SubjectNames { get; set; }
    // 你需要顯示的欄位都可以加
}
