using EnvanterServisi.Api.Contracts.SarfMalzemeler;
using EnvanterServisi.Api.Contracts.Stok;
using EnvanterServisi.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnvanterServisi.Api.Controllers;

[ApiController]
[Route("api/sarf-malzemeler")]
[Authorize(Policy = "AdminVeyaITPersoneli")]
public sealed class SarfMalzemelerController(IEnvanterServisi envanterServisi) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<SarfMalzemeCevap>>> Listele(
        [FromQuery] Guid? kategoriId,
        [FromQuery] Guid? lokasyonId,
        [FromQuery] string? arama,
        CancellationToken cancellationToken)
    {
        return Ok(await envanterServisi.SarfMalzemeleriListeleAsync(kategoriId, lokasyonId, arama, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SarfMalzemeCevap>> Getir(Guid id, CancellationToken cancellationToken)
    {
        var sarfMalzeme = await envanterServisi.SarfMalzemeGetirAsync(id, cancellationToken);
        return sarfMalzeme is null ? NotFound(new { hata = "Sarf malzeme bulunamadı." }) : Ok(sarfMalzeme);
    }

    [HttpPost]
    public async Task<ActionResult<SarfMalzemeCevap>> Olustur([FromBody] SarfMalzemeOlusturIstek istek, CancellationToken cancellationToken)
    {
        var sonuc = await envanterServisi.SarfMalzemeOlusturAsync(istek, cancellationToken);
        return sonuc.BasariliMi
            ? CreatedAtAction(nameof(Getir), new { id = sonuc.Veri!.Id }, sonuc.Veri)
            : BadRequest(new { hata = sonuc.Hata });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SarfMalzemeCevap>> Guncelle(Guid id, [FromBody] SarfMalzemeGuncelleIstek istek, CancellationToken cancellationToken)
    {
        var sonuc = await envanterServisi.SarfMalzemeGuncelleAsync(id, istek, cancellationToken);
        return sonuc.BasariliMi ? Ok(sonuc.Veri) : BadRequest(new { hata = sonuc.Hata });
    }

    [HttpPost("{id:guid}/stok-hareketleri")]
    public async Task<ActionResult<SarfMalzemeCevap>> StokHareketiIsle(Guid id, [FromBody] SarfMalzemeStokHareketiIstek istek, CancellationToken cancellationToken)
    {
        var kullaniciId = KullaniciIdGetir();
        if (!kullaniciId.HasValue)
        {
            return Unauthorized(new { hata = "Token içinde KullaniciId bilgisi bulunamadı." });
        }

        var sonuc = await envanterServisi.SarfMalzemeStokHareketiIsleAsync(id, istek, kullaniciId.Value, cancellationToken);
        return sonuc.BasariliMi ? Ok(sonuc.Veri) : BadRequest(new { hata = sonuc.Hata });
    }

    private Guid? KullaniciIdGetir()
    {
        var claimDegeri = User.FindFirst("KullaniciId")?.Value;
        return Guid.TryParse(claimDegeri, out var kullaniciId) ? kullaniciId : null;
    }
}
