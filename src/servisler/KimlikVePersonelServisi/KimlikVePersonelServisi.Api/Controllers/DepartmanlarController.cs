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
    public ActionResult<IReadOnlyCollection<DepartmanCevap>> Listele()
    {
        return Ok(kimlikPersonelServisi.DepartmanlariListele());
    }

    [HttpGet("{id:guid}")]
    public ActionResult<DepartmanCevap> Getir(Guid id)
    {
        var departman = kimlikPersonelServisi.DepartmanGetir(id);
        return departman is null
            ? NotFound(new { hata = "Departman bulunamadı." })
            : Ok(departman);
    }

    [HttpPost]
    public ActionResult<DepartmanCevap> Olustur([FromBody] DepartmanOlusturIstek istek)
    {
        var sonuc = kimlikPersonelServisi.DepartmanOlustur(istek);
        return sonuc.BasariliMi
            ? CreatedAtAction(nameof(Getir), new { id = sonuc.Veri!.Id }, sonuc.Veri)
            : BadRequest(new { hata = sonuc.Hata });
    }

    [HttpPut("{id:guid}")]
    public ActionResult<DepartmanCevap> Guncelle(Guid id, [FromBody] DepartmanGuncelleIstek istek)
    {
        var sonuc = kimlikPersonelServisi.DepartmanGuncelle(id, istek);
        return sonuc.BasariliMi
            ? Ok(sonuc.Veri)
            : BadRequest(new { hata = sonuc.Hata });
    }
}
