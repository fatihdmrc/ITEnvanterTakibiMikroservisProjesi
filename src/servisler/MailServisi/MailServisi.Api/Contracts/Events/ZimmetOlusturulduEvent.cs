namespace MailServisi.Api.Contracts.Events;

public sealed record ZimmetOlusturulduEvent(
    Guid EventId,
    Guid ZimmetId,
    Guid CihazId,
    Guid PersonelId,
    string PersonelAdSoyad,
    string PersonelEmail,
    string CihazAd,
    string? CihazAssetTag,
    DateOnly ZimmetTarihi,
    Guid ZimmetleyenKullaniciId,
    DateTime OlusmaZamaniUtc);
