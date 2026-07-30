namespace EnvanterServisi.Api.Contracts.Stok;

public sealed record KritikStokKuraliOlusturIstek(
    Guid LokasyonId,
    Guid KategoriId,
    string? CihazModeli,
    int KritikStokSeviyesi);
