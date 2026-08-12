namespace MailServisi.Api.Contracts.Events;

public sealed record ZimmetIadeAlindiEvent(
    Guid EventId,
    Guid ZimmetId,
    Guid CihazId,
    Guid PersonelId,
    string PersonelAdSoyad,
    string PersonelEmail,
    string CihazAd,
    string? CihazAssetTag,
    DateOnly IadeTarihi,
    Guid IadeAlanKullaniciId,
    string? IadeNotu,
    DateTime OlusmaZamaniUtc);
