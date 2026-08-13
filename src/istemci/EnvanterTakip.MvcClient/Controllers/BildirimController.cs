using EnvanterTakip.MvcClient.Sabitler;
using Microsoft.AspNetCore.Mvc;

namespace EnvanterTakip.MvcClient.Controllers;

public sealed class BildirimController(IConfiguration configuration) : Controller
{
    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult BaglantiBilgisi()
    {
        var token = HttpContext.Session.GetString(MvcSabitleri.TokenSessionKey);
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(new { mesaj = MvcMesajlari.BildirimOturumYok });
        }

        var rol = HttpContext.Session.GetString(MvcSabitleri.RolSessionKey);
        if (rol is not (MvcSabitleri.AdminRolu or MvcSabitleri.ITPersoneliRolu))
        {
            return Forbid();
        }

        var servisAdresi = configuration["ServisAdresleri:BildirimServisi"] ?? MvcSabitleri.VarsayilanBildirimServisiAdresi;

        return Ok(new
        {
            hubUrl = $"{servisAdresi.TrimEnd('/')}{MvcSabitleri.BildirimHubYolu}",
            token
        });
    }
}
