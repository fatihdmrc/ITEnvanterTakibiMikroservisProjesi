namespace ZimmetServisi.Api.Services.Harici;

public sealed record HariciPersonelCevap(
    Guid Id,
    string Ad,
    string Soyad,
    string Email,
    Guid DepartmanId,
    string Unvan,
    bool DepartmanSorumlusuMu,
    HariciPersonelDurumu Durum,
    DateOnly IseGirisTarihi,
    DateOnly? IstenAyrilisTarihi,
    bool AktifMi);

public enum HariciPersonelDurumu
{
    Aktif = 1,
    Pasif = 2,
    IstenAyrildi = 3
}

public sealed record HariciCihazCevap(
    Guid Id,
    string? SeriNumarasi,
    string? AssetTag,
    string Ad,
    string Marka,
    string Model,
    Guid KategoriId,
    Guid LokasyonId,
    HariciCihazDurumu Durum,
    DateOnly EnvantereGirisTarihi,
    DateOnly? EnvanterdenCikisTarihi,
    HariciEldenCikarmaTipi EldenCikarmaTipi,
    string? EldenCikarmaAciklamasi,
    string? SatilanKisiVeyaKurum,
    bool AktifMi,
    bool ToplamVarligaDahilMi);

public sealed record HariciCihazDurumHareketiIstek(
    HariciStokHareketNedeni Neden,
    string? Aciklama,
    HariciEldenCikarmaTipi EldenCikarmaTipi,
    string? SatilanKisiVeyaKurum);

public enum HariciCihazDurumu
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

public enum HariciStokHareketNedeni
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

public enum HariciEldenCikarmaTipi
{
    Yok = 0,
    Atildi = 1,
    Satildi = 2,
    Bagislandi = 3,
    Diger = 4
}
