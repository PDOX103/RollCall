using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RollCall.Helpers;
using RollCall.Models;
using RollCall.Models.ViewModels;

namespace RollCall.Controllers
{
    public class UserController : Controller
    {
        private readonly RollCallDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(RollCallDbContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ---------------- HOME ----------------
        public IActionResult Index()
        {
            return View();
        }

        // ---------------- SIGN IN ----------------
        [HttpGet]
        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignIn(string email, string password)
        {

            //Hardcoded Superuser
            if (email == "admin@gmail.com" && password == "123456")
            {
                HttpContext.Session.SetString("UserEmail", email);
                HttpContext.Session.SetString("UserRole", "Admin"); 
                HttpContext.Session.SetString("IsSuperUser", "true");

                _logger.LogInformation("UserEmail: {Email}, UserRole: {Role}",
    HttpContext.Session.GetString("UserEmail"),
    HttpContext.Session.GetString("UserRole"));

                TempData["Message"] = "Logged in as Superuser!";
                return RedirectToAction("AdminPage"); 
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                TempData["Message"] = "Account not created!";
                TempData["MessageType"] = "error";
                return View();
            }

            if (user.PasswordHash != password) 
            {
                TempData["Message"] = "Wrong password!";
                TempData["MessageType"] = "error";
                return View();
            }

            // Save session
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Role);

            TempData["Message"] = "Signed in successfully!";
            //TempData["MessageType"] = "success";

            // Redirect based on role
            if (user.Role == "Student")
                return RedirectToAction("StudentPage");
            else if (user.Role == "Teacher")
                return RedirectToAction("TeacherPage");
            else if (user.Role == "Admin")
                return RedirectToAction("AdminPage");

            return RedirectToAction("Index", "Home");
        }


        // ---------------- CHOOSE SIGN UP ----------------
        [HttpGet]
        public IActionResult Choose()
        {
            return View();
        }

        // ---------------- STUDENT SIGN UP ----------------
        [HttpGet]
        public IActionResult SignUpStudent()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignUpStudent(User model)
        {
            if (ModelState.IsValid)
            {
                model.Role = "Student";
                _context.Users.Add(model);
                _context.SaveChanges();

                HttpContext.Session.SetString("UserEmail", model.Email);
                HttpContext.Session.SetString("UserRole", model.Role);

                return RedirectToAction("StudentPage");
            }
            return View(model);
        }

        // ---------------- TEACHER SIGN UP ----------------
        [HttpGet]
        public IActionResult TeacherSignUp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult TeacherSignUp(User model)
        {
            if (ModelState.IsValid)
            {
                model.Role = "Teacher";
                _context.Users.Add(model);
                _context.SaveChanges();

                HttpContext.Session.SetString("UserEmail", model.Email);
                HttpContext.Session.SetString("UserRole", model.Role);

                return RedirectToAction("TeacherPage");
            }
            return View(model);
        }

        // ---------------- PROFILE (redirects to correct dashboard) ----------------
        public IActionResult Profile()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
                return RedirectToAction("SignIn");

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null) return RedirectToAction("SignIn");

            return View("Profile", user);
        }


        // ---------------- DASHBOARD PAGES ----------------
        public IActionResult StudentPage()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail && u.Role == "Student");
            if (user == null) return RedirectToAction("SignIn");
            return View(user);
        }

        public IActionResult TeacherPage()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail && u.Role == "Teacher");
            if (user == null) return RedirectToAction("SignIn");
            return View(user);
        }

        public IActionResult AdminPage()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");

            // Log the accessed information
            _logger.LogInformation("AdminPage accessed by {Email} with role {Role}", userEmail, userRole);

            if (string.IsNullOrEmpty(userEmail) || userRole != "Admin")
            {
                TempData["Message"] = "Access denied.";
                return RedirectToAction("SignIn");
            }

            // Retrieve user from the database
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            // Log the retrieved user information
            _logger.LogInformation("User retrieved: {User}", user);

            if (user == null)
            {
                return RedirectToAction("SignIn");
            }

            return View(user);
        }

        // ---------------- LOGOUT ----------------
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // ---------------- CREATE COURSE (GET) ----------------
        [HttpGet]
        public IActionResult CreateCourse()
        {
            // Ensure only teachers can access this
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Teacher")
            {
                TempData["Message"] = "Access denied.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index", "Home");
            }
            return View(new Course()); // Pass a new Course object to the view
        }

        // ---------------- CREATE COURSE (POST) ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(Course model)
        {
            _logger.LogInformation("### POST CreateCourse Started ###");

            // 1. Get session values
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");

            _logger.LogInformation($"Session Email: {userEmail}");
            _logger.LogInformation($"Session Role: {userRole}");

            // Check if user is not logged in or is not a teacher
            if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userRole))
            {
                _logger.LogInformation("FAIL: Session was null. Redirecting to SignIn.");
                TempData["Message"] = "Not logged in. Please sign in again.";
                TempData["MessageType"] = "error";
                return RedirectToAction("SignIn");
            }

            if (userRole != "Teacher") // This is the key check - we trust the session role
            {
                _logger.LogInformation("FAIL: User role in session was not 'Teacher'. Redirecting to Home.");
                TempData["Message"] = "Access denied. Teacher access required.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index", "Home");
            }

            ModelState.Remove("Code");
            ModelState.Remove("Teacher");

            if (ModelState.IsValid)
            {
                _logger.LogInformation("ModelState is Valid. Proceeding...");

                // 2. Find the teacher. ONLY filter by email now!
                // We already confirmed their role from the session.
                var teacher = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                _logger.LogInformation($"Teacher found: {teacher != null}");

                // 3. Check if teacher was found
                if (teacher == null)
                {
                    _logger.LogInformation("FAIL: Teacher not found in DB. Redirecting to SignIn.");
                    TempData["Message"] = "User not found.";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("SignIn");
                }

                // Generate a unique code
                var random = new Random();
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                var uniqueCode = new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
                _logger.LogInformation($"Generated Code: {uniqueCode}");

                // Check if code is unique
                while (await _context.Courses.AnyAsync(c => c.Code == uniqueCode))
                {
                    uniqueCode = new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
                    _logger.LogInformation($"Code existed. New Code: {uniqueCode}");
                }

                // Create the new course
                var course = new Course
                {
                    Name = model.Name,
                    Code = uniqueCode,
                    TeacherId = teacher.Id
                };
                _logger.LogInformation($"Course object created for: {course.Name}");

                try
                {
                    _context.Courses.Add(course);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("SUCCESS: Course saved to database!");

                    TempData["ToastMessage"] = $"Course created successfully! Enrollment code: <strong>{uniqueCode}</strong>";
                    TempData["ToastType"] = "success";
                    return RedirectToAction("MyCourses");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "DATABASE ERROR during course save");
                    TempData["ToastMessage"] = "An error occurred saving the course. Please try again.";
                    TempData["ToastType"] = "error";
                    return View(model);
                }
            }
            else
            {
                _logger.LogInformation("FAIL: ModelState was Invalid.");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogInformation($"Validation Error: {error.ErrorMessage}");
                }
                return View(model);
            }
        }

        // ---------------- VIEW MY COURSES ----------------
        public async Task<IActionResult> MyCourses(int page = 1)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userEmail) || userRole != "Teacher")
            {
                TempData["Message"] = "Access denied. Please log in as a teacher.";
                TempData["MessageType"] = "error";
                return RedirectToAction("SignIn");
            }

            var teacher = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (teacher == null)
            {
                TempData["Message"] = "User not found.";
                TempData["MessageType"] = "error";
                return RedirectToAction("SignIn");
            }

            int pageSize = 5; // show 5 courses per page

            var coursesQuery = _context.Courses
                .Where(c => c.TeacherId == teacher.Id && c.IsActive)
                .OrderByDescending(c => c.CreatedAt); // latest courses first

            var totalCourses = await coursesQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCourses / (double)pageSize);

            var courses = await coursesQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(courses);
        }


        // ---------------- END COURSE ----------------
        [HttpPost]
        public async Task<IActionResult> EndCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                TempData["ToastMessage"] = "Course not found.";
                TempData["ToastType"] = "error";
                return RedirectToAction("MyCourses");
            }

            course.IsActive = false;
            course.EndedAt = DateTime.UtcNow; // Record when the course ended

            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "Course has been ended successfully.";
            TempData["ToastType"] = "success";
            return RedirectToAction("MyCourses");
        }


        // ---------------- VIEW ENDED COURSES ----------------
        [HttpGet]
        public async Task<IActionResult> EndCourses(int page = 1)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userEmail) || userRole != "Teacher")
            {
                TempData["ToastMessage"] = "Access denied. Teacher access required.";
                TempData["ToastType"] = "error";
                return RedirectToAction("Index", "Home");
            }

            // Get the teacher
            var teacher = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (teacher == null)
            {
                TempData["ToastMessage"] = "User not found.";
                TempData["ToastType"] = "error";
                return RedirectToAction("SignIn");
            }

            int pageSize = 5; // show 5 courses per page

            // Get ended courses query
            var endedCoursesQuery = _context.Courses
                .Where(c => c.TeacherId == teacher.Id && !c.IsActive)
                .OrderBy(c => c.EndedAt); // earliest ended courses first

            var totalCourses = await endedCoursesQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCourses / (double)pageSize);

            var endedCourses = await endedCoursesQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(endedCourses);
        }



        // ---------------- ENROLL IN COURSE (GET) ----------------
        [HttpGet]
        public IActionResult EnrollInCourse()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Student")
            {
                TempData["ToastMessage"] = "Only students can enroll in courses.";
                TempData["ToastType"] = "error";
                return RedirectToAction("StudentPage");
            }
            return View();
        }

        // ---------------- ENROLL IN COURSE (POST) ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollInCourse(string enrollmentCode)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole != "Student")
            {
                TempData["ToastMessage"] = "Access denied. Student access required.";
                TempData["ToastType"] = "error";
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrEmpty(enrollmentCode))
            {
                TempData["ToastMessage"] = "Please enter an enrollment code.";
                TempData["ToastType"] = "error";
                return View();
            }

            try
            {
                // Get the student
                var student = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (student == null)
                {
                    TempData["ToastMessage"] = "Student not found.";
                    TempData["ToastType"] = "error";
                    return RedirectToAction("SignIn");
                }

                // Find the course by code
                var course = await _context.Courses
                    .Include(c => c.Teacher)
                    .FirstOrDefaultAsync(c => c.Code == enrollmentCode);

                if (course == null)
                {
                    TempData["ToastMessage"] = "Invalid enrollment code. Please check the code and try again.";
                    TempData["ToastType"] = "error";
                    return View();
                }

                // Check if already enrolled
                var existingEnrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.StudentId == student.Id && e.CourseId == course.Id);

                if (existingEnrollment != null)
                {
                    TempData["ToastMessage"] = "You are already enrolled in this course.";
                    TempData["ToastType"] = "warning";
                    return RedirectToAction("StudentPage");
                }

                // Create enrollment
                var enrollment = new Enrollment
                {
                    StudentId = student.Id,
                    CourseId = course.Id
                };

                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();

                TempData["ToastMessage"] = $"Successfully enrolled in <strong>{course.Name}</strong> taught by {course.Teacher.Name}!";
                TempData["ToastType"] = "success";
                return RedirectToAction("MyEnrollments");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during enrollment");
                TempData["ToastMessage"] = "An error occurred during enrollment. Please try again.";
                TempData["ToastType"] = "error";
                return View();
            }
        }

        // ---------------- VIEW MY ENROLLMENTS (STUDENT) ----------------
        [HttpGet]
        public async Task<IActionResult> MyEnrollments()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Student")
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
                return RedirectToAction("SignIn");
            }
            // Get enrollments with course, teacher, and grade details
            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == student.Id)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Teacher)
                .OrderByDescending(e => e.EnrolledAt)
                .Select(e => new StudentEnrollmentVM
                {
                    EnrollmentId = e.Id,
                    CourseId = e.Course.Id,
                    CourseName = e.Course.Name,
                    TeacherName = e.Course.Teacher.Name,
                    EnrolledAt = e.EnrolledAt,
                    Grade = e.Grade
                })
                .ToListAsync();
            return View(enrollments);
        }


        // ---------------- VIEW COURSE ENROLLMENTS ----------------
        public async Task<IActionResult> CourseEnrollments(int courseId)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userEmail) || userRole != "Teacher")
            {
                TempData["ToastMessage"] = "Access denied. Teacher access required.";
                TempData["ToastType"] = "error";
                return RedirectToAction("Index", "Home");
            }

            // Get the teacher
            var teacher = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (teacher == null)
            {
                TempData["ToastMessage"] = "Teacher not found.";
                TempData["ToastType"] = "error";
                return RedirectToAction("SignIn");
            }

            // Get the course and verify it belongs to this teacher
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId && c.TeacherId == teacher.Id);

            if (course == null)
            {
                TempData["ToastMessage"] = "Course not found or access denied.";
                TempData["ToastType"] = "error";
                return RedirectToAction("MyCourses");
            }

            // Get enrolled students with their details
            var enrolledStudents = await _context.Enrollments
                .Where(e => e.CourseId == courseId)
                .Include(e => e.Student)
                .OrderBy(e => e.Student.StudentId)
                .Select(e => new EnrolledStudent
                {
                    StudentId = e.Student.Id,
                    Name = e.Student.Name,
                    Email = e.Student.Email,
                    PhoneNumber = e.Student.PhoneNumber ?? "N/A",
                    StudentIdNumber = e.Student.StudentId ?? "N/A",
                    Section = e.Student.Section ?? "N/A",
                    Department = e.Student.Department ?? "N/A",
                    EnrolledAt = e.EnrolledAt,
                    Grade = e.Grade
                })
                .ToListAsync();

            var viewModel = new CourseEnrollmentsVM
            {
                CourseId = course.Id,
                CourseName = course.Name,
                CourseCode = course.Code,
                EnrolledStudents = enrolledStudents
            };

            return View(viewModel);
        }

        //-----------------------------START SESSION (GET)----------------
        [HttpGet]
        public IActionResult StartSession(int courseId)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userEmail) || userRole != "Teacher")
            {
                TempData["ToastMessage"] = "Access denied. Teacher access required.";
                TempData["ToastType"] = "error";
                return RedirectToAction("Index", "Home");
            }

            var course = _context.Courses.FirstOrDefault(c => c.Id == courseId && c.Teacher.Email == userEmail);
            if (course == null)
            {
                TempData["ToastMessage"] = "Course not found or access denied.";
                TempData["ToastType"] = "error";
                return RedirectToAction("MyCourses");
            }

            return View(new AttendanceSession { CourseId = courseId });
        }

        //-----------------------------START SESSION (POST)----------------
        // store StartTime as UTC and PlannedEndTime as UTC (if provided)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartSession(AttendanceSession model)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var teacher = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (teacher == null)
            {
                TempData["ToastMessage"] = "Teacher not found.";
                TempData["ToastType"] = "error";
                return RedirectToAction("SignIn");
            }

            // Basic validation (course exists and belongs to teacher)
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == model.CourseId && c.TeacherId == teacher.Id);
            if (course == null)
            {
                TempData["ToastMessage"] = "Course not found or access denied.";
                TempData["ToastType"] = "error";
                return RedirectToAction("MyCourses");
            }

            // If PlannedEndTime provided, convert to UTC safely
            DateTime? plannedUtc = null;
            if (model.PlannedEndTime.HasValue)
            {
                // Ensure we treat the incoming DateTime as local/unspecified -> convert to UTC
                var specified = DateTime.SpecifyKind(model.PlannedEndTime.Value, DateTimeKind.Unspecified);
                plannedUtc = specified.ToUniversalTime();
            }

            var session = new AttendanceSession
            {
                CourseId = model.CourseId,
                IsActive = true,
                StartTime = DateTime.UtcNow,
                PlannedEndTime = plannedUtc
            };

            _context.AttendanceSessions.Add(session);
            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "Session started successfully!";
            TempData["ToastType"] = "success";
            return RedirectToAction("CourseSessions", new { courseId = session.CourseId });
        }

        //-----------------------------UPDATE END TIME----------------
        // Validate required value, ensure new end time is in the future, and save as UTC
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEndTime(int sessionId, DateTime? newEndTime)
        {
            var session = await _context.AttendanceSessions.FindAsync(sessionId);
            if (session == null)
            {
                TempData["ToastMessage"] = "Session not found.";
                TempData["ToastType"] = "error";
                return RedirectToAction("MyCourses");
            }

            if (!session.IsActive)
            {
                TempData["ToastMessage"] = "Session already ended.";
                TempData["ToastType"] = "error";
                return RedirectToAction("SessionDetails", new { sessionId });
            }

            if (newEndTime == null)
            {
                TempData["ToastMessage"] = "Please select a valid end time.";
                TempData["ToastType"] = "error";
                return RedirectToAction("SessionDetails", new { sessionId });
            }

            // Convert to UTC and ensure it's a future time relative to server UTC
            var newUtc = DateTime.SpecifyKind(newEndTime.Value, DateTimeKind.Unspecified).ToUniversalTime();
            if (newUtc <= DateTime.UtcNow)
            {
                TempData["ToastMessage"] = "End time must be in the future.";
                TempData["ToastType"] = "error";
                return RedirectToAction("SessionDetails", new { sessionId });
            }

            session.PlannedEndTime = newUtc;
            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "End time updated successfully!";
            TempData["ToastType"] = "success";
            return RedirectToAction("SessionDetails", new { sessionId });
        }




        //-----------------------------END SESSION (POST) (existing)----------------
        // Keep your existing EndSession if you want; it's redirect-based.
        // We will add an AJAX-friendly EndSessionAjax below.

        [HttpPost]
        public async Task<IActionResult> EndSession(int sessionId)
        {
            var session = await _context.AttendanceSessions.FindAsync(sessionId);
            if (session == null || !session.IsActive)
            {
                TempData["ToastMessage"] = "Session not found or already ended.";
                TempData["ToastType"] = "error";
                return RedirectToAction("MyCourses");
            }

            session.EndTime = DateTime.UtcNow;
            session.IsActive = false;
            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "Session ended successfully!";
            TempData["ToastType"] = "success";
            return RedirectToAction("SessionDetails", new { sessionId });
        }

        //-----------------------------END SESSION AJAX----------------
        // Called by JS when countdown reaches zero or when teacher clicks "End" via AJAX.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EndSessionAjax(int sessionId)
        {
            var session = await _context.AttendanceSessions.FindAsync(sessionId);
            if (session == null)
            {
                return Json(new { success = false, message = "Session not found." });
            }

            if (!session.IsActive)
            {
                return Json(new { success = true, alreadyEnded = true });
            }

            session.IsActive = false;
            session.EndTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        //-----------------------------SESSION STATUS (GET)----------------
        // Returns the minimal session state for polling (JSON)
        [HttpGet]
        public async Task<IActionResult> SessionStatus(int sessionId)
        {
            var session = await _context.AttendanceSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
                return NotFound();

            return Json(new
            {
                isActive = session.IsActive,
                plannedEndTime = session.PlannedEndTime.HasValue ? session.PlannedEndTime.Value.ToString("o") : null,
                endTime = session.EndTime.HasValue ? session.EndTime.Value.ToString("o") : null
            });
        }

        // ----------------------------- AUTO END EXPIRED ----------------
        private async Task AutoEndExpiredSessions()
        {
            var now = DateTime.UtcNow;
            var expiredSessions = await _context.AttendanceSessions
                .Where(s => s.IsActive && s.PlannedEndTime.HasValue && s.PlannedEndTime <= now)
                .ToListAsync();

            foreach (var s in expiredSessions)
            {
                s.IsActive = false;
                s.EndTime = now;
            }

            if (expiredSessions.Any())
                await _context.SaveChangesAsync();
        }


        // ---------------- ACTIVE SESSIONS ----------------
        [HttpGet]
        public async Task<IActionResult> ActiveSessions()
        {
            await AutoEndExpiredSessions(); // ✅ auto close expired sessions

            var userEmail = HttpContext.Session.GetString("UserEmail");
            var student = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (student == null) return RedirectToAction("SignIn");

            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == student.Id)
                .Select(e => e.CourseId)
                .ToListAsync();

            var activeSessions = await _context.AttendanceSessions
                .Where(s => enrollments.Contains(s.CourseId) && s.IsActive)
                .Include(s => s.Course)
                .ToListAsync();

            return View(activeSessions);
        }


        //Mark Attendance

        [HttpPost]
        public async Task<IActionResult> MarkAttendance(int sessionId)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var student = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (student == null)
            {
                TempData["ToastMessage"] = "Student not found.";
                TempData["ToastType"] = "error";
                return RedirectToAction("SignIn");
            }

            var session = await _context.AttendanceSessions.FindAsync(sessionId);
            if (session == null || !session.IsActive)
            {
                TempData["ToastMessage"] = "Session not found or inactive.";
                TempData["ToastType"] = "error";
                return RedirectToAction("ActiveSessions");
            }

            var existingRecord = await _context.AttendanceRecords
                .FirstOrDefaultAsync(r => r.SessionId == sessionId && r.StudentId == student.Id);

            if (existingRecord != null)
            {
                TempData["ToastMessage"] = "You have already marked your attendance for this session.";
                TempData["ToastType"] = "warning";
                return RedirectToAction("ActiveSessions");
            }

            var record = new AttendanceRecord
            {
                SessionId = sessionId,
                StudentId = student.Id
            };

            _context.AttendanceRecords.Add(record);
            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "Attendance marked successfully!";
            TempData["ToastType"] = "success";
            return RedirectToAction("ActiveSessions");
        }


        //Pdf of Attendance Record

        [HttpGet]
        public async Task<IActionResult> GeneratePDF(int sessionId)
        {
            var session = await _context.AttendanceSessions
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
            {
                TempData["ToastMessage"] = "Session not found.";
                TempData["ToastType"] = "error";
                return RedirectToAction("MyCourses");
            }

            var records = await _context.AttendanceRecords
                .Where(r => r.SessionId == sessionId)
                .Include(r => r.Student)
                .OrderBy(r => r.MarkedAt)
                .ToListAsync();

            var document = new AttendancePDFDocument(session, records);
            var pdfBytes = document.Generate();

            return File(pdfBytes, "application/pdf", $"Attendance_{session.Course.Name}_{session.StartTime:yyyyMMdd}.pdf");
        }


        // ---------------- COURSE SESSIONS ----------------
        [HttpGet]
        public async Task<IActionResult> CourseSessions(int courseId)
        {
            await AutoEndExpiredSessions(); // ✅ auto close expired sessions

            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userEmail) || userRole != "Teacher")
                return RedirectToAction("Index", "Home");

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId && c.Teacher.Email == userEmail);
            if (course == null) return RedirectToAction("MyCourses");

            var sessions = await _context.AttendanceSessions
                .Where(s => s.CourseId == courseId)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();

            var viewModel = new CourseSessionsVM
            {
                Course = course,
                Sessions = sessions
            };

            return View(viewModel);
        }


        // ---------------- SESSION DETAILS ----------------
        [HttpGet]
        public async Task<IActionResult> SessionDetails(int sessionId)
        {
            await AutoEndExpiredSessions(); // ✅ auto close expired sessions

            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userEmail) || userRole != "Teacher")
                return RedirectToAction("Index", "Home");

            var session = await _context.AttendanceSessions
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.Course.Teacher.Email == userEmail);
            if (session == null) return RedirectToAction("MyCourses");

            var records = await _context.AttendanceRecords
                .Where(r => r.SessionId == sessionId)
                .Include(r => r.Student)
                .OrderBy(r => r.MarkedAt)
                .ToListAsync();

            var viewModel = new SessionDetailsVM
            {
                Session = session,
                Records = records
            };

            return View(viewModel);
        }

        //Assign Grade
        [HttpPost]
        public async Task<IActionResult> AssignGrade(int courseId, int studentId, float grade)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userEmail) || userRole != "Teacher")
            {
                TempData["ToastMessage"] = "Access denied. Teacher access required.";
                TempData["ToastType"] = "error";
                return RedirectToAction("Index", "Home");
            }

            var teacher = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (teacher == null)
            {
                TempData["ToastMessage"] = "Teacher not found.";
                TempData["ToastType"] = "error";
                return RedirectToAction("SignIn");
            }

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId && c.TeacherId == teacher.Id);
            if (course == null)
            {
                TempData["ToastMessage"] = "Course not found or access denied.";
                TempData["ToastType"] = "error";
                return RedirectToAction("MyCourses");
            }

            var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId);
            if (enrollment == null)
            {
                TempData["ToastMessage"] = "Enrollment not found.";
                TempData["ToastType"] = "error";
                return RedirectToAction("CourseEnrollments", new { courseId });
            }

            enrollment.Grade = grade;
            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "Grade assigned successfully!";
            TempData["ToastType"] = "success";
            return RedirectToAction("CourseEnrollments", new { courseId });
        }

        //-------------------UNENROLL------------------------

        [HttpPost]
        public async Task<IActionResult> UnenrollStudent(int courseId, int studentId)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");

            // Only teachers can unenroll
            if (string.IsNullOrEmpty(userEmail) || userRole != "Teacher")
            {
                TempData["ToastMessage"] = "Access denied. Teacher access required.";
                TempData["ToastType"] = "error";
                return RedirectToAction("Index", "Home");
            }

            var teacher = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (teacher == null)
            {
                TempData["ToastMessage"] = "Teacher not found.";
                TempData["ToastType"] = "error";
                return RedirectToAction("SignIn");
            }

            // Check if the course belongs to this teacher
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId && c.TeacherId == teacher.Id);

            if (course == null)
            {
                TempData["ToastMessage"] = "Course not found or access denied.";
                TempData["ToastType"] = "error";
                return RedirectToAction("MyCourses");
            }

            // Find enrollment
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId);

            if (enrollment == null)
            {
                TempData["ToastMessage"] = "Enrollment not found.";
                TempData["ToastType"] = "error";
                return RedirectToAction("CourseEnrollments", new { courseId });
            }

            // Remove enrollment
            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "Student unenrolled successfully.";
            TempData["ToastType"] = "success";

            return RedirectToAction("CourseEnrollments", new { courseId });
        }


       
        

        // ---------------- MANAGE USERS ----------------
        public IActionResult ManageUsers()
        {
            var users = _context.Users.ToList(); // Fetch all users
            return View(users); // Pass the user list to the view
        }


        // ---------------- EDIT USER (GET) ----------------
        [HttpGet]
        public IActionResult EditUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // ---------------- EDIT USER (POST) ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(User model)
        {
            if (ModelState.IsValid)
            {
                _context.Users.Update(model);
                _context.SaveChanges();
                TempData["Message"] = "User updated successfully!";
                return RedirectToAction("ManageUsers");
            }
            return View(model);
        }

        // ---------------- DELETE USER ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            _context.SaveChanges();

            TempData["Message"] = "User deleted successfully!";
            return RedirectToAction("ManageUsers");
        }








    }
}