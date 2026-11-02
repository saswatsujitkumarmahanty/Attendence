using Attendance.Application.Interfaces;
using Attendance.domain.Entities;

namespace Attendance.Application.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _attendanceRepository;

    public AttendanceService(IAttendanceRepository attendanceRepository)
    {
        _attendanceRepository = attendanceRepository;
    }

    public async Task<(bool Success, string? ErrorMessage, AttendanceRecord? Record)> PunchInAsync(
        int employeeId, int currentUserId, string? notes)
    {
        var existingOpenRecord = await _attendanceRepository.GetOpenAttendanceForEmployeeAsync(employeeId);
        if (existingOpenRecord != null)
        {
            return (false, "You are currently punched in. Please punch out before punching in again.", null);
        }

        var now = DateTime.UtcNow;
        var attendance = new AttendanceRecord
        {
            EmployeeId = employeeId,
            AttendanceDate = now.Date,
            Status = "Present",
            PunchInTime = now,
            PunchOutTime = null,
            Notes = notes,
            MarkedBy = currentUserId
        };

        int attendanceId = await _attendanceRepository.CreateAttendanceAsync(attendance);
        attendance.AttendanceId = attendanceId;

        return (true, null, attendance);
    }

    public async Task<(bool Success, string? ErrorMessage, AttendanceRecord? Record)> PunchOutAsync(
        int employeeId, string? notes)
    {
        var openRecord = await _attendanceRepository.GetOpenAttendanceForEmployeeAsync(employeeId);
        if (openRecord == null)
        {
            return (false, "You do not have an active punch-in record to punch out of.", null);
        }

        var now = DateTime.UtcNow;
        var combinedNotes = string.IsNullOrWhiteSpace(notes)
            ? openRecord.Notes
            : string.IsNullOrWhiteSpace(openRecord.Notes)
                ? notes
                : $"{openRecord.Notes} | {notes}";

        double hoursWorked = (now - openRecord.PunchInTime!.Value).TotalHours;
        string status = CalculateStatus(hoursWorked);

        bool updated = await _attendanceRepository.SetPunchOutAsync(openRecord.AttendanceId, now, combinedNotes, status);
        if (!updated)
        {
            return (false, "Attendance record could not be updated.", null);
        }

        openRecord.PunchOutTime = now;
        openRecord.Notes = combinedNotes;
        openRecord.Status = status;

        return (true, null, openRecord);
    }

    private static string CalculateStatus(double hoursWorked)
    {
        if (hoursWorked >= 8)
        {
            return "Full Day";
        }

        if (hoursWorked >= 5)
        {
            return "Half Day";
        }

        return "Absent";
    }
}


