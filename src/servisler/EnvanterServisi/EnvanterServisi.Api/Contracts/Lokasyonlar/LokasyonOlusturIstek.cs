namespace EnvanterServisi.Api.Contracts.Lokasyonlar;

public sealed record LokasyonOlusturIstek(
    string Ad,
    Guid? UstLokasyonId);
