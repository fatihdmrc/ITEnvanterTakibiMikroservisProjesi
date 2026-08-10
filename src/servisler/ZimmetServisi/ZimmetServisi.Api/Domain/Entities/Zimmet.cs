using ZimmetServisi.Api.Domain.Enums;

namespace ZimmetServisi.Api.Domain.Entities;

public sealed class Zimmet : TemelEntity
{
    public Guid CihazId { get; set; }
    public string CihazAd { get; set; } = string.Empty;
    public string? CihazAssetTag { get; set; }
    public string? CihazSeriNumarasi { get; set; }
    public Guid PersonelId { get; set; }
    public string PersonelAdSoyad { get; set; } = string.Empty;
    public string PersonelEmail { get; set; } = string.Empty;
    public DateOnly ZimmetTarihi { get; set; }
    public Guid ZimmetleyenKullaniciId { get; set; }
    public DateOnly? IadeTarihi { get; set; }
    public Guid? IadeAlanKullaniciId { get; set; }
    public IadeKontrolDurumu? IadeKontrolDurumu { get; set; }
    public Guid? IadeKontroluYapanKullaniciId { get; set; }
    public string? IadeNotu { get; set; }
    public ZimmetDurumu Durum { get; set; } = ZimmetDurumu.Aktif;
}
