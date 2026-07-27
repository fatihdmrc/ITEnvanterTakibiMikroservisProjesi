using KimlikVePersonelServisi.Api.Domain.Enums;

namespace KimlikVePersonelServisi.Api.Contracts.Personeller;

public sealed record PersonelGuncelleIstek(
    string Ad,
    string Soyad,
    string Email,
    Guid DepartmanId,
    string Unvan,
    bool DepartmanSorumlusuMu,
    PersonelDurumu Durum,
    bool AktifMi);
