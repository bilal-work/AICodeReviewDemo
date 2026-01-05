using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Text;

namespace AICodeReviewDemo.Controllers;

[ApiController]
[Route("[controller]")]
public class LegacyUserController : ControllerBase
{
    // ISSUE 1: Hard-coded connection string + secret
    private readonly string _connectionString =
        "Server=10.0.0.5;Database=CorpDb;User Id=corp_user;Password=ReallySecret123;";

    // ISSUE 2: Logger optional, dependency injection karışık
    private readonly ILogger<LegacyUserController>? _logger;

    // Demo için parametresiz ctor da var (testler kullanıyor)
    public LegacyUserController()
    {
    }

    public LegacyUserController(ILogger<LegacyUserController> logger)
    {
        _logger = logger;
    }

    // -----------------------------------------------------------
    // LOGIN – En çok hata olan endpoint
    // -----------------------------------------------------------
    [HttpGet("login")]
    public IActionResult Login(string username, string password)
    {
        // ISSUE 3: Null / boş kontrolü yok, validation yok
        // ISSUE 4: SQL Injection (string concatenation)
        string query =
            "SELECT TOP 1 * FROM Users WHERE Username = '" + username + "' AND Password = '" + password + "'";

        // ISSUE 5: using yok, manual connection yönetimi
        SqlConnection conn = new SqlConnection(_connectionString);
        conn.Open();

        SqlCommand cmd = new SqlCommand(query, conn);

        try
        {
            var reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                // ISSUE 6: Sensitive data logging (şifre log’da)
                _logger?.LogInformation("User {Username} logged in with password {Password}", username, password);

                // ISSUE 7: Çok genel bir success message, MFA / lockout vs yok
                return Ok("Giriş Başarılı");
            }

            return Unauthorized("Kullanıcı adı veya şifre hatalı");
        }
        catch (Exception ex)
        {
            // ISSUE 8: Kullanıcıya tüm exception detayını dönme
            return StatusCode(500, "Hata: " + ex.ToString());
        }
        finally
        {
            // ISSUE 9: finally içinde Close, Dispose yok, hata fırlatabilir
            conn.Close();
        }
    }

    // -----------------------------------------------------------
    // REGISTER – Plain text şifre, duplicate validation, swallow
    // -----------------------------------------------------------
    [HttpPost("register")]
    public IActionResult Register(string username, string password, string email)
    {
        // ISSUE 10: Tekrarlayan primitive validation, model yok
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            return BadRequest("Username and password are required");
        }

        // ISSUE 11: Plain text password, hashing/salting yok
        string insertSql =
            "INSERT INTO Users (Username, Password, Email, CreatedAt) " +
            "VALUES ('" + username + "', '" + password + "', '" + email + "', GETDATE())";

        SqlConnection conn = new SqlConnection(_connectionString);
        conn.Open();

        SqlCommand cmd = new SqlCommand(insertSql, conn);

        try
        {
            int rows = cmd.ExecuteNonQuery();
            if (rows == 1)
            {
                return Ok("Kullanıcı oluşturuldu");
            }

            // ISSUE 12: “Magic string” error, detay yok
            return StatusCode(500, "Unexpected row count");
        }
        catch
        {
            // ISSUE 13: Exception swallow – log yok, root cause kayıp
            return BadRequest("Kullanıcı oluşturulamadı");
        }
        finally
        {
            conn.Close();
        }
    }

    // -----------------------------------------------------------
    // PROFIL – Fazla bilgi dönen endpoint
    // -----------------------------------------------------------
    [HttpGet("profile")]
    public IActionResult GetProfile(string username)
    {
        // ISSUE 14: SQL injection tekrar
        string query = "SELECT TOP 1 * FROM Users WHERE Username = '" + username + "'";

        SqlConnection conn = new SqlConnection(_connectionString);
        conn.Open();
        SqlCommand cmd = new SqlCommand(query, conn);

        try
        {
            var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return NotFound();

            // ISSUE 15: Domain model yok, raw DB alanlarını direkt dönen anonymous object
            // ISSUE 16: Password veya sensitive kolonlar maskelenmiyor
            var user = new
            {
                Id = reader["Id"],
                Username = reader["Username"],
                Email = reader["Email"],
                Password = reader["Password"],   // <-- çok kritik
                CreatedAt = reader["CreatedAt"]
            };

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Profil getirilirken hata oluştu");
            return StatusCode(500, "Profil alınamadı");
        }
        finally
        {
            conn.Close();
        }
    }

    // -----------------------------------------------------------
    // EXPORT – Memory / performans ve data correctness sorunlu
    // -----------------------------------------------------------
    [HttpGet("export")]
    public IActionResult ExportUsersToCsv()
    {
        // ISSUE 17: Paging yok, bütün tabloyu RAM’e çekiyor
        const string query = "SELECT Id, Username, Email, CreatedAt FROM Users";

        SqlConnection conn = new SqlConnection(_connectionString);
        conn.Open();
        SqlCommand cmd = new SqlCommand(query, conn);

        var sb = new StringBuilder();
        sb.AppendLine("Id,Username,Email,CreatedAt");

        try
        {
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                // ISSUE 18: CSV escaping yok, virgüllü değerleri bozabilir
                sb.Append(reader["Id"]).Append(',')
                  .Append(reader["Username"]).Append(',')
                  .Append(reader["Email"]).Append(',')
                  .Append(reader["CreatedAt"]).AppendLine();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Export sırasında hata");
            // ISSUE 19: Hata mesajı user’a aynen gidiyor, localization yok
            return StatusCode(500, "Export failed: " + ex.Message);
        }
        finally
        {
            conn.Close();
        }

        // ISSUE 20: Streaming yok, büyük dosyalarda memory spike
        byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", "users.csv");
    }

    // -----------------------------------------------------------
    // RISK CALC – AI edge-case test için güzel bir örnek
    // -----------------------------------------------------------
    [HttpPost("calculate-risk")]
    public int CalculateRisk(int age, int income, int debt, bool hasCard)
    {
        // Refactored to reduce cognitive complexity while preserving behavior
        if (age <= 18)
        {
            return 0;
        }

        if (income <= 5000)
        {
            return 10;
        }

        if (debt < 1000)
        {
            return hasCard ? 100 : 80;
        }

        return debt < 5000 ? 50 : 20;
    }
}
