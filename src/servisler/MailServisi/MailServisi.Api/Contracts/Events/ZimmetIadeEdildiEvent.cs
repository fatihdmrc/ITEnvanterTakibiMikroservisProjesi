namespace MailServisi.Api.Contracts.Events;

public sealed record ZimmetIadeEdildiEvent(
    Guid EventId,
    Guid ZimmetId,
    Guid CihazId,
    Guid PersonelId,
    string PersonelAdSoyad,
    string PersonelEmail,
    string CihazAd,
    string? CihazAssetTag,
    string IadeKontrolDurumu,
    Guid IadeKontroluYapanKullaniciId,
    string? IadeNotu,
    DateTime OlusmaZamaniUtc);
