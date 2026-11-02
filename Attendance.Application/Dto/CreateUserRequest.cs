namespace Attendance.Application.Dto
{
    public class CreateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? EmployeeId { get; set; }
    }
}