
using RollCall.Models;

namespace RollCall.Models.ViewModels
{
    public class CourseSessionsVM
    {
        public Course Course { get; set; } = default!;
        public List<AttendanceSession> Sessions { get; set; } = new List<AttendanceSession>();
    }
}
