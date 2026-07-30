namespace EnvanterServisi.Api.Contracts.SarfMalzemeler;

public sealed record SarfMalzemeCevap(
    Guid Id,
    string Ad,
    Guid KategoriId,
    Guid LokasyonId,
    int EldekiMiktar,
    int KritikStokSeviyesi,
    string Birim,
    bool AktifMi);
