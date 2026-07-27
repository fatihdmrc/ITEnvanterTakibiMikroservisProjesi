using KimlikVePersonelServisi.Api.Domain.Enums;

namespace KimlikVePersonelServisi.Api.Contracts.Kimlik;

public sealed record GirisCevap(
    string Token,
    Guid KullaniciId,
    Guid PersonelId,
    KullaniciRolu Rol,
    DateTimeOffset GecerlilikZamani);
