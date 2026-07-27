using KimlikVePersonelServisi.Api.Domain.Enums;

namespace KimlikVePersonelServisi.Api.Contracts.Kullanicilar;

public sealed record KullaniciCevap(
    Guid Id,
    string KullaniciAdi,
    KullaniciRolu Rol,
    Guid PersonelId,
    bool AktifMi);
