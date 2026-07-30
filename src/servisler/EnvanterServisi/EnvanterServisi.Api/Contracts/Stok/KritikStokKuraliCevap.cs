namespace EnvanterServisi.Api.Contracts.Stok;

public sealed record KritikStokKuraliCevap(
    Guid Id,
    Guid LokasyonId,
    Guid KategoriId,
    string? CihazModeli,
    int KritikStokSeviyesi,
    bool AktifMi);
