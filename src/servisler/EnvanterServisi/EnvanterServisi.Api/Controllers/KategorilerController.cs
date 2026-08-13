using EnvanterServisi.Api.Contracts.Kategoriler;
using EnvanterServisi.Api.Sabitler;
using EnvanterServisi.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnvanterServisi.Api.Controllers;

[ApiController]
[Route("api/kategoriler")]
[Authorize(Policy = "AdminVeyaITPersoneli")]
public sealed class KategorilerController(IEnvanterServisi envanterServisi) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<KategoriCevap>>> Listele(CancellationToken cancellationToken)
        => Ok(await envanterServisi.KategorileriListeleAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<KategoriCevap>> Getir(Guid id, CancellationToken cancellationToken)
    {
        var kategori = await envanterServisi.KategoriGetirAsync(id, cancellationToken);
        return kategori is null ? NotFound(new { hata = EnvanterMesajlari.KategoriBulunamadi }) : Ok(kategori);
    }

    [HttpPost]
    public async Task<ActionResult<KategoriCevap>> Olustur([FromBody] KategoriOlusturIstek istek, CancellationToken cancellationToken)
    {
        var sonuc = await envanterServisi.KategoriOlusturAsync(istek, cancellationToken);
        return sonuc.BasariliMi
            ? CreatedAtAction(nameof(Getir), new { id = sonuc.Veri!.Id }, sonuc.Veri)
            : BadRequest(new { hata = sonuc.Hata });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<KategoriCevap>> Guncelle(Guid id, [FromBody] KategoriGuncelleIstek istek, CancellationToken cancellationToken)
    {
        var sonuc = await envanterServisi.KategoriGuncelleAsync(id, istek, cancellationToken);
        return sonuc.BasariliMi ? Ok(sonuc.Veri) : BadRequest(new { hata = sonuc.Hata });
    }
}
