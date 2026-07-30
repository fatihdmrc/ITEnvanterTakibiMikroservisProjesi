using KimlikVePersonelServisi.Api.Contracts.Kimlik;
using KimlikVePersonelServisi.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace KimlikVePersonelServisi.Api.Controllers;

[ApiController]
[Route("api/kimlik")]
public sealed class KimlikController(IKimlikPersonelServisi kimlikPersonelServisi) : ControllerBase
{
    [HttpPost("giris")]
    public async Task<ActionResult<GirisCevap>> Giris([FromBody] GirisIstek istek, CancellationToken cancellationToken)
    {
        var sonuc = await kimlikPersonelServisi.GirisYapAsync(istek, cancellationToken);
        return sonuc.BasariliMi
            ? Ok(sonuc.Veri)
            : BadRequest(new { hata = sonuc.Hata });
    }
}
