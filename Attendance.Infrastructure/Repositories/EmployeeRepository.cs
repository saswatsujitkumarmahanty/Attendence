using Microsoft.Data.SqlClient;
using Attendance.domain.Entities;
using Microsoft.Extensions.Configuration;

namespace Attendence.Repositories
{
    public class EmployeeRepository
    {
        private readonly string _connectionString;

        public EmployeeRepository(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "DefaultConnection was not found.");
        }

        // GET ALL EMPLOYEES
        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            var employees = new List<Employee>();

            const string query = @"
                SELECT EmployeeId, EmployeeCode, EmployeeName, DepartmentId
                FROM Employees
                ORDER BY EmployeeCode;";

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(query, connection);

            await connection.OpenAsync();

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                employees.Add(new Employee
                {
                    EmployeeId = reader.GetInt32(
                        reader.GetOrdinal("EmployeeId")),

                    EmployeeCode = reader.GetString(
                        reader.GetOrdinal("EmployeeCode")),

                    EmployeeName = reader.GetString(
                        reader.GetOrdinal("EmployeeName")),

                    DepartmentId = reader.GetInt32(
                        reader.GetOrdinal("DepartmentId"))
                });
            }

            return employees;
        }

        // GET EMPLOYEE BY ID
        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            const string query = @"
                SELECT EmployeeId, EmployeeCode, EmployeeName, DepartmentId
                FROM Employees
                WHERE EmployeeId = @EmployeeId;";

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@EmployeeId", System.Data.SqlDbType.Int)
                .Value = id;

            await connection.OpenAsync();

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Employee
                {
                    EmployeeId = reader.GetInt32(
                        reader.GetOrdinal("EmployeeId")),

                    EmployeeCode = reader.GetString(
                        reader.GetOrdinal("EmployeeCode")),

                    EmployeeName = reader.GetString(
                        reader.GetOrdinal("EmployeeName")),

                    DepartmentId = reader.GetInt32(
                        reader.GetOrdinal("DepartmentId"))
                };
            }

            return null;
        }

        // CREATE EMPLOYEE
        public async Task<int> CreateEmployeeAsync(Employee employee)
        {
            const string query = @"
                INSERT INTO Employees
                    (EmployeeCode, EmployeeName, DepartmentId)
                OUTPUT INSERTED.EmployeeId
                VALUES
                    (@EmployeeCode, @EmployeeName, @DepartmentId);";

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@EmployeeCode", System.Data.SqlDbType.NVarChar, 50)
                .Value = employee.EmployeeCode;

            command.Parameters.Add("@EmployeeName", System.Data.SqlDbType.NVarChar, 100)
                .Value = employee.EmployeeName;

            command.Parameters.Add("@DepartmentId", System.Data.SqlDbType.Int)
                .Value = employee.DepartmentId;

            await connection.OpenAsync();

            var result = await command.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }

        // UPDATE EMPLOYEE
        public async Task<bool> UpdateEmployeeAsync(
            int id,
            Employee employee)
        {
            const string query = @"
                UPDATE Employees
                SET
                    EmployeeCode = @EmployeeCode,
                    EmployeeName = @EmployeeName,
                    DepartmentId = @DepartmentId
                WHERE EmployeeId = @EmployeeId;";

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@EmployeeId", System.Data.SqlDbType.Int)
                .Value = id;

            command.Parameters.Add("@EmployeeCode", System.Data.SqlDbType.NVarChar, 50)
                .Value = employee.EmployeeCode;

            command.Parameters.Add("@EmployeeName", System.Data.SqlDbType.NVarChar, 100)
                .Value = employee.EmployeeName;

            command.Parameters.Add("@DepartmentId", System.Data.SqlDbType.Int)
                .Value = employee.DepartmentId;

            await connection.OpenAsync();

            int rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }

        // DELETE EMPLOYEE
        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            const string query = @"
                DELETE FROM Employees
                WHERE EmployeeId = @EmployeeId;";

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@EmployeeId", System.Data.SqlDbType.Int)
                .Value = id;

            await connection.OpenAsync();

            int rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }
    }
}