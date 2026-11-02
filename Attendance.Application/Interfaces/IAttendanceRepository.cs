using Attendance.domain.Entities;

namespace Attendance.Application.Interfaces;

public interface IAttendanceRepository
{
    Task<List<AttendanceRecord>> GetAllAttendanceAsync();
    Task<AttendanceRecord?> GetAttendanceByIdAsync(int id);
    Task<AttendanceRecord?> GetOpenAttendanceForEmployeeAsync(int employeeId);
    Task<int> CreateAttendanceAsync(AttendanceRecord attendance);
    Task<bool> UpdateAttendanceAsync(int id, AttendanceRecord attendance);
    Task<bool> SetPunchOutAsync(int attendanceId, DateTime punchOutTime, string? notes, string status); Task<bool> DeleteAttendanceAsync(int id);
    Task<bool> AttendanceExistsAsync(int employeeId, DateTime attendanceDate);
}