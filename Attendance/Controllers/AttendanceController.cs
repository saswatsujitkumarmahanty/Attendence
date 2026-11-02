using Attendance.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Attendance.domain.Entities;
using Attendance.Infrastructure.Repositories;

namespace Attendance.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Manager")]
    public class AttendanceController : ControllerBase
    {
        private readonly AttendanceRepository _attendanceRepository;

        public AttendanceController(
            AttendanceRepository attendanceRepository)
        {
            _attendanceRepository = attendanceRepository;
        }

        // GET: api/Attendance
        [HttpGet]
        public async Task<ActionResult<List<AttendanceRecord>>> GetAttendance()
        {
            var attendance =
                await _attendanceRepository.GetAllAttendanceAsync();

            return Ok(attendance);
        }

        // GET: api/Attendance/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AttendanceRecord>> GetAttendance(int id)
        {
            var attendance =
                await _attendanceRepository.GetAttendanceByIdAsync(id);

            if (attendance == null)
            {
                return NotFound(new
                {
                    message = "Attendance record not found."
                });
            }

            return Ok(attendance);
        }

        // POST: api/Attendance
        [HttpPost]
        public async Task<ActionResult<AttendanceRecord>> CreateAttendance(
            [FromBody] AttendanceRecord attendance)
        {
            if (attendance == null)
            {
                return BadRequest(new
                {
                    message = "Attendance data is required."
                });
            }

            bool alreadyExists =
    await _attendanceRepository.AttendanceExistsAsync(
        attendance.EmployeeId,
        attendance.AttendanceDate);

            if (alreadyExists)
            {
                return Conflict(new
                {
                    message = "Attendance has already been marked for this student on this date."
                });
            }

            int attendanceId =
                await _attendanceRepository.CreateAttendanceAsync(attendance);

            attendance.AttendanceId = attendanceId;

            return CreatedAtAction(
                nameof(GetAttendance),
                new { id = attendanceId },
                attendance);
        }

        // PUT: api/Attendance/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAttendance(
            int id,
            [FromBody] AttendanceRecord attendance)
        {
            if (attendance == null)
            {
                return BadRequest(new
                {
                    message = "Attendance data is required."
                });
            }

            var existingAttendance =
                await _attendanceRepository.GetAttendanceByIdAsync(id);

            if (existingAttendance == null)
            {
                return NotFound(new
                {
                    message = "Attendance record not found."
                });
            }

            bool updated =
                await _attendanceRepository.UpdateAttendanceAsync(
                    id,
                    attendance);

            if (!updated)
            {
                return NotFound(new
                {
                    message = "Attendance could not be updated."
                });
            }

            return NoContent();
        }

        // DELETE: api/Attendance/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAttendance(int id)
        {
            var existingAttendance =
                await _attendanceRepository.GetAttendanceByIdAsync(id);

            if (existingAttendance == null)
            {
                return NotFound(new
                {
                    message = "Attendance record not found."
                });
            }

            bool deleted =
                await _attendanceRepository.DeleteAttendanceAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Attendance could not be deleted."
                });
            }

            return NoContent();
        }
    }
}