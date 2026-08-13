using ZimmetServisi.Api.Contracts.Zimmetler;
using ZimmetServisi.Api.Domain.Enums;
using ZimmetServisi.Api.Sabitler;
using ZimmetServisi.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ZimmetServisi.Api.Controllers;

[ApiController]
[Route("api/zimmetler")]
[Authorize]
public sealed class ZimmetlerController(IZimmetServisi zimmetServisi) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "AdminVeyaITPersoneli")]
    public async Task<ActionResult<IReadOnlyCollection<ZimmetCevap>>> Listele(
        [FromQuery] Guid? personelId,
        [FromQuery] Guid? cihazId,
        [FromQuery] ZimmetDurumu? durum,
        CancellationToken cancellationToken)
    {
        return Ok(await zimmetServisi.ZimmetleriListeleAsync(personelId, cihazId, durum, cancellationToken));
    }

    [HttpGet("benim")]
    public async Task<ActionResult<IReadOnlyCollection<ZimmetCevap>>> BenimZimmetlerim(CancellationToken cancellationToken)
    {
        var personelId = PersonelIdGetir();
        if (!personelId.HasValue)
        {
            return Unauthorized(new { hata = ZimmetMesajlari.TokenPersonelIdYok });
        }

        return Ok(await zimmetServisi.ZimmetleriListeleAsync(personelId: personelId.Value, cancellationToken: cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ZimmetCevap>> Getir(Guid id, CancellationToken cancellationToken)
    {
        var zimmet = await zimmetServisi.ZimmetGetirAsync(id, cancellationToken);
        if (zimmet is null)
        {
            return NotFound(new { hata = ZimmetMesajlari.ZimmetKaydiBulunamadi });
        }

        if (!YonetimRolundeMi() && zimmet.PersonelId != PersonelIdGetir())
        {
            return Forbid();
        }

        return Ok(zimmet);
    }

    [HttpPost]
    [Authorize(Policy = "AdminVeyaITPersoneli")]
    public async Task<ActionResult<ZimmetCevap>> Olustur([FromBody] ZimmetOlusturIstek istek, CancellationToken cancellationToken)
    {
        var kullaniciId = KullaniciIdGetir();
        var bearerToken = BearerTokenAl();
        if (!kullaniciId.HasValue || bearerToken is null)
        {
            return Unauthorized(new { hata = ZimmetMesajlari.TokenKullaniciBilgisiYok });
        }

        var sonuc = await zimmetServisi.ZimmetOlusturAsync(istek, kullaniciId.Value, bearerToken, cancellationToken);
        return sonuc.BasariliMi
            ? CreatedAtAction(nameof(Getir), new { id = sonuc.Veri!.Id }, sonuc.Veri)
            : BadRequest(new { hata = sonuc.Hata });
    }

    [HttpPost("{id:guid}/iade-alindi")]
    [Authorize(Policy = "AdminVeyaITPersoneli")]
    public async Task<ActionResult<ZimmetCevap>> IadeAlindi(Guid id, [FromBody] ZimmetIadeAlindiIstek istek, CancellationToken cancellationToken)
    {
        var kullaniciId = KullaniciIdGetir();
        var bearerToken = BearerTokenAl();
        if (!kullaniciId.HasValue || bearerToken is null)
        {
            return Unauthorized(new { hata = ZimmetMesajlari.TokenKullaniciBilgisiYok });
        }

        var sonuc = await zimmetServisi.IadeAlindiAsync(id, istek, kullaniciId.Value, bearerToken, cancellationToken);
        return sonuc.BasariliMi ? Ok(sonuc.Veri) : BadRequest(new { hata = sonuc.Hata });
    }

    [HttpPost("{id:guid}/iade-kontrolu")]
    [Authorize(Policy = "AdminVeyaITPersoneli")]
    public async Task<ActionResult<ZimmetCevap>> IadeKontroluTamamla(Guid id, [FromBody] ZimmetIadeKontroluIstek istek, CancellationToken cancellationToken)
    {
        var kullaniciId = KullaniciIdGetir();
        var bearerToken = BearerTokenAl();
        if (!kullaniciId.HasValue || bearerToken is null)
        {
            return Unauthorized(new { hata = ZimmetMesajlari.TokenKullaniciBilgisiYok });
        }

        var sonuc = await zimmetServisi.IadeKontroluTamamlaAsync(id, istek, kullaniciId.Value, bearerToken, cancellationToken);
        return sonuc.BasariliMi ? Ok(sonuc.Veri) : BadRequest(new { hata = sonuc.Hata });
    }

    private Guid? KullaniciIdGetir()
    {
        var claimDegeri = User.FindFirst("KullaniciId")?.Value;
        return Guid.TryParse(claimDegeri, out var kullaniciId) ? kullaniciId : null;
    }

    private Guid? PersonelIdGetir()
    {
        var claimDegeri = User.FindFirst("PersonelId")?.Value;
        return Guid.TryParse(claimDegeri, out var personelId) ? personelId : null;
    }

    private string? BearerTokenAl()
    {
        var authorization = Request.Headers.Authorization.ToString();
        const string onEk = "Bearer ";
        return authorization.StartsWith(onEk, StringComparison.OrdinalIgnoreCase)
            ? authorization[onEk.Length..].Trim()
            : null;
    }

    private bool YonetimRolundeMi()
        => User.IsInRole(ZimmetMesajlari.AdminRolu) || User.IsInRole(ZimmetMesajlari.ITPersoneliRolu);
}
