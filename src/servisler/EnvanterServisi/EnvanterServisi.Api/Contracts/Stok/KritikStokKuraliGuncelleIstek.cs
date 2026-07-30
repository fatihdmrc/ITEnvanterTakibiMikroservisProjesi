namespace EnvanterServisi.Api.Contracts.Stok;

public sealed record KritikStokKuraliGuncelleIstek(
    Guid LokasyonId,
    Guid KategoriId,
    string? CihazModeli,
    int KritikStokSeviyesi,
    bool AktifMi);
