namespace ZimmetServisi.Api.Contracts.Events;

public sealed record CihazKontroleAlindiEvent(
    Guid EventId,
    Guid ZimmetId,
    Guid CihazId,
    Guid PersonelId,
    string PersonelAdSoyad,
    DateOnly IadeTarihi,
    Guid IadeAlanKullaniciId,
    DateTime OlusmaZamaniUtc);
