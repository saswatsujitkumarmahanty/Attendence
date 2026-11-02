using Attendance.domain.Entities;
using Attendance.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Attendence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection was not found.");
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        var users = new List<User>();

        const string query = @"
        SELECT UserId, Username, PasswordHash, Role, IsActive, EmployeeId
        FROM Users
        ORDER BY UserId;";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(query, connection);

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            int employeeIdOrdinal = reader.GetOrdinal("EmployeeId");
            users.Add(new User
            {
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                Role = reader.GetString(reader.GetOrdinal("Role")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                EmployeeId = await reader.IsDBNullAsync(employeeIdOrdinal) ? null : reader.GetInt32(employeeIdOrdinal)
            });
        }

        return users;
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        const string query = @"
            SELECT UserId, Username, PasswordHash, Role, IsActive, EmployeeId
            FROM Users
            WHERE Username = @Username;";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(query, connection);

        command.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar, 100).Value = username;

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            int employeeIdOrdinal = reader.GetOrdinal("EmployeeId");
            return new User
            {
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                Role = reader.GetString(reader.GetOrdinal("Role")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                EmployeeId = await reader.IsDBNullAsync(employeeIdOrdinal) ? null : reader.GetInt32(employeeIdOrdinal)
            };
        }

        return null;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        const string query = @"
            SELECT UserId, Username, PasswordHash, Role, IsActive, EmployeeId
            FROM Users
            WHERE UserId = @UserId;";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(query, connection);

        command.Parameters.Add("@UserId", System.Data.SqlDbType.Int).Value = id;

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            int employeeIdOrdinal = reader.GetOrdinal("EmployeeId");
            return new User
            {
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                Role = reader.GetString(reader.GetOrdinal("Role")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                EmployeeId = await reader.IsDBNullAsync(employeeIdOrdinal) ? null : reader.GetInt32(employeeIdOrdinal)
            };
        }

        return null;
    }

    public async Task<int> CreateUserAsync(User user)
    {
        const string query = @"
            INSERT INTO Users (Username, PasswordHash, Role, IsActive, EmployeeId)
            OUTPUT INSERTED.UserId
            VALUES (@Username, @PasswordHash, @Role, @IsActive, @EmployeeId);";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(query, connection);

        command.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar, 100).Value = user.Username;
        command.Parameters.Add("@PasswordHash", System.Data.SqlDbType.NVarChar, 500).Value = user.PasswordHash;
        command.Parameters.Add("@Role", System.Data.SqlDbType.NVarChar, 50).Value = user.Role;
        command.Parameters.Add("@IsActive", System.Data.SqlDbType.Bit).Value = user.IsActive;
        command.Parameters.Add("@EmployeeId", System.Data.SqlDbType.Int).Value = (object?)user.EmployeeId ?? DBNull.Value;

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }
}