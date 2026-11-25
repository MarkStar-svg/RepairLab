using Microsoft.Data.SqlClient;
using RepairLab.Models;
namespace RepairLab.Data;

public class CaseRepository
{
    private readonly Db _db;
    public CaseRepository(Db db) => _db = db;

    private const string Sel = @"SELECT c.Id,c.Title,c.DeviceType,c.Defect,c.Method,c.Scale,c.Description,c.Status,c.UserId,u.FullName,c.CreatedAt
                                 FROM Cases c JOIN Users u ON u.Id=c.UserId ";

    private static RepairCase Map(SqlDataReader r) => new()
    {
        Id = r.GetInt32(0), Title = r.GetString(1), DeviceType = r.GetString(2), Defect = r.GetString(3),
        Method = r.GetString(4), Scale = r.GetString(5), Description = r.GetString(6), Status = r.GetString(7),
        UserId = r.GetInt32(8), AuthorName = r.GetString(9), CreatedAt = r.GetDateTime(10)
    };

    private List<RepairCase> Many(string tail, Action<SqlCommand> bind)
    {
        var list = new List<RepairCase>();
        using var c = _db.Open();
        using var cmd = new SqlCommand(Sel + tail, c); bind(cmd);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(Map(r));
        return list;
    }

    public List<RepairCase> GetLatest(int n) => Many("ORDER BY c.CreatedAt DESC, c.Id DESC OFFSET 0 ROWS FETCH NEXT @n ROWS ONLY", x => x.Parameters.AddWithValue("@n", n));
    public List<RepairCase> GetAll(string q) => Many("WHERE (@q IS NULL OR c.Title LIKE @like OR c.Defect LIKE @like OR c.DeviceType LIKE @like) ORDER BY c.CreatedAt DESC, c.Id DESC", x =>
    {
        x.Parameters.AddWithValue("@q", string.IsNullOrWhiteSpace(q) ? DBNull.Value : q);
        x.Parameters.AddWithValue("@like", "%" + q + "%");
    });
    public List<RepairCase> GetByUser(int userId) => Many("WHERE c.UserId=@u ORDER BY c.CreatedAt DESC", x => x.Parameters.AddWithValue("@u", userId));
    public RepairCase GetById(int id) => Many("WHERE c.Id=@id", x => x.Parameters.AddWithValue("@id", id)).FirstOrDefault();
    // проверка владельца: WHERE Id = @Id AND UserId = @UserId
    public RepairCase GetOwned(int id, int userId) => Many("WHERE c.Id=@id AND c.UserId=@u", x => { x.Parameters.AddWithValue("@id", id); x.Parameters.AddWithValue("@u", userId); }).FirstOrDefault();

    public int CountByUser(int userId)
    {
        using var c = _db.Open();
        using var cmd = new SqlCommand("SELECT COUNT(*) FROM Cases WHERE UserId=@u", c);
        cmd.Parameters.AddWithValue("@u", userId);
        return (int)cmd.ExecuteScalar();
    }

    private static void Bind(SqlCommand cmd, RepairCase x)
    {
        cmd.Parameters.AddWithValue("@t", x.Title); cmd.Parameters.AddWithValue("@d", x.DeviceType);
        cmd.Parameters.AddWithValue("@f", x.Defect); cmd.Parameters.AddWithValue("@m", x.Method);
        cmd.Parameters.AddWithValue("@s", x.Scale); cmd.Parameters.AddWithValue("@ds", x.Description);
        cmd.Parameters.AddWithValue("@u", x.UserId);
    }

    public void Create(RepairCase x)
    {
        using var c = _db.Open();
        using var cmd = new SqlCommand("INSERT INTO Cases(Title,DeviceType,Defect,Method,Scale,Description,UserId) VALUES(@t,@d,@f,@m,@s,@ds,@u)", c);
        Bind(cmd, x); cmd.ExecuteNonQuery();
    }
    public bool Update(RepairCase x)
    {
        using var c = _db.Open();
        using var cmd = new SqlCommand("UPDATE Cases SET Title=@t,DeviceType=@d,Defect=@f,Method=@m,Scale=@s,Description=@ds WHERE Id=@id AND UserId=@u", c);
        Bind(cmd, x); cmd.Parameters.AddWithValue("@id", x.Id);
        return cmd.ExecuteNonQuery() > 0;
    }
    public bool Delete(int id, int userId)
    {
        using var c = _db.Open();
        using var cmd = new SqlCommand("DELETE FROM Cases WHERE Id=@id AND UserId=@u", c);
        cmd.Parameters.AddWithValue("@id", id); cmd.Parameters.AddWithValue("@u", userId);
        return cmd.ExecuteNonQuery() > 0;
    }
    public void DeleteAny(int id)
    {
        using var c = _db.Open();
        using var cmd = new SqlCommand("DELETE FROM Cases WHERE Id=@id", c);
        cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery();
    }
}
