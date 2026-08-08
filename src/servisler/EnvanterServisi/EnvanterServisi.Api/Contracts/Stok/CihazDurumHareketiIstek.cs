using EnvanterServisi.Api.Domain.Enums;

namespace EnvanterServisi.Api.Contracts.Stok;

public sealed record CihazDurumHareketiIstek(
    StokHareketNedeni Neden,
    string? Aciklama,
    EldenCikarmaTipi EldenCikarmaTipi,
    string? SatilanKisiVeyaKurum);
