using KimlikVePersonelServisi.Api.Domain.Enums;

namespace KimlikVePersonelServisi.Api.Contracts.Kullanicilar;

public sealed record KullaniciOlusturIstek(
    string KullaniciAdi,
    string Sifre,
    KullaniciRolu Rol,
    Guid PersonelId);
