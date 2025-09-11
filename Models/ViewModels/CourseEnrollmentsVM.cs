namespace RollCall.Models.ViewModels
{
    public class CourseEnrollmentsVM
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public List<EnrolledStudent> EnrolledStudents { get; set; } = new List<EnrolledStudent>();
    }

    public class EnrolledStudent
    {
        public int StudentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string StudentIdNumber { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime EnrolledAt { get; set; }
        public float? Grade { get; set; }
    }
}