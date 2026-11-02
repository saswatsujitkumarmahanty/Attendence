using Attendance.domain.Entities;

namespace Attendance.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByIdAsync(int id);
    Task<int> CreateUserAsync(User user);
    Task<List<User>> GetAllUsersAsync();
}