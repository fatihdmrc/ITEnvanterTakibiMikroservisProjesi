using KimlikVePersonelServisi.Api.Contracts.Kullanicilar;
using KimlikVePersonelServisi.Api.Sabitler;
using KimlikVePersonelServisi.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KimlikVePersonelServisi.Api.Controllers;

[ApiController]
[Route("api/kullanicilar")]
[Authorize(Policy = KimlikPersonelMesajlari.SadeceAdminPolicy)]
public sealed class KullanicilarController(IKimlikPersonelServisi kimlikPersonelServisi) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<KullaniciCevap>>> Listele(CancellationToken cancellationToken)
    {
        return Ok(await kimlikPersonelServisi.KullanicilariListeleAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<KullaniciCevap>> Olustur([FromBody] KullaniciOlusturIstek istek, CancellationToken cancellationToken)
    {
        var sonuc = await kimlikPersonelServisi.KullaniciOlusturAsync(istek, cancellationToken);
        return sonuc.BasariliMi
            ? Created($"/api/kullanicilar/{sonuc.Veri!.Id}", sonuc.Veri)
            : BadRequest(new { hata = sonuc.Hata });
    }
}
