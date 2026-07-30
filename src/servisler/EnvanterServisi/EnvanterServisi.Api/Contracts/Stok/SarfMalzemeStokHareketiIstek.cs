using EnvanterServisi.Api.Domain.Enums;

namespace EnvanterServisi.Api.Contracts.Stok;

public sealed record SarfMalzemeStokHareketiIstek(
    StokHareketTipi HareketTipi,
    StokHareketNedeni Neden,
    int Miktar,
    string? Aciklama);
