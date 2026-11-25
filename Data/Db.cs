using Microsoft.Data.SqlClient;
namespace RepairLab.Data;

public class Db
{
    private readonly string _cs;
    public Db(IConfiguration cfg) => _cs = cfg.GetConnectionString("Default");
    public SqlConnection Open() { var c = new SqlConnection(_cs); c.Open(); return c; }

    public void Init()
    {
        var sb = new SqlConnectionStringBuilder(_cs);
        var name = sb.InitialCatalog; sb.InitialCatalog = "master";
        using (var m = new SqlConnection(sb.ConnectionString))
        {
            m.Open();
            using var cmd = new SqlCommand($"IF DB_ID(N'{name}') IS NULL CREATE DATABASE [{name}]", m);
            cmd.ExecuteNonQuery();
        }
        using var c = Open();
        new SqlCommand(@"
IF OBJECT_ID('Users') IS NULL CREATE TABLE Users(
  Id INT IDENTITY PRIMARY KEY, Email NVARCHAR(200) NOT NULL UNIQUE, FullName NVARCHAR(100) NOT NULL,
  PasswordHash NVARCHAR(200) NOT NULL, Role NVARCHAR(20) NOT NULL DEFAULT 'User', CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME());
IF OBJECT_ID('Cases') IS NULL CREATE TABLE Cases(
  Id INT IDENTITY PRIMARY KEY, Title NVARCHAR(120) NOT NULL, DeviceType NVARCHAR(50) NOT NULL, Defect NVARCHAR(200) NOT NULL,
  Method NVARCHAR(100) NOT NULL, Scale NVARCHAR(10) NOT NULL, Description NVARCHAR(2000) NOT NULL, Status NVARCHAR(30) NOT NULL DEFAULT N'Новая',
  UserId INT NOT NULL REFERENCES Users(Id), CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME());", c).ExecuteNonQuery();

        if ((int)new SqlCommand("SELECT COUNT(*) FROM Users", c).ExecuteScalar() > 0) return;

        // демо-данные: админ admin@repairlab.local / Admin123
        var ins = new SqlCommand("INSERT INTO Users(Email,FullName,PasswordHash,Role) OUTPUT INSERTED.Id VALUES(@e,@n,@p,'Admin')", c);
        ins.Parameters.AddWithValue("@e", "admin@repairlab.local");
        ins.Parameters.AddWithValue("@n", "Администратор");
        ins.Parameters.AddWithValue("@p", PasswordHasher.Hash("Admin123"));
        var adminId = (int)ins.ExecuteScalar();

        var seed = new (string t, string d, string f, string m, string s, string desc, string st)[]
        {
            ("Видеокарта: обрыв дорожки под BGA", "Видеокарта", "Обрыв внутренней дорожки GPU-подложки", "FIB-восстановление дорожки (напыление Pt/W)", "мкм", "Сервисы отказали: дефект скрыт между слоями подложки. SEM локализует разрыв, FIB вскрывает слой и восстанавливает проводник напылением.", "Готово"),
            ("Ноутбук: прогар переходного отверстия (via)", "Ноутбук", "Прогар via в многослойной плате", "FIB-срез и локализация дефекта", "нм", "Поперечный срез через via на нанометровом разрешении показывает место деградации металлизации.", "В работе"),
            ("Материнская плата ПК: короткое замыкание слоёв", "ПК / материнская плата", "КЗ между внутренними слоями", "SEM-диагностика", "мкм", "Диагностика без разборки на уровне слоёв: поиск проводящего мостика и оценка возможности ремонта.", "Готово"),
            ("Прогар медной дорожки текстолита", "Ноутбук", "Выгорание участка дорожки питания", "Раскрытие и расплетение металлизации слоёв", "мкм", "Послойное раскрытие металлизации и восстановление проводящего пути в зоне прогара.", "Новая"),
            ("Видеокарта: микротрещина контактной площадки", "Видеокарта", "Трещина pad под чипом памяти", "FIB-восстановление дорожки (напыление Pt/W)", "нм", "Микротрещина видна только в SEM; восстановление контакта локальным напылением металла.", "Готово"),
        };
        foreach (var x in seed)
        {
            var cmd = new SqlCommand("INSERT INTO Cases(Title,DeviceType,Defect,Method,Scale,Description,Status,UserId) VALUES(@t,@d,@f,@m,@s,@ds,@st,@u)", c);
            cmd.Parameters.AddWithValue("@t", x.t); cmd.Parameters.AddWithValue("@d", x.d); cmd.Parameters.AddWithValue("@f", x.f);
            cmd.Parameters.AddWithValue("@m", x.m); cmd.Parameters.AddWithValue("@s", x.s); cmd.Parameters.AddWithValue("@ds", x.desc);
            cmd.Parameters.AddWithValue("@st", x.st); cmd.Parameters.AddWithValue("@u", adminId);
            cmd.ExecuteNonQuery();
        }
    }
}
