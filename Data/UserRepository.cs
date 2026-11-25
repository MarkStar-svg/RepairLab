using Microsoft.Data.SqlClient;
using RepairLab.Models;
namespace RepairLab.Data;

public class UserRepository
{
    private readonly Db _db;
    public UserRepository(Db db) => _db = db;

    private static User Map(SqlDataReader r) => new()
    {
        Id = r.GetInt32(0), Email = r.GetString(1), FullName = r.GetString(2),
        PasswordHash = r.GetString(3), Role = r.GetString(4), CreatedAt = r.GetDateTime(5)
    };
    private const string Sel = "SELECT Id,Email,FullName,PasswordHash,Role,CreatedAt FROM Users ";

    private User One(string where, Action<SqlCommand> bind)
    {
        using var c = _db.Open();
        using var cmd = new SqlCommand(Sel + where, c); bind(cmd);
        using var r = cmd.ExecuteReader();
        return r.Read() ? Map(r) : null;
    }
    public User GetByEmail(string email) => One("WHERE Email=@e", c => c.Parameters.AddWithValue("@e", email));
    public User GetById(int id) => One("WHERE Id=@id", c => c.Parameters.AddWithValue("@id", id));

    public int Create(string email, string name, string hash)
    {
        using var c = _db.Open();
        using var cmd = new SqlCommand("INSERT INTO Users(Email,FullName,PasswordHash,Role) OUTPUT INSERTED.Id VALUES(@e,@n,@p,'User')", c);
        cmd.Parameters.AddWithValue("@e", email); cmd.Parameters.AddWithValue("@n", name); cmd.Parameters.AddWithValue("@p", hash);
        return (int)cmd.ExecuteScalar();
    }
    public bool EmailTakenByOther(string email, int exceptId)
    {
        using var c = _db.Open();
        using var cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Email=@e AND Id<>@id", c);
        cmd.Parameters.AddWithValue("@e", email); cmd.Parameters.AddWithValue("@id", exceptId);
        return (int)cmd.ExecuteScalar() > 0;
    }
    public void UpdateProfile(int id, string name, string email)
    {
        using var c = _db.Open();
        using var cmd = new SqlCommand("UPDATE Users SET FullName=@n, Email=@e WHERE Id=@id", c);
        cmd.Parameters.AddWithValue("@n", name); cmd.Parameters.AddWithValue("@e", email); cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }
    public void UpdatePassword(int id, string hash)
    {
        using var c = _db.Open();
        using var cmd = new SqlCommand("UPDATE Users SET PasswordHash=@p WHERE Id=@id", c);
        cmd.Parameters.AddWithValue("@p", hash); cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }
}
