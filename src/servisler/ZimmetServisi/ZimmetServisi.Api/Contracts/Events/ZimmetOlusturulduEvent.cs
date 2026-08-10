namespace ZimmetServisi.Api.Contracts.Events;

public sealed record ZimmetOlusturulduEvent(
    Guid EventId,
    Guid ZimmetId,
    Guid CihazId,
    Guid PersonelId,
    string PersonelAdSoyad,
    string CihazAd,
    string? CihazAssetTag,
    DateOnly ZimmetTarihi,
    Guid ZimmetleyenKullaniciId,
    DateTime OlusmaZamaniUtc);
