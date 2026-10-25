using Microsoft.Data.SqlClient;
using StudentAdminPortal.Models;

namespace StudentAdminPortal.Data;


public class StudentRepository
{
    private readonly string _connectionString;

    public StudentRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    private SqlConnection CreateConnection() => new(_connectionString);

    private static Student MapStudent(SqlDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
        LastName = reader.GetString(reader.GetOrdinal("LastName")),
        Email = reader.GetString(reader.GetOrdinal("Email")),
        Course = reader.GetString(reader.GetOrdinal("Course")),
        RegistrationDate = reader.GetDateTime(reader.GetOrdinal("RegistrationDate"))
    };

    public async Task<List<Student>> GetAllAsync()
    {
        var students = new List<Student>();
        await using var connection = CreateConnection();
        await using var command = new SqlCommand(
            "SELECT Id, FirstName, LastName, Email, Course, RegistrationDate FROM Students ORDER BY Id",
            connection);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            students.Add(MapStudent(reader));
        }
        return students;
    }

    public async Task<List<Student>> SearchAsync(string term)
    {
        var students = new List<Student>();
        await using var connection = CreateConnection();
        await using var command = new SqlCommand(
            @"SELECT Id, FirstName, LastName, Email, Course, RegistrationDate
              FROM Students
              WHERE FirstName LIKE @Term OR LastName LIKE @Term OR Email LIKE @Term
              ORDER BY Id",
            connection);
        command.Parameters.AddWithValue("@Term", $"%{term}%");
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            students.Add(MapStudent(reader));
        }
        return students;
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        await using var connection = CreateConnection();
        await using var command = new SqlCommand(
            "SELECT Id, FirstName, LastName, Email, Course, RegistrationDate FROM Students WHERE Id = @Id",
            connection);
        command.Parameters.AddWithValue("@Id", id);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapStudent(reader) : null;
    }

    public async Task<int> AddAsync(Student student)
    {
        await using var connection = CreateConnection();
        // RegistrationDate is filled by the database default (SYSUTCDATETIME()),
        // so it is not part of the INSERT.
        await using var command = new SqlCommand(
            @"INSERT INTO Students (FirstName, LastName, Email, Course)
              VALUES (@FirstName, @LastName, @Email, @Course);
              SELECT CAST(SCOPE_IDENTITY() AS int);",
            connection);
        command.Parameters.AddWithValue("@FirstName", student.FirstName);
        command.Parameters.AddWithValue("@LastName", student.LastName);
        command.Parameters.AddWithValue("@Email", student.Email);
        command.Parameters.AddWithValue("@Course", student.Course);
        await connection.OpenAsync();
        return (int)(await command.ExecuteScalarAsync())!;
    }

    public async Task<bool> UpdateAsync(Student student)
    {
        await using var connection = CreateConnection();
        await using var command = new SqlCommand(
            @"UPDATE Students
              SET FirstName = @FirstName, LastName = @LastName, Email = @Email, Course = @Course
              WHERE Id = @Id",
            connection);
        command.Parameters.AddWithValue("@FirstName", student.FirstName);
        command.Parameters.AddWithValue("@LastName", student.LastName);
        command.Parameters.AddWithValue("@Email", student.Email);
        command.Parameters.AddWithValue("@Course", student.Course);
        command.Parameters.AddWithValue("@Id", student.Id);
        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var connection = CreateConnection();
        await using var command = new SqlCommand("DELETE FROM Students WHERE Id = @Id", connection);
        command.Parameters.AddWithValue("@Id", id);
        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync() > 0;
    }


    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
    {
        await using var connection = CreateConnection();
        var sql = "SELECT COUNT(1) FROM Students WHERE LOWER(Email) = LOWER(@Email)";
        if (excludeId.HasValue)
        {
            sql += " AND Id <> @ExcludeId";
        }
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Email", email);
        if (excludeId.HasValue)
        {
            command.Parameters.AddWithValue("@ExcludeId", excludeId.Value);
        }
        await connection.OpenAsync();
        return (int)(await command.ExecuteScalarAsync())! > 0;
    }

    public async Task<List<Student>> GetRecentAsync(int count)
    {
        var students = new List<Student>();
        await using var connection = CreateConnection();
        await using var command = new SqlCommand(
            @"SELECT TOP (@Count) Id, FirstName, LastName, Email, Course, RegistrationDate
              FROM Students
              ORDER BY RegistrationDate DESC, Id DESC",
            connection);
        command.Parameters.AddWithValue("@Count", count);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            students.Add(MapStudent(reader));
        }
        return students;
    }


    public async Task<(int TotalStudents, int TotalCourses)> GetStatisticsAsync()
    {
        await using var connection = CreateConnection();
        await using var command = new SqlCommand(
            "SELECT COUNT(*), COUNT(DISTINCT Course) FROM Students",
            connection);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        return (reader.GetInt32(0), reader.GetInt32(1));
    }
}
