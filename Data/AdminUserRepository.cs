using Microsoft.Data.SqlClient;
using StudentAdminPortal.Models;

namespace StudentAdminPortal.Data;

// All database access for administrator accounts, using ADO.NET only.
// Password hashing lives in AccountController (PasswordHasher<AdminUser>);
// this repository only stores and reads the resulting hash, never plain text.
public class AdminUserRepository
{
    private readonly string _connectionString;

    public AdminUserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    private SqlConnection CreateConnection() => new(_connectionString);

    private static AdminUser MapAdminUser(SqlDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        Username = reader.GetString(reader.GetOrdinal("Username")),
        Email = reader.GetString(reader.GetOrdinal("Email")),
        PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
    };

    // Login accepts either the username or the email address.
    public async Task<AdminUser?> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        await using var connection = CreateConnection();
        await using var command = new SqlCommand(
            @"SELECT Id, Username, Email, PasswordHash, CreatedAt
              FROM AdminUsers
              WHERE Username = @Login OR Email = @Login",
            connection);
        command.Parameters.AddWithValue("@Login", usernameOrEmail);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapAdminUser(reader) : null;
    }

    public async Task<int> GetAdminCountAsync()
    {
        await using var connection = CreateConnection();
        await using var command = new SqlCommand("SELECT COUNT(*) FROM AdminUsers", connection);
        await connection.OpenAsync();
        return (int)(await command.ExecuteScalarAsync())!;
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        await using var connection = CreateConnection();
        await using var command = new SqlCommand(
            "SELECT COUNT(1) FROM AdminUsers WHERE LOWER(Username) = LOWER(@Username)", connection);
        command.Parameters.AddWithValue("@Username", username);
        await connection.OpenAsync();
        return (int)(await command.ExecuteScalarAsync())! > 0;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        await using var connection = CreateConnection();
        await using var command = new SqlCommand(
            "SELECT COUNT(1) FROM AdminUsers WHERE LOWER(Email) = LOWER(@Email)", connection);
        command.Parameters.AddWithValue("@Email", email);
        await connection.OpenAsync();
        return (int)(await command.ExecuteScalarAsync())! > 0;
    }

    // admin.PasswordHash must already be set by the caller (see AccountController).
    public async Task<int> CreateAsync(AdminUser admin)
    {
        await using var connection = CreateConnection();
        await using var command = new SqlCommand(
            @"INSERT INTO AdminUsers (Username, Email, PasswordHash)
              VALUES (@Username, @Email, @PasswordHash);
              SELECT CAST(SCOPE_IDENTITY() AS int);",
            connection);
        command.Parameters.AddWithValue("@Username", admin.Username);
        command.Parameters.AddWithValue("@Email", admin.Email);
        command.Parameters.AddWithValue("@PasswordHash", admin.PasswordHash);
        await connection.OpenAsync();
        return (int)(await command.ExecuteScalarAsync())!;
    }
}
