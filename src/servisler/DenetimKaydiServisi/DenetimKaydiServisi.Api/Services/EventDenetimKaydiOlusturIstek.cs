namespace DenetimKaydiServisi.Api.Services;

public sealed record EventDenetimKaydiOlusturIstek(
    Guid EventId,
    string EventAdi,
    string KaynakServis,
    string? VarlikTuru,
    string? VarlikId,
    string? VarlikAdi,
    Guid? KullaniciId,
    string? Aciklama,
    DateTime OlusmaZamaniUtc,
    object Payload);
