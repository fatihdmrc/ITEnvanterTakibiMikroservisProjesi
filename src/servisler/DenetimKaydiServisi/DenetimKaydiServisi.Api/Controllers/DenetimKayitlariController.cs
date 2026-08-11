using DenetimKaydiServisi.Api.Contracts.DenetimKayitlari;
using DenetimKaydiServisi.Api.Domain.Enums;
using DenetimKaydiServisi.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DenetimKaydiServisi.Api.Controllers;

[ApiController]
[Route("api/denetim-kayitlari")]
[Authorize(Policy = "AdminVeyaITPersoneli")]
public sealed class DenetimKayitlariController(IDenetimKaydiServisi denetimKaydiServisi) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DenetimKaydiListeCevap>> Listele(
        [FromQuery] DenetimKayitTuru? kayitTuru,
        [FromQuery] string? eventAdi,
        [FromQuery] string? islemTuru,
        [FromQuery] string? kaynakServis,
        [FromQuery] string? varlikTuru,
        [FromQuery] string? varlikId,
        [FromQuery] Guid? kullaniciId,
        [FromQuery] DateTime? baslangic,
        [FromQuery] DateTime? bitis,
        [FromQuery] int sayfa = 1,
        [FromQuery] int sayfaBoyutu = 25,
        CancellationToken cancellationToken = default)
    {
        var filtre = new DenetimKaydiFiltre(
            kayitTuru,
            eventAdi,
            islemTuru,
            kaynakServis,
            varlikTuru,
            varlikId,
            kullaniciId,
            baslangic,
            bitis,
            sayfa,
            sayfaBoyutu);

        return Ok(await denetimKaydiServisi.ListeleAsync(filtre, cancellationToken));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DenetimKaydiCevap>> Getir(string id, CancellationToken cancellationToken)
    {
        var kayit = await denetimKaydiServisi.GetirAsync(id, cancellationToken);
        return kayit is null ? NotFound(new { hata = "Denetim kaydi bulunamadi." }) : Ok(kayit);
    }

    [HttpPost("crud")]
    public async Task<ActionResult<DenetimKaydiCevap>> CrudKaydiOlustur([FromBody] CrudDenetimKaydiOlusturIstek istek, CancellationToken cancellationToken)
    {
        var kullaniciId = istek.KullaniciId ?? KullaniciIdGetir();
        var rol = string.IsNullOrWhiteSpace(istek.Rol) ? RolGetir() : istek.Rol;
        var normalIstek = istek with { KullaniciId = kullaniciId, Rol = rol };

        var kayit = await denetimKaydiServisi.CrudKaydiOlusturAsync(normalIstek, cancellationToken);
        return CreatedAtAction(nameof(Getir), new { id = kayit.Id }, kayit);
    }

    private Guid? KullaniciIdGetir()
    {
        var claimDegeri = User.FindFirst("KullaniciId")?.Value;
        return Guid.TryParse(claimDegeri, out var kullaniciId) ? kullaniciId : null;
    }

    private string? RolGetir()
        => User.FindFirst("Rol")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
}
