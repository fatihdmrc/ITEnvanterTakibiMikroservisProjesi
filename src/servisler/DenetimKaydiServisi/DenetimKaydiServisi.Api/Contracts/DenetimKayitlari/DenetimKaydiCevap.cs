using DenetimKaydiServisi.Api.Domain.Enums;

namespace DenetimKaydiServisi.Api.Contracts.DenetimKayitlari;

public sealed record DenetimKaydiCevap(
    string Id,
    Guid? EventId,
    DenetimKayitTuru KayitTuru,
    string KaynakServis,
    string? EventAdi,
    string? IslemTuru,
    string? VarlikTuru,
    string? VarlikId,
    string? VarlikAdi,
    Guid? KullaniciId,
    string? Rol,
    string? HttpMetodu,
    string? Endpoint,
    DateTime OlusmaZamaniUtc,
    DateTime AlinmaZamaniUtc,
    string? Aciklama,
    string? Payload);
