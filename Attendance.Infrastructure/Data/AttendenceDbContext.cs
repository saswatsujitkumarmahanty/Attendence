using Microsoft.EntityFrameworkCore;
using Attendance.domain.Entities;
namespace Attendance.Data
{


    public class AttendanceDbContext : DbContext
    {
        public AttendanceDbContext(
            DbContextOptions<AttendanceDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<AttendanceRecord> Attendances { get; set; }

        public DbSet<User> Users { get; set; }
    }
}