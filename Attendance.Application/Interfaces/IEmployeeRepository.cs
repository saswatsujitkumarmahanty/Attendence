using Attendance.domain.Entities;

namespace Attendance.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<List<Employee>> GetAllEmployeesAsync();
    Task<Employee?> GetEmployeeByIdAsync(int id);
    Task<int> CreateEmployeeAsync(Employee employee);
    Task<bool> UpdateEmployeeAsync(int id, Employee employee);
    Task<bool> DeleteEmployeeAsync(int id);
}