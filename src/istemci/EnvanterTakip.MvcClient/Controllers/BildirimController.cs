using Microsoft.AspNetCore.Mvc;

namespace EnvanterTakip.MvcClient.Controllers;

public sealed class BildirimController(IConfiguration configuration) : Controller
{
    private const string TokenSessionKey = "KimlikToken";
    private const string RolSessionKey = "Rol";

    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult BaglantiBilgisi()
    {
        var token = HttpContext.Session.GetString(TokenSessionKey);
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(new { mesaj = "Bildirim bağlantısı için oturum açılmalıdır." });
        }

        var rol = HttpContext.Session.GetString(RolSessionKey);
        if (rol is not ("Admin" or "ITPersoneli"))
        {
            return Forbid();
        }

        var servisAdresi = configuration["ServisAdresleri:BildirimServisi"] ?? "http://localhost:5004";

        return Ok(new
        {
            hubUrl = $"{servisAdresi.TrimEnd('/')}/hubs/bildirim",
            token
        });
    }
}
