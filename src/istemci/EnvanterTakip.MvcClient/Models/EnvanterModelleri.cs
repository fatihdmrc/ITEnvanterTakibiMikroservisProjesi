using System.ComponentModel.DataAnnotations;

namespace EnvanterTakip.MvcClient.Models;

public sealed class EnvanterPanelModel
{
    public IReadOnlyCollection<KategoriModel> Kategoriler { get; set; } = [];
    public IReadOnlyCollection<LokasyonModel> Lokasyonlar { get; set; } = [];
    public IReadOnlyCollection<CihazModel> Cihazlar { get; set; } = [];
    public IReadOnlyCollection<SarfMalzemeModel> SarfMalzemeler { get; set; } = [];
    public StokOzetModel StokOzet { get; set; } = new(0, 0, 0, []);
    public CihazFiltreModel CihazFiltre { get; set; } = new();
    public List<string> ListelemeHatalari { get; set; } = [];
    public string AktifSekme { get; set; } = "stok";
    public string? BasariMesaji { get; set; }
    public string? HataMesaji { get; set; }
    public bool OturumVarMi { get; set; }
}

public sealed class CihazIslemleriSayfaModel
{
    public CihazGuncelleFormModel Form { get; set; } = new();
    public CihazDurumHareketiFormModel DurumHareketi { get; set; } = new();
    public IReadOnlyCollection<StokHareketiModel> DurumHareketleri { get; set; } = [];
    public IReadOnlyCollection<KategoriModel> Kategoriler { get; set; } = [];
    public IReadOnlyCollection<LokasyonModel> Lokasyonlar { get; set; } = [];
    public string? BasariMesaji { get; set; }
    public string? HataMesaji { get; set; }
}

public sealed class SarfMalzemeIslemleriSayfaModel
{
    public SarfMalzemeGuncelleFormModel Form { get; set; } = new();
    public SarfMalzemeStokHareketiFormModel StokHareketi { get; set; } = new();
    public IReadOnlyCollection<StokHareketiModel> StokHareketleri { get; set; } = [];
    public IReadOnlyCollection<KategoriModel> Kategoriler { get; set; } = [];
    public IReadOnlyCollection<LokasyonModel> Lokasyonlar { get; set; } = [];
    public string? BasariMesaji { get; set; }
    public string? HataMesaji { get; set; }
}

public sealed class CihazFiltreModel
{
    public Guid? KategoriId { get; set; }
    public Guid? LokasyonId { get; set; }
    public bool? AktifMi { get; set; }
}

public class KategoriOlusturFormModel
{
    [Required(ErrorMessage = "Kategori adı zorunludur.")]
    public string Ad { get; set; } = string.Empty;

    public Guid? UstKategoriId { get; set; }
    public VarlikTuruModel VarlikTuru { get; set; } = VarlikTuruModel.SeriNumarali;
    public int? KritikStokSeviyesi { get; set; }
}

public sealed class KategoriGuncelleFormModel : KategoriOlusturFormModel
{
    public Guid Id { get; set; }
    public bool AktifMi { get; set; } = true;
}

public class LokasyonOlusturFormModel
{
    [Required(ErrorMessage = "Lokasyon adı zorunludur.")]
    public string Ad { get; set; } = string.Empty;

    public Guid? UstLokasyonId { get; set; }
}

public sealed class LokasyonGuncelleFormModel : LokasyonOlusturFormModel
{
    public Guid Id { get; set; }
    public bool AktifMi { get; set; } = true;
}

public class CihazOlusturFormModel
{
    public string? SeriNumarasi { get; set; }

    [Required(ErrorMessage = "Cihaz adı zorunludur.")]
    public string Ad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Marka zorunludur.")]
    public string Marka { get; set; } = string.Empty;

    [Required(ErrorMessage = "Model zorunludur.")]
    public string Model { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kategori zorunludur.")]
    public Guid KategoriId { get; set; }

    [Required(ErrorMessage = "Lokasyon zorunludur.")]
    public Guid LokasyonId { get; set; }

    public DateOnly EnvantereGirisTarihi { get; set; } = DateOnly.FromDateTime(DateTime.Today);
}

public sealed class CihazGuncelleFormModel : CihazOlusturFormModel
{
    public Guid Id { get; set; }
    public string? AssetTag { get; set; }
    public CihazDurumuModel Durum { get; set; } = CihazDurumuModel.Kullanilabilir;
    public DateOnly? EnvanterdenCikisTarihi { get; set; }
    public EldenCikarmaTipiModel EldenCikarmaTipi { get; set; } = EldenCikarmaTipiModel.Yok;
    public string? EldenCikarmaAciklamasi { get; set; }
    public string? SatilanKisiVeyaKurum { get; set; }
    public bool AktifMi { get; set; } = true;
    public bool ToplamVarligaDahilMi { get; set; } = true;
}

public class SarfMalzemeOlusturFormModel
{
    [Required(ErrorMessage = "Sarf malzeme adı zorunludur.")]
    public string Ad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kategori zorunludur.")]
    public Guid KategoriId { get; set; }

    [Required(ErrorMessage = "Lokasyon zorunludur.")]
    public Guid LokasyonId { get; set; }

    public int EldekiMiktar { get; set; }
    public int KritikStokSeviyesi { get; set; }
    public string Birim { get; set; } = "Adet";
}

public sealed class SarfMalzemeGuncelleFormModel : SarfMalzemeOlusturFormModel
{
    public Guid Id { get; set; }
    public bool AktifMi { get; set; } = true;
}

public sealed class CihazDurumHareketiFormModel
{
    public Guid Id { get; set; }
    public StokHareketNedeniModel Neden { get; set; } = StokHareketNedeniModel.ManuelStokCikisi;
    public string? Aciklama { get; set; }
    public EldenCikarmaTipiModel EldenCikarmaTipi { get; set; } = EldenCikarmaTipiModel.Diger;
    public string? SatilanKisiVeyaKurum { get; set; }
}

public sealed class SarfMalzemeStokHareketiFormModel
{
    public Guid Id { get; set; }
    public StokHareketTipiModel HareketTipi { get; set; } = StokHareketTipiModel.Cikis;
    public StokHareketNedeniModel Neden { get; set; } = StokHareketNedeniModel.ManuelStokCikisi;
    public int Miktar { get; set; } = 1;
    public string? Aciklama { get; set; }
}

public sealed record KategoriModel(
    Guid Id,
    string Ad,
    Guid? UstKategoriId,
    VarlikTuruModel VarlikTuru,
    int? KritikStokSeviyesi,
    bool AktifMi);

public sealed record LokasyonModel(
    Guid Id,
    string Ad,
    Guid? UstLokasyonId,
    bool AktifMi);

public sealed record CihazModel(
    Guid Id,
    string? SeriNumarasi,
    string? AssetTag,
    string Ad,
    string Marka,
    string Model,
    Guid KategoriId,
    Guid LokasyonId,
    CihazDurumuModel Durum,
    DateOnly EnvantereGirisTarihi,
    DateOnly? EnvanterdenCikisTarihi,
    EldenCikarmaTipiModel EldenCikarmaTipi,
    string? EldenCikarmaAciklamasi,
    string? SatilanKisiVeyaKurum,
    bool AktifMi,
    bool ToplamVarligaDahilMi);

public sealed record SarfMalzemeModel(
    Guid Id,
    string Ad,
    Guid KategoriId,
    Guid LokasyonId,
    int EldekiMiktar,
    int KritikStokSeviyesi,
    string Birim,
    bool AktifMi);

public sealed record StokOzetModel(
    int ToplamVarlik,
    int KullanilabilirCihazStoku,
    int SarfMalzemeToplamMiktari,
    IReadOnlyCollection<KritikStokModel> KritikStoklar);

public sealed record KritikStokModel(
    string VarlikTuru,
    Guid KategoriId,
    Guid LokasyonId,
    string? Model,
    int MevcutMiktar,
    int KritikStokSeviyesi);

public sealed record StokHareketiModel(
    Guid Id,
    Guid? CihazId,
    Guid? SarfMalzemeId,
    StokHareketTipiModel HareketTipi,
    StokHareketNedeniModel Neden,
    int? Miktar,
    string? Aciklama,
    Guid OlusturanKullaniciId,
    DateTime OlusturulmaTarihi);

public enum VarlikTuruModel
{
    SeriNumarali = 1,
    SarfMalzeme = 2
}

public enum CihazDurumuModel
{
    Kullanilabilir = 1,
    Zimmetli = 2,
    Incelemede = 3,
    Bakimda = 4,
    HasarliTeslimAlindi = 5,
    Kayip = 6,
    Calindi = 7,
    HurdaIskarta = 8,
    KullanimDisi = 9
}

public enum StokHareketTipiModel
{
    Giris = 1,
    Cikis = 2,
    Duzeltme = 3
}

public enum StokHareketNedeniModel
{
    EnvantereGiris = 1,
    ManuelStokCikisi = 2,
    FizikselSayimDuzeltmesi = 3,
    Ariza = 4,
    Calinma = 5,
    Kaybolma = 6,
    HurdaIskarta = 7,
    KullanimOmruBitti = 8,
    BakimdanDondu = 9,
    IncelemeyeAlindi = 10,
    HasarliTeslimAlindi = 11,
    Zimmetlendi = 12,
    ZimmetIadeAlindi = 13
}

public enum EldenCikarmaTipiModel
{
    Yok = 0,
    Atildi = 1,
    Satildi = 2,
    Bagislandi = 3,
    Diger = 4
}
