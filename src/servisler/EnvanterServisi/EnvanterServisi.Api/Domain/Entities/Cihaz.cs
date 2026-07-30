using EnvanterServisi.Api.Domain.Enums;

namespace EnvanterServisi.Api.Domain.Entities;

public sealed class Cihaz : TemelEntity
{
    public string? SeriNumarasi { get; set; }
    public string? AssetTag { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Marka { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public Guid KategoriId { get; set; }
    public Guid LokasyonId { get; set; }
    public CihazDurumu Durum { get; set; } = CihazDurumu.Kullanilabilir;
    public DateOnly EnvantereGirisTarihi { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public DateOnly? EnvanterdenCikisTarihi { get; set; }
    public EldenCikarmaTipi EldenCikarmaTipi { get; set; } = EldenCikarmaTipi.Yok;
    public string? EldenCikarmaAciklamasi { get; set; }
    public string? SatilanKisiVeyaKurum { get; set; }
    public bool AktifMi { get; set; } = true;
    public bool ToplamVarligaDahilMi { get; set; } = true;
}
