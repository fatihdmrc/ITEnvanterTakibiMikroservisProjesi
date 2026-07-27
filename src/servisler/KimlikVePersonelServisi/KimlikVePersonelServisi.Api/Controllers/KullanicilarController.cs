using KimlikVePersonelServisi.Api.Contracts.Kullanicilar;
using KimlikVePersonelServisi.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KimlikVePersonelServisi.Api.Controllers;

[ApiController]
[Route("api/kullanicilar")]
[Authorize(Policy = "SadeceAdmin")]
public sealed class KullanicilarController(IKimlikPersonelServisi kimlikPersonelServisi) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyCollection<KullaniciCevap>> Listele()
    {
        return Ok(kimlikPersonelServisi.KullanicilariListele());
    }

    [HttpPost]
    public ActionResult<KullaniciCevap> Olustur([FromBody] KullaniciOlusturIstek istek)
    {
        var sonuc = kimlikPersonelServisi.KullaniciOlustur(istek);
        return sonuc.BasariliMi
            ? Created($"/api/kullanicilar/{sonuc.Veri!.Id}", sonuc.Veri)
            : BadRequest(new { hata = sonuc.Hata });
    }
}
