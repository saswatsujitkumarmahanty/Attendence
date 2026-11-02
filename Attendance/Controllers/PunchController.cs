using System.Security.Claims;
using Attendance.Application.DTOs;
using Attendance.Application.Interfaces;
using Attendance.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Attendance.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PunchController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        private readonly IAttendanceRepository _attendanceRepository;

        public PunchController(
            IAttendanceService attendanceService,
            IAttendanceRepository attendanceRepository)
        {
            _attendanceService = attendanceService;
            _attendanceRepository = attendanceRepository;
        }

        // POST: api/Punch/in
        [HttpPost("in")]
        public async Task<IActionResult> PunchIn([FromBody] PunchIn request)
        {
            int currentUserId = GetCurrentUserId();
            int employeeId = ResolveEmployeeId(request.EmployeeId);

            var (success, error, record) = await _attendanceService.PunchInAsync(
                employeeId, currentUserId, request.Notes);

            if (!success)
            {
                return Conflict(new { message = error });
            }

            return Ok(record);
        }

        // POST: api/Punch/out
        [HttpPost("out")]
        public async Task<IActionResult> PunchOut([FromBody] PunchOut request)
        {
            int employeeId = ResolveEmployeeId(request.EmployeeId);

            var (success, error, record) = await _attendanceService.PunchOutAsync(
                employeeId, request.Notes);

            if (!success)
            {
                return BadRequest(new { message = error });
            }

            return Ok(record);
        }

        // GET: api/Punch/status/5
        [HttpGet("status/{employeeId:int}")]
        public async Task<IActionResult> GetStatus(int employeeId)
        {
            int resolvedEmployeeId = ResolveEmployeeId(employeeId);

            var openRecord = await _attendanceRepository.GetOpenAttendanceForEmployeeAsync(resolvedEmployeeId);

            return Ok(new
            {
                isPunchedIn = openRecord != null,
                record = openRecord
            });
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var userId) ? userId : 0;
        }

        // Employees can only punch themselves in/out (their EmployeeId comes from the JWT).
        // Admins/Managers may pass an EmployeeId in the request body to act on someone else's behalf.
        private int ResolveEmployeeId(int requestedEmployeeId)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (string.Equals(role, "Employee", StringComparison.OrdinalIgnoreCase))
            {
                var employeeClaim = User.FindFirstValue("EmployeeId");
                if (int.TryParse(employeeClaim, out var employeeId))
                {
                    return employeeId;
                }
            }

            return requestedEmployeeId;
        }
    }
}