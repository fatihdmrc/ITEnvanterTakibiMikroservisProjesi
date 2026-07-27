using KimlikVePersonelServisi.Api.Domain.Enums;

namespace KimlikVePersonelServisi.Api.Domain.Entities;

public sealed class Kullanici
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string KullaniciAdi { get; set; } = string.Empty;
    public string SifreHash { get; set; } = string.Empty;
    public KullaniciRolu Rol { get; set; }
    public Guid PersonelId { get; set; }
    public bool AktifMi { get; set; } = true;
}
