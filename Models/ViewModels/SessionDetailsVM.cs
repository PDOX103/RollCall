// Models/ViewModels/SessionDetailsVM.cs
using RollCall.Models;

namespace RollCall.Models.ViewModels
{
    public class SessionDetailsVM
    {
        public AttendanceSession Session { get; set; } = default!;
        public List<AttendanceRecord> Records { get; set; } = new List<AttendanceRecord>();
    }
}
