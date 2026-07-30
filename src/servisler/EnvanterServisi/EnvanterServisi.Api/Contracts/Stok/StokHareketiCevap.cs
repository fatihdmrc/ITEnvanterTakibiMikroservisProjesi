using EnvanterServisi.Api.Domain.Enums;

namespace EnvanterServisi.Api.Contracts.Stok;

public sealed record StokHareketiCevap(
    Guid Id,
    Guid? CihazId,
    Guid? SarfMalzemeId,
    StokHareketTipi HareketTipi,
    StokHareketNedeni Neden,
    int? Miktar,
    string? Aciklama,
    Guid OlusturanKullaniciId,
    DateTime OlusturulmaTarihi);
