using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RollCall.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                TempData["Message"] = "Account not created!";
                TempData["MessageType"] = "error";
                return View();
            }

            if (user.PasswordHash != password) // replace with hashing in production
            {
                TempData["Message"] = "Wrong password!";
                TempData["MessageType"] = "error";
                return View();
            }

            // Save session
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Role);

            //TempData["Message"] = "Signed in successfully!";
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
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail && u.Role == "Admin");
            if (user == null) return RedirectToAction("SignIn");
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
                    return RedirectToAction("TeacherPage");
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
        public async Task<IActionResult> MyCourses()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userEmail) || userRole != "Teacher")
            {
                TempData["Message"] = "Access denied. Please log in as a teacher.";
                TempData["MessageType"] = "error";
                return RedirectToAction("SignIn");
            }

            // Get the teacher
            var teacher = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (teacher == null)
            {
                TempData["Message"] = "User not found.";
                TempData["MessageType"] = "error";
                return RedirectToAction("SignIn");
            }

            // Get all courses created by this teacher, ordered by most recent
            var courses = await _context.Courses
                .Where(c => c.TeacherId == teacher.Id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(courses);
        }

    }
}