namespace EnvanterServisi.Api.Contracts.Lokasyonlar;

public sealed record LokasyonCevap(
    Guid Id,
    string Ad,
    Guid? UstLokasyonId,
    bool AktifMi);
