using SQLite;

namespace ConvertisseurDevises.Models;

public class User
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique]
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}