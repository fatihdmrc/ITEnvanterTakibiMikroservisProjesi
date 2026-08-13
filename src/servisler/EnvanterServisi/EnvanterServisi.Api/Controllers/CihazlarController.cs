using EnvanterServisi.Api.Contracts.Cihazlar;
using EnvanterServisi.Api.Contracts.Stok;
using EnvanterServisi.Api.Domain.Enums;
using EnvanterServisi.Api.Sabitler;
using EnvanterServisi.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnvanterServisi.Api.Controllers;

[ApiController]
[Route("api/cihazlar")]
[Authorize(Policy = "AdminVeyaITPersoneli")]
public sealed class CihazlarController(IEnvanterServisi envanterServisi) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CihazCevap>>> Listele(
        [FromQuery] Guid? kategoriId,
        [FromQuery] Guid? lokasyonId,
        [FromQuery] bool? aktifMi,
        [FromQuery] CihazDurumu? durum,
        [FromQuery] string? arama,
        CancellationToken cancellationToken)
    {
        return Ok(await envanterServisi.CihazlariListeleAsync(kategoriId, lokasyonId, aktifMi, durum, arama, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CihazCevap>> Getir(Guid id, CancellationToken cancellationToken)
    {
        var cihaz = await envanterServisi.CihazGetirAsync(id, cancellationToken);
        return cihaz is null ? NotFound(new { hata = EnvanterMesajlari.CihazBulunamadi }) : Ok(cihaz);
    }

    [HttpPost]
    public async Task<ActionResult<CihazCevap>> Olustur([FromBody] CihazOlusturIstek istek, CancellationToken cancellationToken)
    {
        var sonuc = await envanterServisi.CihazOlusturAsync(istek, cancellationToken);
        return sonuc.BasariliMi
            ? CreatedAtAction(nameof(Getir), new { id = sonuc.Veri!.Id }, sonuc.Veri)
            : BadRequest(new { hata = sonuc.Hata });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CihazCevap>> Guncelle(Guid id, [FromBody] CihazGuncelleIstek istek, CancellationToken cancellationToken)
    {
        var sonuc = await envanterServisi.CihazGuncelleAsync(id, istek, cancellationToken);
        return sonuc.BasariliMi ? Ok(sonuc.Veri) : BadRequest(new { hata = sonuc.Hata });
    }

    [HttpPost("{id:guid}/durum-hareketleri")]
    public async Task<ActionResult<CihazCevap>> DurumHareketiIsle(Guid id, [FromBody] CihazDurumHareketiIstek istek, CancellationToken cancellationToken)
    {
        var kullaniciId = KullaniciIdGetir();
        if (!kullaniciId.HasValue)
        {
            return Unauthorized(new { hata = EnvanterMesajlari.TokenKullaniciIdYok });
        }

        var sonuc = await envanterServisi.CihazDurumHareketiIsleAsync(id, istek, kullaniciId.Value, cancellationToken);
        return sonuc.BasariliMi ? Ok(sonuc.Veri) : BadRequest(new { hata = sonuc.Hata });
    }

    private Guid? KullaniciIdGetir()
    {
        var claimDegeri = User.FindFirst("KullaniciId")?.Value;
        return Guid.TryParse(claimDegeri, out var kullaniciId) ? kullaniciId : null;
    }
}
