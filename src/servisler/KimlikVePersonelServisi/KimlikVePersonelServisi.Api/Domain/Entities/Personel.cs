using KimlikVePersonelServisi.Api.Domain.Enums;

namespace KimlikVePersonelServisi.Api.Domain.Entities;

public sealed class Personel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid DepartmanId { get; set; }
    public string Unvan { get; set; } = string.Empty;
    public bool DepartmanSorumlusuMu { get; set; }
    public PersonelDurumu Durum { get; set; } = PersonelDurumu.Aktif;
    public DateOnly IseGirisTarihi { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public DateOnly? IstenAyrilisTarihi { get; set; }
    public bool AktifMi { get; set; } = true;
}
