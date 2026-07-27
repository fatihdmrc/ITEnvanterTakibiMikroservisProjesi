using KimlikVePersonelServisi.Api.Domain.Enums;

namespace KimlikVePersonelServisi.Api.Contracts.Personeller;

public sealed record PersonelCevap(
    Guid Id,
    string Ad,
    string Soyad,
    string Email,
    Guid DepartmanId,
    string Unvan,
    bool DepartmanSorumlusuMu,
    PersonelDurumu Durum,
    DateOnly IseGirisTarihi,
    DateOnly? IstenAyrilisTarihi,
    bool AktifMi);
