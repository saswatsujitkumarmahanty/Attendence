namespace Attendance.domain.Entities;

public class AttendanceRecord
{
    public int AttendanceId { get; set; }
    public int EmployeeId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? PunchInTime { get; set; }
    public DateTime? PunchOutTime { get; set; }
    public string? Notes { get; set; }
    public int MarkedBy { get; set; }

    public Employee? Employee { get; set; }
}