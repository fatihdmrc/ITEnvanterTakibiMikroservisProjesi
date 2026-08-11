using System.ComponentModel.DataAnnotations;

namespace EnvanterTakip.MvcClient.Models;

public enum DenetimKayitTuruModel
{
    Event = 1,
    Crud = 2
}

public sealed class DenetimKaydiModel
{
    public string Id { get; set; } = string.Empty;
    public Guid? EventId { get; set; }
    public DenetimKayitTuruModel KayitTuru { get; set; }
    public string KaynakServis { get; set; } = string.Empty;
    public string? EventAdi { get; set; }
    public string? IslemTuru { get; set; }
    public string? VarlikTuru { get; set; }
    public string? VarlikId { get; set; }
    public string? VarlikAdi { get; set; }
    public Guid? KullaniciId { get; set; }
    public string? Rol { get; set; }
    public string? HttpMetodu { get; set; }
    public string? Endpoint { get; set; }
    public DateTime OlusmaZamaniUtc { get; set; }
    public DateTime AlinmaZamaniUtc { get; set; }
    public string? Aciklama { get; set; }
    public string? Payload { get; set; }
}

public sealed class DenetimListeCevapModel
{
    public IReadOnlyCollection<DenetimKaydiModel> Kayitlar { get; set; } = [];
    public long ToplamKayit { get; set; }
    public int Sayfa { get; set; } = 1;
    public int SayfaBoyutu { get; set; } = 25;
}

public sealed class DenetimFiltreModel
{
    public DenetimKayitTuruModel? KayitTuru { get; set; }
    public string? EventAdi { get; set; }
    public string? IslemTuru { get; set; }
    public string? KaynakServis { get; set; }
    public string? VarlikTuru { get; set; }
    public string? VarlikId { get; set; }
    public Guid? KullaniciId { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? Baslangic { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? Bitis { get; set; }

    public int Sayfa { get; set; } = 1;
    public int SayfaBoyutu { get; set; } = 25;
}

public sealed class DenetimPanelModel
{
    public bool OturumVarMi { get; set; }
    public bool YonetimYetkisiVarMi { get; set; }
    public string? Rol { get; set; }
    public string? HataMesaji { get; set; }
    public DenetimFiltreModel Filtre { get; set; } = new();
    public DenetimListeCevapModel Liste { get; set; } = new();
}
