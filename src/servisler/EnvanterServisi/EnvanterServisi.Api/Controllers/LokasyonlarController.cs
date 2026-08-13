using EnvanterServisi.Api.Contracts.Lokasyonlar;
using EnvanterServisi.Api.Sabitler;
using EnvanterServisi.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnvanterServisi.Api.Controllers;

[ApiController]
[Route("api/lokasyonlar")]
[Authorize(Policy = "AdminVeyaITPersoneli")]
public sealed class LokasyonlarController(IEnvanterServisi envanterServisi) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<LokasyonCevap>>> Listele(CancellationToken cancellationToken)
        => Ok(await envanterServisi.LokasyonlariListeleAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LokasyonCevap>> Getir(Guid id, CancellationToken cancellationToken)
    {
        var lokasyon = await envanterServisi.LokasyonGetirAsync(id, cancellationToken);
        return lokasyon is null ? NotFound(new { hata = EnvanterMesajlari.LokasyonBulunamadi }) : Ok(lokasyon);
    }

    [HttpPost]
    public async Task<ActionResult<LokasyonCevap>> Olustur([FromBody] LokasyonOlusturIstek istek, CancellationToken cancellationToken)
    {
        var sonuc = await envanterServisi.LokasyonOlusturAsync(istek, cancellationToken);
        return sonuc.BasariliMi
            ? CreatedAtAction(nameof(Getir), new { id = sonuc.Veri!.Id }, sonuc.Veri)
            : BadRequest(new { hata = sonuc.Hata });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<LokasyonCevap>> Guncelle(Guid id, [FromBody] LokasyonGuncelleIstek istek, CancellationToken cancellationToken)
    {
        var sonuc = await envanterServisi.LokasyonGuncelleAsync(id, istek, cancellationToken);
        return sonuc.BasariliMi ? Ok(sonuc.Veri) : BadRequest(new { hata = sonuc.Hata });
    }
}
