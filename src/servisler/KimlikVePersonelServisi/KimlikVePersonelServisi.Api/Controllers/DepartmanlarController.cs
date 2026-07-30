using KimlikVePersonelServisi.Api.Contracts.Departmanlar;
using KimlikVePersonelServisi.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KimlikVePersonelServisi.Api.Controllers;

[ApiController]
[Route("api/departmanlar")]
[Authorize(Policy = "AdminVeyaITPersoneli")]
public sealed class DepartmanlarController(IKimlikPersonelServisi kimlikPersonelServisi) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<DepartmanCevap>>> Listele(CancellationToken cancellationToken)
    {
        return Ok(await kimlikPersonelServisi.DepartmanlariListeleAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DepartmanCevap>> Getir(Guid id, CancellationToken cancellationToken)
    {
        var departman = await kimlikPersonelServisi.DepartmanGetirAsync(id, cancellationToken);
        return departman is null
            ? NotFound(new { hata = "Departman bulunamadı." })
            : Ok(departman);
    }

    [HttpPost]
    public async Task<ActionResult<DepartmanCevap>> Olustur([FromBody] DepartmanOlusturIstek istek, CancellationToken cancellationToken)
    {
        var sonuc = await kimlikPersonelServisi.DepartmanOlusturAsync(istek, cancellationToken);
        return sonuc.BasariliMi
            ? CreatedAtAction(nameof(Getir), new { id = sonuc.Veri!.Id }, sonuc.Veri)
            : BadRequest(new { hata = sonuc.Hata });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DepartmanCevap>> Guncelle(Guid id, [FromBody] DepartmanGuncelleIstek istek, CancellationToken cancellationToken)
    {
        var sonuc = await kimlikPersonelServisi.DepartmanGuncelleAsync(id, istek, cancellationToken);
        return sonuc.BasariliMi
            ? Ok(sonuc.Veri)
            : BadRequest(new { hata = sonuc.Hata });
    }
}
