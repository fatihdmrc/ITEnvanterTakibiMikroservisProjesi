namespace EnvanterServisi.Api.Domain.Entities;

public sealed class SarfMalzeme : TemelEntity
{
    public string Ad { get; set; } = string.Empty;
    public Guid KategoriId { get; set; }
    public Guid LokasyonId { get; set; }
    public int EldekiMiktar { get; set; }
    public int KritikStokSeviyesi { get; set; }
    public string Birim { get; set; } = "Adet";
    public bool AktifMi { get; set; } = true;
}
