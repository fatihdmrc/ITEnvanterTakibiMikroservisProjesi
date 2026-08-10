using ZimmetServisi.Api.Domain.Enums;

namespace ZimmetServisi.Api.Contracts.Events;

public sealed record ZimmetIadeEdildiEvent(
    Guid EventId,
    Guid ZimmetId,
    Guid CihazId,
    Guid PersonelId,
    string PersonelAdSoyad,
    IadeKontrolDurumu IadeKontrolDurumu,
    Guid IadeKontroluYapanKullaniciId,
    string? IadeNotu,
    DateTime OlusmaZamaniUtc);
