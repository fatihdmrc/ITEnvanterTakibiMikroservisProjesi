namespace EnvanterServisi.Api.Contracts.SarfMalzemeler;

public sealed record SarfMalzemeGuncelleIstek(
    string Ad,
    Guid KategoriId,
    Guid LokasyonId,
    int EldekiMiktar,
    int KritikStokSeviyesi,
    string Birim,
    bool AktifMi);
