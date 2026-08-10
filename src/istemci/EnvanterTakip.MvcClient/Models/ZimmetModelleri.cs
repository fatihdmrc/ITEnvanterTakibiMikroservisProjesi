using System.ComponentModel.DataAnnotations;

namespace EnvanterTakip.MvcClient.Models;

public sealed class ZimmetPanelModel
{
    public IReadOnlyCollection<ZimmetModel> Zimmetler { get; set; } = [];
    public IReadOnlyCollection<PersonelModel> AktifPersoneller { get; set; } = [];
    public IReadOnlyCollection<CihazModel> KullanilabilirCihazlar { get; set; } = [];
    public List<string> ListelemeHatalari { get; set; } = [];
    public ZimmetOlusturFormModel OlusturFormu { get; set; } = new();
    public bool OturumVarMi { get; set; }
    public bool YonetimYetkisiVarMi { get; set; }
    public string? Rol { get; set; }
    public string? BasariMesaji { get; set; }
    public string? HataMesaji { get; set; }
}

public sealed class ZimmetIadeAlSayfaModel
{
    public ZimmetModel Zimmet { get; set; } = null!;
    public ZimmetIadeAlindiFormModel Form { get; set; } = new();
    public string? HataMesaji { get; set; }
}

public sealed class ZimmetIadeKontrolSayfaModel
{
    public ZimmetModel Zimmet { get; set; } = null!;
    public ZimmetIadeKontroluFormModel Form { get; set; } = new();
    public string? HataMesaji { get; set; }
}

public sealed class ZimmetOlusturFormModel
{
    [Required(ErrorMessage = "Cihaz seçimi zorunludur.")]
    public Guid CihazId { get; set; }

    [Required(ErrorMessage = "Personel seçimi zorunludur.")]
    public Guid PersonelId { get; set; }

    public DateOnly? ZimmetTarihi { get; set; } = DateOnly.FromDateTime(DateTime.Today);
}

public sealed class ZimmetIadeAlindiFormModel
{
    public Guid Id { get; set; }
    public DateOnly? IadeTarihi { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public string? IadeNotu { get; set; }
}

public sealed class ZimmetIadeKontroluFormModel
{
    public Guid Id { get; set; }
    public ZimmetIadeKontrolDurumuModel IadeKontrolDurumu { get; set; } = ZimmetIadeKontrolDurumuModel.Saglam;
    public string? IadeNotu { get; set; }
}

public sealed record ZimmetModel(
    Guid Id,
    Guid CihazId,
    string CihazAd,
    string? CihazAssetTag,
    string? CihazSeriNumarasi,
    Guid PersonelId,
    string PersonelAdSoyad,
    string PersonelEmail,
    DateOnly ZimmetTarihi,
    Guid ZimmetleyenKullaniciId,
    DateOnly? IadeTarihi,
    Guid? IadeAlanKullaniciId,
    ZimmetDurumuModel Durum,
    ZimmetIadeKontrolDurumuModel? IadeKontrolDurumu,
    Guid? IadeKontroluYapanKullaniciId,
    string? IadeNotu,
    DateTime OlusturulmaTarihi,
    DateTime? GuncellenmeTarihi);

public enum ZimmetDurumuModel
{
    Aktif = 1,
    IadeSurecinde = 2,
    IadeEdildi = 3
}

public enum ZimmetIadeKontrolDurumuModel
{
    Saglam = 1,
    Bakimda = 2,
    HurdaIskarta = 3,
    HasarliTeslimAlindi = 4
}
