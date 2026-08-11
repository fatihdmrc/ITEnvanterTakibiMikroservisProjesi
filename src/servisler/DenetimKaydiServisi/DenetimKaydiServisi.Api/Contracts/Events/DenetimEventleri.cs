using System.Text.Json;

namespace DenetimKaydiServisi.Api.Contracts.Events;

public sealed record PersonelIstenAyrildiEvent(
    Guid EventId,
    Guid PersonelId,
    string AdSoyad,
    string Email,
    Guid DepartmanId,
    DateOnly IstenAyrilisTarihi,
    DateTime OlusmaZamaniUtc);

public sealed record CihazDurumuDegistiEvent(
    Guid EventId,
    Guid CihazId,
    string? AssetTag,
    string? SeriNumarasi,
    string OncekiDurum,
    string YeniDurum,
    bool AktifMi,
    bool ToplamVarligaDahilMi,
    string Neden,
    Guid OlusturanKullaniciId,
    DateTime OlusmaZamaniUtc);

public sealed record KritikStokSeviyesineDusulduEvent(
    Guid EventId,
    string VarlikTuru,
    Guid KategoriId,
    Guid LokasyonId,
    string? CihazModeli,
    Guid? SarfMalzemeId,
    string? SarfMalzemeAdi,
    int MevcutMiktar,
    int KritikStokSeviyesi,
    DateTime OlusmaZamaniUtc);

public sealed record ZimmetOlusturulduEvent(
    Guid EventId,
    Guid ZimmetId,
    Guid CihazId,
    Guid PersonelId,
    string PersonelAdSoyad,
    string CihazAd,
    string? CihazAssetTag,
    DateOnly ZimmetTarihi,
    Guid ZimmetleyenKullaniciId,
    DateTime OlusmaZamaniUtc);

public sealed record ZimmetIadeAlindiEvent(
    Guid EventId,
    Guid ZimmetId,
    Guid CihazId,
    Guid PersonelId,
    string PersonelAdSoyad,
    DateOnly IadeTarihi,
    Guid IadeAlanKullaniciId,
    string? IadeNotu,
    DateTime OlusmaZamaniUtc);

public sealed record ZimmetIadeEdildiEvent(
    Guid EventId,
    Guid ZimmetId,
    Guid CihazId,
    Guid PersonelId,
    string PersonelAdSoyad,
    JsonElement IadeKontrolDurumu,
    Guid IadeKontroluYapanKullaniciId,
    string? IadeNotu,
    DateTime OlusmaZamaniUtc);

public sealed record CihazKontroleAlindiEvent(
    Guid EventId,
    Guid ZimmetId,
    Guid CihazId,
    Guid PersonelId,
    string PersonelAdSoyad,
    DateOnly IadeTarihi,
    Guid IadeAlanKullaniciId,
    DateTime OlusmaZamaniUtc);

public sealed record CihazHasarliTeslimAlindiEvent(
    Guid EventId,
    Guid ZimmetId,
    Guid CihazId,
    Guid PersonelId,
    string PersonelAdSoyad,
    string? IadeNotu,
    Guid IadeKontroluYapanKullaniciId,
    DateTime OlusmaZamaniUtc);
