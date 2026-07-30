using EnvanterServisi.Api.Domain.Enums;

namespace EnvanterServisi.Api.Domain.Entities;

public sealed class StokHareketi : TemelEntity
{
    public Guid? CihazId { get; set; }
    public Guid? SarfMalzemeId { get; set; }
    public StokHareketTipi HareketTipi { get; set; }
    public StokHareketNedeni Neden { get; set; }
    public int? Miktar { get; set; }
    public string? Aciklama { get; set; }
    public Guid OlusturanKullaniciId { get; set; }
}
