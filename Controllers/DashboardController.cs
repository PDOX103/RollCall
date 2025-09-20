using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RollCall.Models;

namespace RollCall.Controllers
{
    public class DashboardController : Controller
    {
        private readonly RollCallDbContext _context;

        public DashboardController(RollCallDbContext context)
        {
            _context = context;
        }

        // ---------------- STUDENT DASHBOARD ----------------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userEmail) || userRole != "Student")
            {
                TempData["ToastMessage"] = "Access denied. Student access required.";
                TempData["ToastType"] = "error";
                return RedirectToAction("Index", "Home");
            }

            var student = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (student == null)
            {
                TempData["ToastMessage"] = "Student not found.";
                TempData["ToastType"] = "error";
                return RedirectToAction("SignIn", "User");
            }

            // Get all enrollments for this student
            var courses = await _context.Enrollments
                .Where(e => e.StudentId == student.Id)
                .Include(e => e.Course)
                .ToListAsync();

            var dashboardData = new List<DashboardVM>();

            foreach (var enrollment in courses)
            {
                var totalSessions = await _context.AttendanceSessions
                    .Where(s => s.CourseId == enrollment.CourseId)
                    .CountAsync();

                var attendedSessions = await _context.AttendanceRecords
                    .Where(r => r.StudentId == student.Id && r.Session.CourseId == enrollment.CourseId)
                    .CountAsync();

                double percentage = (totalSessions > 0)
                    ? Math.Round((attendedSessions * 100.0) / totalSessions, 2)
                    : 0;

                dashboardData.Add(new DashboardVM
                {
                    CourseName = enrollment.Course.Name,
                    TotalSessions = totalSessions,
                    AttendedSessions = attendedSessions,
                    AttendancePercentage = percentage
                });
            }

            return View(dashboardData);
        }
    }

    // ViewModel for Dashboard
    public class DashboardVM
    {
        public string CourseName { get; set; }
        public int TotalSessions { get; set; }
        public int AttendedSessions { get; set; }
        public double AttendancePercentage { get; set; }
    }
}
