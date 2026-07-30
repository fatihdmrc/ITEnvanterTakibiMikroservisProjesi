namespace EnvanterServisi.Api.Domain.Entities;

public sealed class KritikStokKurali : TemelEntity
{
    public Guid LokasyonId { get; set; }
    public Guid KategoriId { get; set; }
    public string? CihazModeli { get; set; }
    public int KritikStokSeviyesi { get; set; }
    public bool AktifMi { get; set; } = true;
}
