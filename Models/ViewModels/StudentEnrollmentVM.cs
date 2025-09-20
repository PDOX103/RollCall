
namespace RollCall.Models.ViewModels
{
    public class StudentEnrollmentVM
    {
        public int EnrollmentId { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; }  // Ensure this property exists
        public string TeacherName { get; set; }  // Ensure this property exists
        public DateTime EnrolledAt { get; set; }
        public float? Grade { get; set; }
    }
}
