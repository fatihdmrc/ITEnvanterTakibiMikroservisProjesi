namespace EnvanterServisi.Api.Domain.Entities;

public sealed class Lokasyon : TemelEntity
{
    public string Ad { get; set; } = string.Empty;
    public Guid? UstLokasyonId { get; set; }
    public bool AktifMi { get; set; } = true;
}
