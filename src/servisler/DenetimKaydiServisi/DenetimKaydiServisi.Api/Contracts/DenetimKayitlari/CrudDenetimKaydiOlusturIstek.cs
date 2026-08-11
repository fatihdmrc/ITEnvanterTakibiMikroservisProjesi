namespace DenetimKaydiServisi.Api.Contracts.DenetimKayitlari;

public sealed record CrudDenetimKaydiOlusturIstek(
    string KaynakServis,
    string IslemTuru,
    string? VarlikTuru,
    string? VarlikId,
    string? VarlikAdi,
    Guid? KullaniciId,
    string? Rol,
    string? HttpMetodu,
    string? Endpoint,
    string? Aciklama,
    string? Payload,
    DateTime? OlusmaZamaniUtc);
