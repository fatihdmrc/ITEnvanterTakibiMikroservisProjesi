using KimlikVePersonelServisi.Api.Contracts.Personeller;
using KimlikVePersonelServisi.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KimlikVePersonelServisi.Api.Controllers;

[ApiController]
[Route("api/personeller")]
[Authorize(Policy = "AdminVeyaITPersoneli")]
public sealed class PersonellerController(IKimlikPersonelServisi kimlikPersonelServisi) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyCollection<PersonelCevap>> Listele()
    {
        return Ok(kimlikPersonelServisi.PersonelleriListele());
    }

    [HttpGet("{id:guid}")]
    public ActionResult<PersonelCevap> Getir(Guid id)
    {
        var personel = kimlikPersonelServisi.PersonelGetir(id);
        return personel is null
            ? NotFound(new { hata = "Personel bulunamadı." })
            : Ok(personel);
    }

    [HttpPost]
    public ActionResult<PersonelCevap> Olustur([FromBody] PersonelOlusturIstek istek)
    {
        var sonuc = kimlikPersonelServisi.PersonelOlustur(istek);
        return sonuc.BasariliMi
            ? CreatedAtAction(nameof(Getir), new { id = sonuc.Veri!.Id }, sonuc.Veri)
            : BadRequest(new { hata = sonuc.Hata });
    }

    [HttpPut("{id:guid}")]
    public ActionResult<PersonelCevap> Guncelle(Guid id, [FromBody] PersonelGuncelleIstek istek)
    {
        var sonuc = kimlikPersonelServisi.PersonelGuncelle(id, istek);
        return sonuc.BasariliMi
            ? Ok(sonuc.Veri)
            : BadRequest(new { hata = sonuc.Hata });
    }

    [HttpPost("{id:guid}/isten-ayrildi")]
    public ActionResult<PersonelCevap> IstenAyrildiYap(Guid id)
    {
        var sonuc = kimlikPersonelServisi.PersoneliIstenAyrildiYap(id);
        return sonuc.BasariliMi
            ? Ok(sonuc.Veri)
            : BadRequest(new { hata = sonuc.Hata });
    }
}
