using EnvanterServisi.Api.Contracts.Cihazlar;
using EnvanterServisi.Api.Contracts.Kategoriler;
using EnvanterServisi.Api.Contracts.Lokasyonlar;
using EnvanterServisi.Api.Contracts.SarfMalzemeler;
using EnvanterServisi.Api.Contracts.Stok;
using EnvanterServisi.Api.Domain.Enums;

namespace EnvanterServisi.Api.Services;

public interface IEnvanterServisi
{
    Task<IReadOnlyCollection<KategoriCevap>> KategorileriListeleAsync(CancellationToken cancellationToken = default);
    Task<KategoriCevap?> KategoriGetirAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Sonuc<KategoriCevap>> KategoriOlusturAsync(KategoriOlusturIstek istek, CancellationToken cancellationToken = default);
    Task<Sonuc<KategoriCevap>> KategoriGuncelleAsync(Guid id, KategoriGuncelleIstek istek, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<LokasyonCevap>> LokasyonlariListeleAsync(CancellationToken cancellationToken = default);
    Task<LokasyonCevap?> LokasyonGetirAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Sonuc<LokasyonCevap>> LokasyonOlusturAsync(LokasyonOlusturIstek istek, CancellationToken cancellationToken = default);
    Task<Sonuc<LokasyonCevap>> LokasyonGuncelleAsync(Guid id, LokasyonGuncelleIstek istek, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CihazCevap>> CihazlariListeleAsync(Guid? kategoriId = null, Guid? lokasyonId = null, bool? aktifMi = null, CihazDurumu? durum = null, string? arama = null, CancellationToken cancellationToken = default);
    Task<CihazCevap?> CihazGetirAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Sonuc<CihazCevap>> CihazOlusturAsync(CihazOlusturIstek istek, CancellationToken cancellationToken = default);
    Task<Sonuc<CihazCevap>> CihazGuncelleAsync(Guid id, CihazGuncelleIstek istek, CancellationToken cancellationToken = default);
    Task<Sonuc<CihazCevap>> CihazDurumHareketiIsleAsync(Guid id, CihazDurumHareketiIstek istek, Guid olusturanKullaniciId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SarfMalzemeCevap>> SarfMalzemeleriListeleAsync(Guid? kategoriId = null, Guid? lokasyonId = null, string? arama = null, CancellationToken cancellationToken = default);
    Task<SarfMalzemeCevap?> SarfMalzemeGetirAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Sonuc<SarfMalzemeCevap>> SarfMalzemeOlusturAsync(SarfMalzemeOlusturIstek istek, CancellationToken cancellationToken = default);
    Task<Sonuc<SarfMalzemeCevap>> SarfMalzemeGuncelleAsync(Guid id, SarfMalzemeGuncelleIstek istek, CancellationToken cancellationToken = default);
    Task<Sonuc<SarfMalzemeCevap>> SarfMalzemeStokHareketiIsleAsync(Guid id, SarfMalzemeStokHareketiIstek istek, Guid olusturanKullaniciId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<StokHareketiCevap>> StokHareketleriniListeleAsync(Guid? cihazId = null, Guid? sarfMalzemeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<KritikStokKuraliCevap>> KritikStokKurallariniListeleAsync(CancellationToken cancellationToken = default);
    Task<Sonuc<KritikStokKuraliCevap>> KritikStokKuraliOlusturAsync(KritikStokKuraliOlusturIstek istek, CancellationToken cancellationToken = default);
    Task<Sonuc<KritikStokKuraliCevap>> KritikStokKuraliGuncelleAsync(Guid id, KritikStokKuraliGuncelleIstek istek, CancellationToken cancellationToken = default);
    Task<StokOzetCevap> StokOzetiniGetirAsync(CancellationToken cancellationToken = default);
}
