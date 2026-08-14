using KimlikVePersonelServisi.Api.Contracts.Personeller;
using KimlikVePersonelServisi.Api.Sabitler;
using KimlikVePersonelServisi.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KimlikVePersonelServisi.Api.Controllers;

[ApiController]
[Route("api/personeller")]
public sealed class PersonellerController(IKimlikPersonelServisi kimlikPersonelServisi) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = KimlikPersonelMesajlari.SadeceAdminPolicy)]
    public async Task<ActionResult<IReadOnlyCollection<PersonelCevap>>> Listele(CancellationToken cancellationToken)
    {
        return Ok(await kimlikPersonelServisi.PersonelleriListeleAsync(cancellationToken));
    }

    [HttpGet("zimmet-secimi")]
    [Authorize(Policy = KimlikPersonelMesajlari.AdminVeyaITPersoneliPolicy)]
    public async Task<ActionResult<IReadOnlyCollection<PersonelCevap>>> ZimmetSecimiListele(CancellationToken cancellationToken)
    {
        return Ok(await kimlikPersonelServisi.PersonelleriListeleAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = KimlikPersonelMesajlari.SadeceAdminPolicy)]
    public async Task<ActionResult<PersonelCevap>> Getir(Guid id, CancellationToken cancellationToken)
    {
        var personel = await kimlikPersonelServisi.PersonelGetirAsync(id, cancellationToken);
        return personel is null
            ? NotFound(new { hata = KimlikPersonelMesajlari.PersonelBulunamadi })
            : Ok(personel);
    }

    [HttpGet("{id:guid}/zimmet-dogrulama")]
    [Authorize(Policy = KimlikPersonelMesajlari.AdminVeyaITPersoneliPolicy)]
    public async Task<ActionResult<PersonelCevap>> ZimmetDogrulamaGetir(Guid id, CancellationToken cancellationToken)
    {
        var personel = await kimlikPersonelServisi.PersonelGetirAsync(id, cancellationToken);
        return personel is null
            ? NotFound(new { hata = KimlikPersonelMesajlari.PersonelBulunamadi })
            : Ok(personel);
    }

    [HttpPost]
    [Authorize(Policy = KimlikPersonelMesajlari.SadeceAdminPolicy)]
    public async Task<ActionResult<PersonelCevap>> Olustur([FromBody] PersonelOlusturIstek istek, CancellationToken cancellationToken)
    {
        var sonuc = await kimlikPersonelServisi.PersonelOlusturAsync(istek, cancellationToken);
        return sonuc.BasariliMi
            ? CreatedAtAction(nameof(Getir), new { id = sonuc.Veri!.Id }, sonuc.Veri)
            : BadRequest(new { hata = sonuc.Hata });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = KimlikPersonelMesajlari.SadeceAdminPolicy)]
    public async Task<ActionResult<PersonelCevap>> Guncelle(Guid id, [FromBody] PersonelGuncelleIstek istek, CancellationToken cancellationToken)
    {
        var sonuc = await kimlikPersonelServisi.PersonelGuncelleAsync(id, istek, cancellationToken);
        return sonuc.BasariliMi
            ? Ok(sonuc.Veri)
            : BadRequest(new { hata = sonuc.Hata });
    }

    [HttpPost("{id:guid}/isten-ayrildi")]
    [Authorize(Policy = KimlikPersonelMesajlari.SadeceAdminPolicy)]
    public async Task<ActionResult<PersonelCevap>> IstenAyrildiYap(Guid id, CancellationToken cancellationToken)
    {
        var sonuc = await kimlikPersonelServisi.PersoneliIstenAyrildiYapAsync(id, cancellationToken);
        return sonuc.BasariliMi
            ? Ok(sonuc.Veri)
            : BadRequest(new { hata = sonuc.Hata });
    }
}
