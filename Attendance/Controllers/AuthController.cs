using Attendance.Application.Dto;
using Attendance.domain.Entities;
using Attendence.Helper;
using Attendence.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Attendance.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly EmployeeRepository _employeeRepository;
        private readonly JWTHelper _jwtHelper;
        private readonly PasswordHasher<User> _passwordHasher;

        private static readonly string[] AllowedRoles = { "Admin", "Manager", "Employee" };

        public AuthController(
            UserRepository userRepository,
            EmployeeRepository employeeRepository,
            JWTHelper jwtHelper)
        {
            _userRepository = userRepository;
            _employeeRepository = employeeRepository;
            _jwtHelper = jwtHelper;
            _passwordHasher = new PasswordHasher<User>();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username and password are required." });
            }

            var user = await _userRepository.GetUserByUsernameAsync(request.Username);

            if (user == null || !user.IsActive)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            var passwordResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

            if (passwordResult == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            var token = _jwtHelper.GenerateJwtToken(user);

            return Ok(new
            {
                message = "Login successful.",
                token = token,
                userId = user.UserId,
                username = user.Username,
                role = user.Role
            });
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userRepository.GetAllUsersAsync();
            var result = users.Select(u => new {
                userId = u.UserId,
                username = u.Username,
                role = u.Role,
                isActive = u.IsActive,
                employeeId = u.EmployeeId
            });
            return Ok(result);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] CreateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username and password are required." });
            }

            if (!AllowedRoles.Contains(request.Role))
            {
                return BadRequest(new { message = $"Role must be one of: {string.Join(", ", AllowedRoles)}." });
            }

            var existingUser = await _userRepository.GetUserByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                return Conflict(new { message = "That username is already taken." });
            }

            if (request.EmployeeId.HasValue)
            {
                var employee = await _employeeRepository.GetEmployeeByIdAsync(request.EmployeeId.Value);
                if (employee == null)
                {
                    return BadRequest(new { message = "No employee exists with that EmployeeId." });
                }
            }

            var newUser = new User
            {
                Username = request.Username,
                Role = request.Role,
                IsActive = true,
                EmployeeId = request.EmployeeId
            };

            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, request.Password);
            int userId = await _userRepository.CreateUserAsync(newUser);

            return StatusCode(201, new
            {
                message = "User created.",
                userId,
                username = newUser.Username,
                role = newUser.Role,
                employeeId = newUser.EmployeeId
            });
        }
    }
}