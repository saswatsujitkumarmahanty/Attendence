using Attendence.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Attendance.domain.Entities;

namespace Attendence.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class EmployeesController : ControllerBase
    {
        private readonly EmployeeRepository _employeeRepository;

        public EmployeesController(EmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        // GET: api/employees
        [HttpGet]
        public async Task<ActionResult<List<Employee>>> GetEmployees()
        {
            var employees = await _employeeRepository.GetAllEmployeesAsync();

            return Ok(employees);
        }

        // GET: api/employees/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(id);

            if (employee == null)
            {
                return NotFound(new
                {
                    message = "Employee not found."
                });
            }

            return Ok(employee);
        }

        // POST: api/employees
        [HttpPost]
        public async Task<ActionResult<Employee>> CreateEmployee(
            [FromBody] Employee employee)
        {
            if (employee == null)
            {
                return BadRequest(new
                {
                    message = "Employee data is required."
                });
            }

            int employeeId =
                await _employeeRepository.CreateEmployeeAsync(employee);

            employee.EmployeeId = employeeId;

            return CreatedAtAction(
                nameof(GetEmployee),
                new { id = employeeId },
                employee);
        }

        // PUT: api/employees/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEmployee(
            int id,
            [FromBody] Employee employee)
        {
            if (employee == null)
            {
                return BadRequest(new
                {
                    message = "Employee data is required."
                });
            }

            var existingEmployee =
                await _employeeRepository.GetEmployeeByIdAsync(id);

            if (existingEmployee == null)
            {
                return NotFound(new
                {
                    message = "Employee not found."
                });
            }

            bool updated =
                await _employeeRepository.UpdateEmployeeAsync(id, employee);

            if (!updated)
            {
                return NotFound(new
                {
                    message = "Employee could not be updated."
                });
            }

            return NoContent();
        }

        // DELETE: api/employees/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var existingEmployee =
                await _employeeRepository.GetEmployeeByIdAsync(id);

            if (existingEmployee == null)
            {
                return NotFound(new
                {
                    message = "Employee not found."
                });
            }

            bool deleted =
                await _employeeRepository.DeleteEmployeeAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Employee could not be deleted."
                });
            }

            return NoContent();
        }
    }
}