using ZimmetServisi.Api.Domain.Enums;

namespace ZimmetServisi.Api.Contracts.Zimmetler;

public sealed record ZimmetCevap(
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
    ZimmetDurumu Durum,
    IadeKontrolDurumu? IadeKontrolDurumu,
    Guid? IadeKontroluYapanKullaniciId,
    string? IadeNotu,
    DateTime OlusturulmaTarihi,
    DateTime? GuncellenmeTarihi);
