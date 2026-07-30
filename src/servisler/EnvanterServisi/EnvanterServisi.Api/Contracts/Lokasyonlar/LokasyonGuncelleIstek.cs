namespace EnvanterServisi.Api.Contracts.Lokasyonlar;

public sealed record LokasyonGuncelleIstek(
    string Ad,
    Guid? UstLokasyonId,
    bool AktifMi);
