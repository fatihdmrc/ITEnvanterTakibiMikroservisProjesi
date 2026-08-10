namespace ZimmetServisi.Api.Contracts.Events;

public sealed record ZimmetIadeAlindiEvent(
    Guid EventId,
    Guid ZimmetId,
    Guid CihazId,
    Guid PersonelId,
    string PersonelAdSoyad,
    DateOnly IadeTarihi,
    Guid IadeAlanKullaniciId,
    string? IadeNotu,
    DateTime OlusmaZamaniUtc);
