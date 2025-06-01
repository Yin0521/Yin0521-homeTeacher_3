namespace project.Models
{
    public class RecommendedTeacher
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SubjectSpecialty { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        
        public string? PhotoPath { get; set; }//頭像 
        public List<string> Subjects { get; set; } = new(); //多科目名稱

        public string DisplayPhoto
        {
            get
            {
                return string.IsNullOrEmpty(PhotoPath) ? "/uploads/Blank_Avatar.png" : PhotoPath;
            }
        }

    }
}

