using KimlikVePersonelServisi.Api.Contracts.Kimlik;
using KimlikVePersonelServisi.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace KimlikVePersonelServisi.Api.Controllers;

[ApiController]
[Route("api/kimlik")]
public sealed class KimlikController(IKimlikPersonelServisi kimlikPersonelServisi) : ControllerBase
{
    [HttpPost("giris")]
    public ActionResult<GirisCevap> Giris([FromBody] GirisIstek istek)
    {
        var sonuc = kimlikPersonelServisi.GirisYap(istek);
        return sonuc.BasariliMi
            ? Ok(sonuc.Veri)
            : BadRequest(new { hata = sonuc.Hata });
    }
}
