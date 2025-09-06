using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RollCall.Models;
using Microsoft.AspNetCore.Http;

namespace RollCall.Controllers
{
    public class UserController : Controller
    {
        private readonly RollCallDbContext _context;

        public UserController(RollCallDbContext context)
        {
            _context = context;
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

    }
}