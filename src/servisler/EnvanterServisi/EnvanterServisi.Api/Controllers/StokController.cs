using EnvanterServisi.Api.Contracts.Stok;
using EnvanterServisi.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnvanterServisi.Api.Controllers;

[ApiController]
[Route("api/stok")]
[Authorize(Policy = "AdminVeyaITPersoneli")]
public sealed class StokController(IEnvanterServisi envanterServisi) : ControllerBase
{
    [HttpGet("ozet")]
    public async Task<ActionResult<StokOzetCevap>> Ozet(CancellationToken cancellationToken)
        => Ok(await envanterServisi.StokOzetiniGetirAsync(cancellationToken));

    [HttpGet("hareketler")]
    public async Task<ActionResult<IReadOnlyCollection<StokHareketiCevap>>> HareketleriListele(
        [FromQuery] Guid? cihazId,
        [FromQuery] Guid? sarfMalzemeId,
        CancellationToken cancellationToken)
    {
        return Ok(await envanterServisi.StokHareketleriniListeleAsync(cihazId, sarfMalzemeId, cancellationToken));
    }

    [HttpGet("kritik-kurallar")]
    public async Task<ActionResult<IReadOnlyCollection<KritikStokKuraliCevap>>> KritikKurallariListele(CancellationToken cancellationToken)
        => Ok(await envanterServisi.KritikStokKurallariniListeleAsync(cancellationToken));

    [HttpPost("kritik-kurallar")]
    public async Task<ActionResult<KritikStokKuraliCevap>> KritikKuralOlustur([FromBody] KritikStokKuraliOlusturIstek istek, CancellationToken cancellationToken)
    {
        var sonuc = await envanterServisi.KritikStokKuraliOlusturAsync(istek, cancellationToken);
        return sonuc.BasariliMi
            ? Created($"/api/stok/kritik-kurallar/{sonuc.Veri!.Id}", sonuc.Veri)
            : BadRequest(new { hata = sonuc.Hata });
    }

    [HttpPut("kritik-kurallar/{id:guid}")]
    public async Task<ActionResult<KritikStokKuraliCevap>> KritikKuralGuncelle(Guid id, [FromBody] KritikStokKuraliGuncelleIstek istek, CancellationToken cancellationToken)
    {
        var sonuc = await envanterServisi.KritikStokKuraliGuncelleAsync(id, istek, cancellationToken);
        return sonuc.BasariliMi ? Ok(sonuc.Veri) : BadRequest(new { hata = sonuc.Hata });
    }
}
