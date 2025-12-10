using Microsoft.AspNetCore.Mvc;
using EventManagementSystem.Models;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace EventManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email and password are required.");
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null && VerifyPassword(password, user.PasswordHash))
            {
                // Check if user account is active
                if (!user.IsActive)
                {
                    ModelState.AddModelError("", "Your account has been deactivated.");
                    return View();
                }

                // Set session
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Username", user.Username ?? "");
                HttpContext.Session.SetString("IsAdmin", user.IsAdmin ? "true" : "false");
                return RedirectToAction("Index", "Event");
            }

            ModelState.AddModelError("", "Invalid email or password.");
            return View();
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword, string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || 
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(firstName) || 
                string.IsNullOrWhiteSpace(lastName))
            {
                ModelState.AddModelError("", "All fields are required.");
                return View();
            }

            // Validate password
            if (password.Length < 8)
            {
                ModelState.AddModelError("", "Password must be at least 8 characters long.");
                return View();
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View();
            }

            // Validate email format
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address != email)
                {
                    ModelState.AddModelError("", "Invalid email format.");
                    return View();
                }
            }
            catch
            {
                ModelState.AddModelError("", "Invalid email format.");
                return View();
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "Email already exists.");
                return View();
            }

            var existingUsername = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (existingUsername != null)
            {
                ModelState.AddModelError("", "Username already exists.");
                return View();
            }

            var user = new User
            {
                Username = username,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PasswordHash = HashPassword(password),
                IsAdmin = false // New users are organizers by default, not admins
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username ?? "");
            return RedirectToAction("Index", "Event");
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Event");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }
    }
}
