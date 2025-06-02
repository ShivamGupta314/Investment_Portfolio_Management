using InvestmentPortfolioManagement.Helpers;
using InvestmentPortfolioManagement.Interfaces;
using InvestmentPortfolioManagement.Models;
using InvestmentPortfolioManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace InvestmentPortfolioManagement.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public UserController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        // GET: /User/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /User/Login
        [HttpPost]
        public async Task<IActionResult> Login(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields.";
                return RedirectToAction("Login");
            }

            var user = await _userService.AuthenticateAsync(model.Username, model.Password);
            if (user != null)
            {
                var token = JwtHelper.GenerateJwtToken(user, _configuration);
                Response.Cookies.Append("jwt", token, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddHours(2)
                });
                return RedirectToAction("Index", "Portfolio");
            }

            TempData["Error"] = "Invalid username or password.";
            return RedirectToAction("Login");
        }

        // GET: /User/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /User/Register
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid registration details.";
                return View(user);
            }

            await _userService.RegisterAsync(user);
            TempData["Success"] = "Registration successful. Please login.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt"); // Delete JWT token
            return RedirectToAction("Login", "User");
        }

    }
}
