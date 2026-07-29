using Microsoft.AspNetCore.Identity;

namespace KimlikVePersonelServisi.Api.Domain.Entities;

public sealed class UygulamaKullanici : IdentityUser<Guid>
{
    public Guid PersonelId { get; set; }
    public bool AktifMi { get; set; } = true;
}
