namespace ZimmetServisi.Api.Contracts.Events;

public sealed record CihazHasarliTeslimAlindiEvent(
    Guid EventId,
    Guid ZimmetId,
    Guid CihazId,
    Guid PersonelId,
    string PersonelAdSoyad,
    string? IadeNotu,
    Guid IadeKontroluYapanKullaniciId,
    DateTime OlusmaZamaniUtc);
