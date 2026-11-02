using Attendance.Application.Interfaces;
using Attendance.domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Attendance.Infrastructure.Repositories;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly string _connectionString;

    public AttendanceRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection was not found.");
    }

    public async Task<AttendanceRecord?> GetOpenAttendanceForEmployeeAsync(int employeeId)
    {
        const string query = @"
            SELECT TOP 1 AttendanceId, EmployeeId, AttendanceDate, Status, PunchInTime, PunchOutTime, Notes, MarkedBy
            FROM AttendanceRecords
            WHERE EmployeeId = @EmployeeId AND PunchOutTime IS NULL
            ORDER BY PunchInTime DESC;";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(query, connection);
        command.Parameters.Add("@EmployeeId", System.Data.SqlDbType.Int).Value = employeeId;

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new AttendanceRecord
            {
                AttendanceId = reader.GetInt32(reader.GetOrdinal("AttendanceId")),
                EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                AttendanceDate = reader.GetDateTime(reader.GetOrdinal("AttendanceDate")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                PunchInTime = reader.GetDateTime(reader.GetOrdinal("PunchInTime")),
                PunchOutTime = reader.IsDBNull(reader.GetOrdinal("PunchOutTime")) ? null : reader.GetDateTime(reader.GetOrdinal("PunchOutTime")),
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                MarkedBy = reader.GetInt32(reader.GetOrdinal("MarkedBy"))
            };
        }

        return null;
    }

    public async Task<int> CreateAttendanceAsync(AttendanceRecord attendance)
    {
        const string query = @"
            INSERT INTO AttendanceRecords (EmployeeId, AttendanceDate, Status, PunchInTime, PunchOutTime, Notes, MarkedBy)
            OUTPUT INSERTED.AttendanceId
            VALUES (@EmployeeId, @AttendanceDate, @Status, @PunchInTime, @PunchOutTime, @Notes, @MarkedBy);";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(query, connection);

        command.Parameters.Add("@EmployeeId", System.Data.SqlDbType.Int).Value = attendance.EmployeeId;
        command.Parameters.Add("@AttendanceDate", System.Data.SqlDbType.Date).Value = attendance.AttendanceDate;
        command.Parameters.Add("@Status", System.Data.SqlDbType.NVarChar, 50).Value = attendance.Status;
        command.Parameters.Add("@PunchInTime", System.Data.SqlDbType.DateTime).Value = attendance.PunchInTime;
        command.Parameters.Add("@PunchOutTime", System.Data.SqlDbType.DateTime).Value = (object?)attendance.PunchOutTime ?? DBNull.Value;
        command.Parameters.Add("@Notes", System.Data.SqlDbType.NVarChar, 500).Value = (object?)attendance.Notes ?? DBNull.Value;
        command.Parameters.Add("@MarkedBy", System.Data.SqlDbType.Int).Value = attendance.MarkedBy;

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> SetPunchOutAsync(int attendanceId, DateTime punchOutTime, string? notes, string status)
    {
        const string query = @"
        UPDATE AttendanceRecords
        SET PunchOutTime = @PunchOutTime, Notes = @Notes, Status = @Status
        WHERE AttendanceId = @AttendanceId;";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(query, connection);

        command.Parameters.Add("@AttendanceId", System.Data.SqlDbType.Int).Value = attendanceId;
        command.Parameters.Add("@PunchOutTime", System.Data.SqlDbType.DateTime).Value = punchOutTime;
        command.Parameters.Add("@Notes", System.Data.SqlDbType.NVarChar, 500).Value = (object?)notes ?? DBNull.Value;
        command.Parameters.Add("@Status", System.Data.SqlDbType.NVarChar, 50).Value = status;

        await connection.OpenAsync();
        int rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<List<AttendanceRecord>> GetAllAttendanceAsync()
    {
        var records = new List<AttendanceRecord>();

        const string query = @"
            SELECT AttendanceId, EmployeeId, AttendanceDate, Status, PunchInTime, PunchOutTime, Notes, MarkedBy
            FROM AttendanceRecords
            ORDER BY AttendanceDate DESC, PunchInTime DESC;";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(query, connection);

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            records.Add(MapRecord(reader));
        }

        return records;
    }

    public async Task<AttendanceRecord?> GetAttendanceByIdAsync(int id)
    {
        const string query = @"
            SELECT AttendanceId, EmployeeId, AttendanceDate, Status, PunchInTime, PunchOutTime, Notes, MarkedBy
            FROM AttendanceRecords
            WHERE AttendanceId = @AttendanceId;";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(query, connection);
        command.Parameters.Add("@AttendanceId", System.Data.SqlDbType.Int).Value = id;

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapRecord(reader);
        }

        return null;
    }

    public async Task<bool> UpdateAttendanceAsync(int id, AttendanceRecord attendance)
    {
        const string query = @"
            UPDATE AttendanceRecords
            SET EmployeeId = @EmployeeId,
                AttendanceDate = @AttendanceDate,
                Status = @Status,
                PunchInTime = @PunchInTime,
                PunchOutTime = @PunchOutTime,
                Notes = @Notes,
                MarkedBy = @MarkedBy
            WHERE AttendanceId = @AttendanceId;";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(query, connection);

        command.Parameters.Add("@AttendanceId", System.Data.SqlDbType.Int).Value = id;
        command.Parameters.Add("@EmployeeId", System.Data.SqlDbType.Int).Value = attendance.EmployeeId;
        command.Parameters.Add("@AttendanceDate", System.Data.SqlDbType.Date).Value = attendance.AttendanceDate;
        command.Parameters.Add("@Status", System.Data.SqlDbType.NVarChar, 50).Value = attendance.Status;
        command.Parameters.Add("@PunchInTime", System.Data.SqlDbType.DateTime).Value = (object?)attendance.PunchInTime ?? DBNull.Value;
        command.Parameters.Add("@PunchOutTime", System.Data.SqlDbType.DateTime).Value = (object?)attendance.PunchOutTime ?? DBNull.Value;
        command.Parameters.Add("@Notes", System.Data.SqlDbType.NVarChar, 500).Value = (object?)attendance.Notes ?? DBNull.Value;
        command.Parameters.Add("@MarkedBy", System.Data.SqlDbType.Int).Value = attendance.MarkedBy;

        await connection.OpenAsync();
        int rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> DeleteAttendanceAsync(int id)
    {
        const string query = @"DELETE FROM AttendanceRecords WHERE AttendanceId = @AttendanceId;";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(query, connection);
        command.Parameters.Add("@AttendanceId", System.Data.SqlDbType.Int).Value = id;

        await connection.OpenAsync();
        int rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> AttendanceExistsAsync(int employeeId, DateTime attendanceDate)
    {
        const string query = @"
            SELECT COUNT(1)
            FROM AttendanceRecords
            WHERE EmployeeId = @EmployeeId AND AttendanceDate = @AttendanceDate;";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(query, connection);
        command.Parameters.Add("@EmployeeId", System.Data.SqlDbType.Int).Value = employeeId;
        command.Parameters.Add("@AttendanceDate", System.Data.SqlDbType.Date).Value = attendanceDate.Date;

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    private static AttendanceRecord MapRecord(SqlDataReader reader)
    {
        return new AttendanceRecord
        {
            AttendanceId = reader.GetInt32(reader.GetOrdinal("AttendanceId")),
            EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
            AttendanceDate = reader.GetDateTime(reader.GetOrdinal("AttendanceDate")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            PunchInTime = reader.IsDBNull(reader.GetOrdinal("PunchInTime")) ? null : reader.GetDateTime(reader.GetOrdinal("PunchInTime")),
            PunchOutTime = reader.IsDBNull(reader.GetOrdinal("PunchOutTime")) ? null : reader.GetDateTime(reader.GetOrdinal("PunchOutTime")),
            Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
            MarkedBy = reader.GetInt32(reader.GetOrdinal("MarkedBy"))
        };
    }
}