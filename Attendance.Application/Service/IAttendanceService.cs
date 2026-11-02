using Attendance.domain.Entities;

namespace Attendance.Application.Services;

public interface IAttendanceService
{
    Task<(bool Success, string? ErrorMessage, AttendanceRecord? Record)> PunchInAsync(int employeeId, int currentUserId, string? notes);
    Task<(bool Success, string? ErrorMessage, AttendanceRecord? Record)> PunchOutAsync(int employeeId, string? notes);
}