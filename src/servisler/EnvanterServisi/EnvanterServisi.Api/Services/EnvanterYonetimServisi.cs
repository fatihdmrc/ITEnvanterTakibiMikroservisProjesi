using EnvanterServisi.Api.Contracts.Cihazlar;
using EnvanterServisi.Api.Contracts.Events;
using EnvanterServisi.Api.Contracts.Kategoriler;
using EnvanterServisi.Api.Contracts.Lokasyonlar;
using EnvanterServisi.Api.Contracts.SarfMalzemeler;
using EnvanterServisi.Api.Contracts.Stok;
using EnvanterServisi.Api.Data;
using EnvanterServisi.Api.Domain.Entities;
using EnvanterServisi.Api.Domain.Enums;
using EnvanterServisi.Api.Options;
using EnvanterServisi.Api.Repositories;
using EnvanterServisi.Api.Sabitler;
using EnvanterServisi.Api.Services.Cache;
using DotNetCore.CAP;
using Microsoft.Extensions.Options;

namespace EnvanterServisi.Api.Services;

public sealed class EnvanterYonetimServisi(
    EnvanterDbContext dbContext,
    IKategoriRepository kategoriRepository,
    ILokasyonRepository lokasyonRepository,
    ICihazRepository cihazRepository,
    ISarfMalzemeRepository sarfMalzemeRepository,
    IKritikStokKuraliRepository kritikStokKuraliRepository,
    IStokHareketiRepository stokHareketiRepository,
    IReferansVeriCacheServisi referansVeriCacheServisi,
    IOptions<CacheAyarlari> cacheAyarlari,
    ICapPublisher capPublisher) : IEnvanterServisi
{
    // Servis katmanı, HTTP detayından bağımsız olarak envanter iş kurallarını uygular.
    public async Task<IReadOnlyCollection<KategoriCevap>> KategorileriListeleAsync(CancellationToken cancellationToken = default)
    {
        return await referansVeriCacheServisi.GetOrSetAsync(
            ReferansVeriCacheAnahtarlari.Kategoriler,
            async token =>
            {
                var kategoriler = await kategoriRepository.ListeleAsync(token);
                return kategoriler.Select(KategoriCevabaDonustur).ToList();
            },
            ReferansVeriCacheSuresi(),
            cancellationToken);
    }

    public async Task<KategoriCevap?> KategoriGetirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var kategori = await kategoriRepository.GetirAsync(id, cancellationToken);
        return kategori is null ? null : KategoriCevabaDonustur(kategori);
    }

    public async Task<Sonuc<KategoriCevap>> KategoriOlusturAsync(KategoriOlusturIstek istek, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(istek.Ad))
        {
            return Sonuc<KategoriCevap>.Basarisiz(EnvanterMesajlari.KategoriAdiZorunlu);
        }

        if (istek.UstKategoriId.HasValue && !await kategoriRepository.VarMiAsync(istek.UstKategoriId.Value, cancellationToken))
        {
            return Sonuc<KategoriCevap>.Basarisiz(EnvanterMesajlari.UstKategoriBulunamadi);
        }

        var ad = istek.Ad.Trim();
        if (await kategoriRepository.AdKullaniliyorMuAsync(ad, istek.UstKategoriId, cancellationToken: cancellationToken))
        {
            return Sonuc<KategoriCevap>.Basarisiz(EnvanterMesajlari.KategoriAdiKullaniliyor);
        }

        var kategori = new Kategori
        {
            Ad = ad,
            UstKategoriId = istek.UstKategoriId,
            VarlikTuru = istek.VarlikTuru,
            KritikStokSeviyesi = istek.KritikStokSeviyesi
        };

        kategoriRepository.Ekle(kategori);
        await kategoriRepository.KaydetAsync(cancellationToken);
        await referansVeriCacheServisi.SilAsync(ReferansVeriCacheAnahtarlari.Kategoriler, cancellationToken);

        return Sonuc<KategoriCevap>.Basarili(KategoriCevabaDonustur(kategori));
    }

    public async Task<Sonuc<KategoriCevap>> KategoriGuncelleAsync(Guid id, KategoriGuncelleIstek istek, CancellationToken cancellationToken = default)
    {
        var kategori = await kategoriRepository.GetirAsync(id, cancellationToken);
        if (kategori is null)
        {
            return Sonuc<KategoriCevap>.Basarisiz(EnvanterMesajlari.KategoriBulunamadi);
        }

        if (id == istek.UstKategoriId)
        {
            return Sonuc<KategoriCevap>.Basarisiz(EnvanterMesajlari.KategoriKendiUstuOlamaz);
        }

        if (istek.UstKategoriId.HasValue && !await kategoriRepository.VarMiAsync(istek.UstKategoriId.Value, cancellationToken))
        {
            return Sonuc<KategoriCevap>.Basarisiz(EnvanterMesajlari.UstKategoriBulunamadi);
        }

        var ad = istek.Ad.Trim();
        if (string.IsNullOrWhiteSpace(ad))
        {
            return Sonuc<KategoriCevap>.Basarisiz(EnvanterMesajlari.KategoriAdiZorunlu);
        }

        if (await kategoriRepository.AdKullaniliyorMuAsync(ad, istek.UstKategoriId, id, cancellationToken))
        {
            return Sonuc<KategoriCevap>.Basarisiz(EnvanterMesajlari.KategoriAdiKullaniliyor);
        }

        kategori.Ad = ad;
        kategori.UstKategoriId = istek.UstKategoriId;
        kategori.VarlikTuru = istek.VarlikTuru;
        kategori.KritikStokSeviyesi = istek.KritikStokSeviyesi;
        kategori.AktifMi = istek.AktifMi;

        await kategoriRepository.KaydetAsync(cancellationToken);
        await referansVeriCacheServisi.SilAsync(ReferansVeriCacheAnahtarlari.Kategoriler, cancellationToken);
        return Sonuc<KategoriCevap>.Basarili(KategoriCevabaDonustur(kategori));
    }

    public async Task<IReadOnlyCollection<LokasyonCevap>> LokasyonlariListeleAsync(CancellationToken cancellationToken = default)
    {
        return await referansVeriCacheServisi.GetOrSetAsync(
            ReferansVeriCacheAnahtarlari.Lokasyonlar,
            async token =>
            {
                var lokasyonlar = await lokasyonRepository.ListeleAsync(token);
                return lokasyonlar.Select(LokasyonCevabaDonustur).ToList();
            },
            ReferansVeriCacheSuresi(),
            cancellationToken);
    }

    public async Task<LokasyonCevap?> LokasyonGetirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lokasyon = await lokasyonRepository.GetirAsync(id, cancellationToken);
        return lokasyon is null ? null : LokasyonCevabaDonustur(lokasyon);
    }

    public async Task<Sonuc<LokasyonCevap>> LokasyonOlusturAsync(LokasyonOlusturIstek istek, CancellationToken cancellationToken = default)
    {
        var ad = istek.Ad.Trim();
        if (string.IsNullOrWhiteSpace(ad))
        {
            return Sonuc<LokasyonCevap>.Basarisiz(EnvanterMesajlari.LokasyonAdiZorunlu);
        }

        if (istek.UstLokasyonId.HasValue && !await lokasyonRepository.VarMiAsync(istek.UstLokasyonId.Value, cancellationToken))
        {
            return Sonuc<LokasyonCevap>.Basarisiz(EnvanterMesajlari.UstLokasyonBulunamadi);
        }

        if (await lokasyonRepository.AdKullaniliyorMuAsync(ad, istek.UstLokasyonId, cancellationToken: cancellationToken))
        {
            return Sonuc<LokasyonCevap>.Basarisiz(EnvanterMesajlari.LokasyonAdiKullaniliyor);
        }

        var lokasyon = new Lokasyon
        {
            Ad = ad,
            UstLokasyonId = istek.UstLokasyonId
        };

        lokasyonRepository.Ekle(lokasyon);
        await lokasyonRepository.KaydetAsync(cancellationToken);
        await referansVeriCacheServisi.SilAsync(ReferansVeriCacheAnahtarlari.Lokasyonlar, cancellationToken);

        return Sonuc<LokasyonCevap>.Basarili(LokasyonCevabaDonustur(lokasyon));
    }

    public async Task<Sonuc<LokasyonCevap>> LokasyonGuncelleAsync(Guid id, LokasyonGuncelleIstek istek, CancellationToken cancellationToken = default)
    {
        var lokasyon = await lokasyonRepository.GetirAsync(id, cancellationToken);
        if (lokasyon is null)
        {
            return Sonuc<LokasyonCevap>.Basarisiz(EnvanterMesajlari.LokasyonBulunamadi);
        }

        if (id == istek.UstLokasyonId)
        {
            return Sonuc<LokasyonCevap>.Basarisiz(EnvanterMesajlari.LokasyonKendiUstuOlamaz);
        }

        if (istek.UstLokasyonId.HasValue && !await lokasyonRepository.VarMiAsync(istek.UstLokasyonId.Value, cancellationToken))
        {
            return Sonuc<LokasyonCevap>.Basarisiz(EnvanterMesajlari.UstLokasyonBulunamadi);
        }

        var ad = istek.Ad.Trim();
        if (string.IsNullOrWhiteSpace(ad))
        {
            return Sonuc<LokasyonCevap>.Basarisiz(EnvanterMesajlari.LokasyonAdiZorunlu);
        }

        if (await lokasyonRepository.AdKullaniliyorMuAsync(ad, istek.UstLokasyonId, id, cancellationToken))
        {
            return Sonuc<LokasyonCevap>.Basarisiz(EnvanterMesajlari.LokasyonAdiKullaniliyor);
        }

        lokasyon.Ad = ad;
        lokasyon.UstLokasyonId = istek.UstLokasyonId;
        lokasyon.AktifMi = istek.AktifMi;

        await lokasyonRepository.KaydetAsync(cancellationToken);
        await referansVeriCacheServisi.SilAsync(ReferansVeriCacheAnahtarlari.Lokasyonlar, cancellationToken);
        return Sonuc<LokasyonCevap>.Basarili(LokasyonCevabaDonustur(lokasyon));
    }

    public async Task<IReadOnlyCollection<CihazCevap>> CihazlariListeleAsync(Guid? kategoriId = null, Guid? lokasyonId = null, bool? aktifMi = null, CihazDurumu? durum = null, string? arama = null, CancellationToken cancellationToken = default)
    {
        var cihazlar = await cihazRepository.FiltreleAsync(kategoriId, lokasyonId, aktifMi, durum, arama, cancellationToken);
        return cihazlar.Select(CihazCevabaDonustur).ToList();
    }

    public async Task<CihazCevap?> CihazGetirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cihaz = await cihazRepository.GetirAsync(id, cancellationToken);
        return cihaz is null ? null : CihazCevabaDonustur(cihaz);
    }

    public async Task<Sonuc<CihazCevap>> CihazOlusturAsync(CihazOlusturIstek istek, CancellationToken cancellationToken = default)
    {
        var assetTag = BosIseNull(istek.AssetTag) ?? await SiradakiAssetTagUretAsync(cancellationToken);

        var kimlikKontrolu = await CihazKimlikBilgisiniDogrulaAsync(istek.SeriNumarasi, assetTag, null, cancellationToken);
        if (!kimlikKontrolu.BasariliMi)
        {
            return Sonuc<CihazCevap>.Basarisiz(kimlikKontrolu.Hata!);
        }

        var referansKontrolu = await CihazReferanslariniDogrulaAsync(istek.KategoriId, istek.LokasyonId, cancellationToken);
        if (!referansKontrolu.BasariliMi)
        {
            return Sonuc<CihazCevap>.Basarisiz(referansKontrolu.Hata!);
        }

        var cihaz = new Cihaz
        {
            SeriNumarasi = BosIseNull(istek.SeriNumarasi),
            AssetTag = assetTag,
            Ad = istek.Ad.Trim(),
            Marka = istek.Marka.Trim(),
            Model = istek.Model.Trim(),
            KategoriId = istek.KategoriId,
            LokasyonId = istek.LokasyonId,
            EnvantereGirisTarihi = istek.EnvantereGirisTarihi
        };

        CihazKapsamAlanlariniDurumaGoreGuncelle(cihaz);

        cihazRepository.Ekle(cihaz);
        await cihazRepository.KaydetAsync(cancellationToken);

        return Sonuc<CihazCevap>.Basarili(CihazCevabaDonustur(cihaz));
    }

    public async Task<Sonuc<CihazCevap>> CihazGuncelleAsync(Guid id, CihazGuncelleIstek istek, CancellationToken cancellationToken = default)
    {
        var cihaz = await cihazRepository.GetirAsync(id, cancellationToken);
        if (cihaz is null)
        {
            return Sonuc<CihazCevap>.Basarisiz(EnvanterMesajlari.CihazBulunamadi);
        }

        var assetTag = BosIseNull(istek.AssetTag) ?? cihaz.AssetTag ?? await SiradakiAssetTagUretAsync(cancellationToken);

        var kimlikKontrolu = await CihazKimlikBilgisiniDogrulaAsync(istek.SeriNumarasi, assetTag, id, cancellationToken);
        if (!kimlikKontrolu.BasariliMi)
        {
            return Sonuc<CihazCevap>.Basarisiz(kimlikKontrolu.Hata!);
        }

        var referansKontrolu = await CihazReferanslariniDogrulaAsync(istek.KategoriId, istek.LokasyonId, cancellationToken);
        if (!referansKontrolu.BasariliMi)
        {
            return Sonuc<CihazCevap>.Basarisiz(referansKontrolu.Hata!);
        }

        cihaz.SeriNumarasi = BosIseNull(istek.SeriNumarasi);
        cihaz.AssetTag = assetTag;
        cihaz.Ad = istek.Ad.Trim();
        cihaz.Marka = istek.Marka.Trim();
        cihaz.Model = istek.Model.Trim();
        cihaz.KategoriId = istek.KategoriId;
        cihaz.LokasyonId = istek.LokasyonId;
        cihaz.EnvantereGirisTarihi = istek.EnvantereGirisTarihi;
        CihazKapsamAlanlariniDurumaGoreGuncelle(cihaz);

        await cihazRepository.KaydetAsync(cancellationToken);
        return Sonuc<CihazCevap>.Basarili(CihazCevabaDonustur(cihaz));
    }

    public async Task<Sonuc<CihazCevap>> CihazDurumHareketiIsleAsync(Guid id, CihazDurumHareketiIstek istek, Guid olusturanKullaniciId, CancellationToken cancellationToken = default)
    {
        var cihaz = await cihazRepository.GetirAsync(id, cancellationToken);
        if (cihaz is null)
        {
            return Sonuc<CihazCevap>.Basarisiz(EnvanterMesajlari.CihazBulunamadi);
        }

        var oncekiDurum = cihaz.Durum;
        var durumSonucu = CihazDurumunuHareketeGoreGuncelle(cihaz, istek);
        if (!durumSonucu.BasariliMi)
        {
            return Sonuc<CihazCevap>.Basarisiz(durumSonucu.Hata!);
        }

        using var transaction = dbContext.Database.BeginTransaction(capPublisher, autoCommit: false);

        stokHareketiRepository.Ekle(new StokHareketi
        {
            CihazId = cihaz.Id,
            HareketTipi = CihazDurumHareketTipiniBelirle(istek),
            Neden = istek.Neden,
            Aciklama = BosIseNull(istek.Aciklama),
            OlusturanKullaniciId = olusturanKullaniciId
        });

        await cihazRepository.KaydetAsync(cancellationToken);
        await capPublisher.PublishAsync(
            EventAdlari.CihazDurumuDegisti,
            new CihazDurumuDegistiEvent(
                Guid.NewGuid(),
                cihaz.Id,
                cihaz.AssetTag,
                cihaz.SeriNumarasi,
                oncekiDurum.ToString(),
                cihaz.Durum.ToString(),
                cihaz.AktifMi,
                cihaz.ToplamVarligaDahilMi,
                istek.Neden.ToString(),
                olusturanKullaniciId,
                DateTime.UtcNow),
            cancellationToken: cancellationToken);

        await CihazKritikStokEventleriniYayinlaAsync(cihaz, cancellationToken);

        transaction.Commit();
        return Sonuc<CihazCevap>.Basarili(CihazCevabaDonustur(cihaz));
    }

    public async Task<IReadOnlyCollection<SarfMalzemeCevap>> SarfMalzemeleriListeleAsync(Guid? kategoriId = null, Guid? lokasyonId = null, string? arama = null, CancellationToken cancellationToken = default)
    {
        var sarfMalzemeler = await sarfMalzemeRepository.FiltreleAsync(kategoriId, lokasyonId, arama, cancellationToken);
        return sarfMalzemeler.Select(SarfMalzemeCevabaDonustur).ToList();
    }

    public async Task<SarfMalzemeCevap?> SarfMalzemeGetirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sarfMalzeme = await sarfMalzemeRepository.GetirAsync(id, cancellationToken);
        return sarfMalzeme is null ? null : SarfMalzemeCevabaDonustur(sarfMalzeme);
    }

    public async Task<Sonuc<SarfMalzemeCevap>> SarfMalzemeOlusturAsync(SarfMalzemeOlusturIstek istek, CancellationToken cancellationToken = default)
    {
        var sonuc = await SarfMalzemeReferanslariniDogrulaAsync(istek.KategoriId, istek.LokasyonId, cancellationToken);
        if (!sonuc.BasariliMi)
        {
            return Sonuc<SarfMalzemeCevap>.Basarisiz(sonuc.Hata!);
        }

        var ad = istek.Ad.Trim();
        if (string.IsNullOrWhiteSpace(ad))
        {
            return Sonuc<SarfMalzemeCevap>.Basarisiz(EnvanterMesajlari.SarfMalzemeAdiZorunlu);
        }

        if (istek.EldekiMiktar < 0 || istek.KritikStokSeviyesi < 0)
        {
            return Sonuc<SarfMalzemeCevap>.Basarisiz(EnvanterMesajlari.MiktarVeKritikStokNegatifOlamaz);
        }

        if (await sarfMalzemeRepository.AdKullaniliyorMuAsync(ad, istek.KategoriId, istek.LokasyonId, cancellationToken: cancellationToken))
        {
            return Sonuc<SarfMalzemeCevap>.Basarisiz(EnvanterMesajlari.SarfMalzemeAdiKullaniliyor);
        }

        var sarfMalzeme = new SarfMalzeme
        {
            Ad = ad,
            KategoriId = istek.KategoriId,
            LokasyonId = istek.LokasyonId,
            EldekiMiktar = istek.EldekiMiktar,
            KritikStokSeviyesi = istek.KritikStokSeviyesi,
            Birim = string.IsNullOrWhiteSpace(istek.Birim) ? EnvanterMesajlari.VarsayilanBirim : istek.Birim.Trim()
        };

        sarfMalzemeRepository.Ekle(sarfMalzeme);
        await sarfMalzemeRepository.KaydetAsync(cancellationToken);

        return Sonuc<SarfMalzemeCevap>.Basarili(SarfMalzemeCevabaDonustur(sarfMalzeme));
    }

    public async Task<Sonuc<SarfMalzemeCevap>> SarfMalzemeGuncelleAsync(Guid id, SarfMalzemeGuncelleIstek istek, CancellationToken cancellationToken = default)
    {
        var sarfMalzeme = await sarfMalzemeRepository.GetirAsync(id, cancellationToken);
        if (sarfMalzeme is null)
        {
            return Sonuc<SarfMalzemeCevap>.Basarisiz(EnvanterMesajlari.SarfMalzemeBulunamadi);
        }

        var sonuc = await SarfMalzemeReferanslariniDogrulaAsync(istek.KategoriId, istek.LokasyonId, cancellationToken);
        if (!sonuc.BasariliMi)
        {
            return Sonuc<SarfMalzemeCevap>.Basarisiz(sonuc.Hata!);
        }

        var ad = istek.Ad.Trim();
        if (string.IsNullOrWhiteSpace(ad))
        {
            return Sonuc<SarfMalzemeCevap>.Basarisiz(EnvanterMesajlari.SarfMalzemeAdiZorunlu);
        }

        if (istek.EldekiMiktar < 0 || istek.KritikStokSeviyesi < 0)
        {
            return Sonuc<SarfMalzemeCevap>.Basarisiz(EnvanterMesajlari.MiktarVeKritikStokNegatifOlamaz);
        }

        if (await sarfMalzemeRepository.AdKullaniliyorMuAsync(ad, istek.KategoriId, istek.LokasyonId, id, cancellationToken))
        {
            return Sonuc<SarfMalzemeCevap>.Basarisiz(EnvanterMesajlari.SarfMalzemeAdiKullaniliyor);
        }

        sarfMalzeme.Ad = ad;
        sarfMalzeme.KategoriId = istek.KategoriId;
        sarfMalzeme.LokasyonId = istek.LokasyonId;
        sarfMalzeme.EldekiMiktar = istek.EldekiMiktar;
        sarfMalzeme.KritikStokSeviyesi = istek.KritikStokSeviyesi;
        sarfMalzeme.Birim = string.IsNullOrWhiteSpace(istek.Birim) ? EnvanterMesajlari.VarsayilanBirim : istek.Birim.Trim();
        sarfMalzeme.AktifMi = istek.AktifMi;

        await sarfMalzemeRepository.KaydetAsync(cancellationToken);
        return Sonuc<SarfMalzemeCevap>.Basarili(SarfMalzemeCevabaDonustur(sarfMalzeme));
    }

    public async Task<Sonuc<SarfMalzemeCevap>> SarfMalzemeStokHareketiIsleAsync(Guid id, SarfMalzemeStokHareketiIstek istek, Guid olusturanKullaniciId, CancellationToken cancellationToken = default)
    {
        var sarfMalzeme = await sarfMalzemeRepository.GetirAsync(id, cancellationToken);
        if (sarfMalzeme is null)
        {
            return Sonuc<SarfMalzemeCevap>.Basarisiz(EnvanterMesajlari.SarfMalzemeBulunamadi);
        }

        if (!SarfMalzemeStokHareketNedeniMi(istek.Neden))
        {
            return Sonuc<SarfMalzemeCevap>.Basarisiz(EnvanterMesajlari.SarfStokHareketiDesteklenmiyor);
        }

        if (istek.Miktar <= 0)
        {
            return Sonuc<SarfMalzemeCevap>.Basarisiz(EnvanterMesajlari.StokHareketMiktariPozitifOlmali);
        }

        if (istek.HareketTipi == StokHareketTipi.Cikis && sarfMalzeme.EldekiMiktar < istek.Miktar)
        {
            return Sonuc<SarfMalzemeCevap>.Basarisiz(EnvanterMesajlari.EldekiMiktardanFazlaCikisYapilamaz);
        }

        using var transaction = dbContext.Database.BeginTransaction(capPublisher, autoCommit: false);

        sarfMalzeme.EldekiMiktar = istek.HareketTipi switch
        {
            StokHareketTipi.Giris => sarfMalzeme.EldekiMiktar + istek.Miktar,
            StokHareketTipi.Cikis => sarfMalzeme.EldekiMiktar - istek.Miktar,
            StokHareketTipi.Duzeltme => istek.Miktar,
            _ => sarfMalzeme.EldekiMiktar
        };

        stokHareketiRepository.Ekle(new StokHareketi
        {
            SarfMalzemeId = sarfMalzeme.Id,
            HareketTipi = istek.HareketTipi,
            Neden = istek.Neden,
            Miktar = istek.Miktar,
            Aciklama = BosIseNull(istek.Aciklama),
            OlusturanKullaniciId = olusturanKullaniciId
        });

        await sarfMalzemeRepository.KaydetAsync(cancellationToken);
        await SarfMalzemeKritikStokEventiYayinlaAsync(sarfMalzeme, cancellationToken);

        transaction.Commit();
        return Sonuc<SarfMalzemeCevap>.Basarili(SarfMalzemeCevabaDonustur(sarfMalzeme));
    }

    public async Task<IReadOnlyCollection<StokHareketiCevap>> StokHareketleriniListeleAsync(Guid? cihazId = null, Guid? sarfMalzemeId = null, CancellationToken cancellationToken = default)
    {
        var hareketler = await stokHareketiRepository.FiltreleAsync(cihazId, sarfMalzemeId, cancellationToken);
        return hareketler.Select(StokHareketiCevabaDonustur).ToList();
    }

    public async Task<IReadOnlyCollection<KritikStokKuraliCevap>> KritikStokKurallariniListeleAsync(CancellationToken cancellationToken = default)
    {
        var kurallar = await kritikStokKuraliRepository.ListeleAsync(cancellationToken);
        return kurallar.Select(KritikStokKuraliCevabaDonustur).ToList();
    }

    public async Task<Sonuc<KritikStokKuraliCevap>> KritikStokKuraliOlusturAsync(KritikStokKuraliOlusturIstek istek, CancellationToken cancellationToken = default)
    {
        var sonuc = await KritikStokKuraliDogrulaAsync(istek.LokasyonId, istek.KategoriId, istek.KritikStokSeviyesi, cancellationToken);
        if (!sonuc.BasariliMi)
        {
            return Sonuc<KritikStokKuraliCevap>.Basarisiz(sonuc.Hata!);
        }

        var cihazModeli = BosIseNull(istek.CihazModeli);
        if (await KritikStokKuraliKullaniliyorMuAsync(istek.LokasyonId, istek.KategoriId, cihazModeli, null, cancellationToken))
        {
            return Sonuc<KritikStokKuraliCevap>.Basarisiz(EnvanterMesajlari.KritikStokKuraliZatenVar);
        }

        var kural = new KritikStokKurali
        {
            LokasyonId = istek.LokasyonId,
            KategoriId = istek.KategoriId,
            CihazModeli = cihazModeli,
            KritikStokSeviyesi = istek.KritikStokSeviyesi
        };

        kritikStokKuraliRepository.Ekle(kural);
        await kritikStokKuraliRepository.KaydetAsync(cancellationToken);

        return Sonuc<KritikStokKuraliCevap>.Basarili(KritikStokKuraliCevabaDonustur(kural));
    }

    public async Task<Sonuc<KritikStokKuraliCevap>> KritikStokKuraliGuncelleAsync(Guid id, KritikStokKuraliGuncelleIstek istek, CancellationToken cancellationToken = default)
    {
        var kural = await kritikStokKuraliRepository.GetirAsync(id, cancellationToken);
        if (kural is null)
        {
            return Sonuc<KritikStokKuraliCevap>.Basarisiz(EnvanterMesajlari.KritikStokKuraliBulunamadi);
        }

        var sonuc = istek.AktifMi
            ? await KritikStokKuraliDogrulaAsync(istek.LokasyonId, istek.KategoriId, istek.KritikStokSeviyesi, cancellationToken)
            : await PasifKritikStokKuraliDogrulaAsync(istek.LokasyonId, istek.KategoriId, istek.KritikStokSeviyesi, cancellationToken);
        if (!sonuc.BasariliMi)
        {
            return Sonuc<KritikStokKuraliCevap>.Basarisiz(sonuc.Hata!);
        }

        var cihazModeli = BosIseNull(istek.CihazModeli);
        if (istek.AktifMi && await KritikStokKuraliKullaniliyorMuAsync(istek.LokasyonId, istek.KategoriId, cihazModeli, id, cancellationToken))
        {
            return Sonuc<KritikStokKuraliCevap>.Basarisiz(EnvanterMesajlari.KritikStokKuraliZatenVar);
        }

        kural.LokasyonId = istek.LokasyonId;
        kural.KategoriId = istek.KategoriId;
        kural.CihazModeli = cihazModeli;
        kural.KritikStokSeviyesi = istek.KritikStokSeviyesi;
        kural.AktifMi = istek.AktifMi;

        await kritikStokKuraliRepository.KaydetAsync(cancellationToken);
        return Sonuc<KritikStokKuraliCevap>.Basarili(KritikStokKuraliCevabaDonustur(kural));
    }

    public async Task<StokOzetCevap> StokOzetiniGetirAsync(CancellationToken cancellationToken = default)
    {
        var toplamVarlik = await cihazRepository.ToplamVarlikSayisiAsync(cancellationToken);
        var kullanilabilirCihazStoku = await cihazRepository.KullanilabilirStokSayisiAsync(cancellationToken: cancellationToken);
        var sarfToplam = await sarfMalzemeRepository.ToplamMiktarAsync(cancellationToken);
        var kritikStoklar = new List<KritikStokCevap>();

        var seriKategoriIdleri = (await kategoriRepository.ListeleAsync(cancellationToken))
            .Where(kategori => kategori.AktifMi && kategori.VarlikTuru == VarlikTuru.SeriNumarali)
            .Select(kategori => kategori.Id)
            .ToHashSet();

        var kurallar = (await kritikStokKuraliRepository.ListeleAsync(cancellationToken))
            .Where(kural => kural.AktifMi && seriKategoriIdleri.Contains(kural.KategoriId))
            .ToList();

        foreach (var kural in kurallar)
        {
            var mevcut = await cihazRepository.KullanilabilirStokSayisiAsync(
                kural.KategoriId,
                kural.LokasyonId,
                kural.CihazModeli,
                cancellationToken);

            if (mevcut < kural.KritikStokSeviyesi)
            {
                kritikStoklar.Add(new KritikStokCevap("SeriNumarali", kural.KategoriId, kural.LokasyonId, kural.CihazModeli, mevcut, kural.KritikStokSeviyesi));
            }
        }

        var sarfMalzemeler = await sarfMalzemeRepository.ListeleAsync(cancellationToken);
        foreach (var sarfMalzeme in sarfMalzemeler.Where(sarfMalzeme => sarfMalzeme.AktifMi && sarfMalzeme.EldekiMiktar < sarfMalzeme.KritikStokSeviyesi))
        {
            kritikStoklar.Add(new KritikStokCevap("SarfMalzeme", sarfMalzeme.KategoriId, sarfMalzeme.LokasyonId, null, sarfMalzeme.EldekiMiktar, sarfMalzeme.KritikStokSeviyesi));
        }

        return new StokOzetCevap(toplamVarlik, kullanilabilirCihazStoku, sarfToplam, kritikStoklar);
    }

    private async Task CihazKritikStokEventleriniYayinlaAsync(Cihaz cihaz, CancellationToken cancellationToken)
    {
        var kurallar = (await kritikStokKuraliRepository.ListeleAsync(cancellationToken))
            .Where(kural =>
                kural.AktifMi
                && kural.KategoriId == cihaz.KategoriId
                && kural.LokasyonId == cihaz.LokasyonId
                && (string.IsNullOrWhiteSpace(kural.CihazModeli)
                    || string.Equals(kural.CihazModeli, cihaz.Model, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        foreach (var kural in kurallar)
        {
            var mevcut = await cihazRepository.KullanilabilirStokSayisiAsync(
                kural.KategoriId,
                kural.LokasyonId,
                kural.CihazModeli,
                cancellationToken);

            if (mevcut >= kural.KritikStokSeviyesi)
            {
                continue;
            }

            await capPublisher.PublishAsync(
                EventAdlari.KritikStokSeviyesineDusuldu,
                new KritikStokSeviyesineDusulduEvent(
                    Guid.NewGuid(),
                    "SeriNumarali",
                    kural.KategoriId,
                    kural.LokasyonId,
                    kural.CihazModeli,
                    null,
                    null,
                    mevcut,
                    kural.KritikStokSeviyesi,
                    DateTime.UtcNow),
                cancellationToken: cancellationToken);
        }
    }

    private Task SarfMalzemeKritikStokEventiYayinlaAsync(SarfMalzeme sarfMalzeme, CancellationToken cancellationToken)
    {
        if (!sarfMalzeme.AktifMi || sarfMalzeme.EldekiMiktar >= sarfMalzeme.KritikStokSeviyesi)
        {
            return Task.CompletedTask;
        }

        return capPublisher.PublishAsync(
            EventAdlari.KritikStokSeviyesineDusuldu,
            new KritikStokSeviyesineDusulduEvent(
                Guid.NewGuid(),
                "SarfMalzeme",
                sarfMalzeme.KategoriId,
                sarfMalzeme.LokasyonId,
                null,
                sarfMalzeme.Id,
                sarfMalzeme.Ad,
                sarfMalzeme.EldekiMiktar,
                sarfMalzeme.KritikStokSeviyesi,
                DateTime.UtcNow),
            cancellationToken: cancellationToken);
    }

    private async Task<Sonuc<bool>> CihazKimlikBilgisiniDogrulaAsync(string? seriNumarasi, string? assetTag, Guid? haricCihazId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(seriNumarasi) && string.IsNullOrWhiteSpace(assetTag))
        {
            return Sonuc<bool>.Basarisiz(EnvanterMesajlari.CihazKimligiZorunlu);
        }

        if (await cihazRepository.SeriNumarasiVeyaAssetTagKullaniliyorMuAsync(BosIseNull(seriNumarasi), BosIseNull(assetTag), haricCihazId, cancellationToken))
        {
            return Sonuc<bool>.Basarisiz(EnvanterMesajlari.CihazKimligiKullaniliyor);
        }

        return Sonuc<bool>.Basarili(true);
    }

    private async Task<string> SiradakiAssetTagUretAsync(CancellationToken cancellationToken)
    {
        var siraNumarasi = await cihazRepository.SonAssetTagSiraNumarasiAsync(cancellationToken);

        while (true)
        {
            siraNumarasi++;
            var assetTag = $"BT-{siraNumarasi:000000}";

            if (!await cihazRepository.SeriNumarasiVeyaAssetTagKullaniliyorMuAsync(null, assetTag, null, cancellationToken))
            {
                return assetTag;
            }
        }
    }

    private static Sonuc<bool> CihazDurumunuHareketeGoreGuncelle(Cihaz cihaz, CihazDurumHareketiIstek istek)
    {
        switch (istek.Neden)
        {
            case StokHareketNedeni.Ariza:
                cihaz.Durum = CihazDurumu.Bakimda;
                cihaz.EldenCikarmaTipi = EldenCikarmaTipi.Yok;
                cihaz.EldenCikarmaAciklamasi = null;
                cihaz.SatilanKisiVeyaKurum = null;
                break;

            case StokHareketNedeni.BakimdanDondu:
                cihaz.Durum = CihazDurumu.Kullanilabilir;
                cihaz.EldenCikarmaTipi = EldenCikarmaTipi.Yok;
                cihaz.EldenCikarmaAciklamasi = null;
                cihaz.SatilanKisiVeyaKurum = null;
                break;

            case StokHareketNedeni.Zimmetlendi:
                cihaz.Durum = CihazDurumu.Zimmetli;
                cihaz.EldenCikarmaTipi = EldenCikarmaTipi.Yok;
                cihaz.EldenCikarmaAciklamasi = null;
                cihaz.SatilanKisiVeyaKurum = null;
                break;

            case StokHareketNedeni.ZimmetIadeAlindi:
                cihaz.Durum = CihazDurumu.Incelemede;
                cihaz.EldenCikarmaTipi = EldenCikarmaTipi.Yok;
                cihaz.EldenCikarmaAciklamasi = null;
                cihaz.SatilanKisiVeyaKurum = null;
                break;

            case StokHareketNedeni.IncelemeyeAlindi:
                cihaz.Durum = CihazDurumu.Incelemede;
                cihaz.EldenCikarmaTipi = EldenCikarmaTipi.Yok;
                cihaz.EldenCikarmaAciklamasi = null;
                cihaz.SatilanKisiVeyaKurum = null;
                break;

            case StokHareketNedeni.HasarliTeslimAlindi:
                cihaz.Durum = CihazDurumu.HasarliTeslimAlindi;
                cihaz.EldenCikarmaTipi = EldenCikarmaTipi.Yok;
                cihaz.EldenCikarmaAciklamasi = null;
                cihaz.SatilanKisiVeyaKurum = null;
                break;

            case StokHareketNedeni.Calinma:
                cihaz.Durum = CihazDurumu.Calindi;
                cihaz.EldenCikarmaTipi = EldenCikarmaTipi.Yok;
                cihaz.EldenCikarmaAciklamasi = BosIseNull(istek.Aciklama);
                cihaz.SatilanKisiVeyaKurum = null;
                break;

            case StokHareketNedeni.Kaybolma:
                cihaz.Durum = CihazDurumu.Kayip;
                cihaz.EldenCikarmaTipi = EldenCikarmaTipi.Yok;
                cihaz.EldenCikarmaAciklamasi = BosIseNull(istek.Aciklama);
                cihaz.SatilanKisiVeyaKurum = null;
                break;

            case StokHareketNedeni.HurdaIskarta:
                cihaz.Durum = CihazDurumu.HurdaIskarta;
                cihaz.EldenCikarmaTipi = istek.EldenCikarmaTipi;
                cihaz.EldenCikarmaAciklamasi = BosIseNull(istek.Aciklama);
                cihaz.SatilanKisiVeyaKurum = BosIseNull(istek.SatilanKisiVeyaKurum);
                break;

            case StokHareketNedeni.ManuelStokCikisi:
            case StokHareketNedeni.KullanimOmruBitti:
            case StokHareketNedeni.FizikselSayimDuzeltmesi:
                cihaz.Durum = CihazDurumu.KullanimDisi;
                cihaz.EldenCikarmaTipi = istek.EldenCikarmaTipi == EldenCikarmaTipi.Yok
                    ? EldenCikarmaTipi.Diger
                    : istek.EldenCikarmaTipi;
                cihaz.EldenCikarmaAciklamasi = BosIseNull(istek.Aciklama);
                cihaz.SatilanKisiVeyaKurum = BosIseNull(istek.SatilanKisiVeyaKurum);
                break;

            default:
                return Sonuc<bool>.Basarisiz(EnvanterMesajlari.CihazDurumHareketiDesteklenmiyor);
        }

        CihazKapsamAlanlariniDurumaGoreGuncelle(cihaz);
        return Sonuc<bool>.Basarili(true);
    }

    private static void CihazKapsamAlanlariniDurumaGoreGuncelle(Cihaz cihaz)
    {
        var envanterDisindaMi = cihaz.Durum is CihazDurumu.Kayip or CihazDurumu.Calindi or CihazDurumu.KullanimDisi
            || (cihaz.Durum == CihazDurumu.HurdaIskarta && cihaz.EldenCikarmaTipi != EldenCikarmaTipi.Yok);

        cihaz.AktifMi = !envanterDisindaMi;
        cihaz.ToplamVarligaDahilMi = !envanterDisindaMi;

        if (!envanterDisindaMi)
        {
            cihaz.EnvanterdenCikisTarihi = null;
            cihaz.EldenCikarmaTipi = EldenCikarmaTipi.Yok;
            cihaz.EldenCikarmaAciklamasi = null;
            cihaz.SatilanKisiVeyaKurum = null;
            return;
        }

        cihaz.EnvanterdenCikisTarihi ??= DateOnly.FromDateTime(DateTime.UtcNow);

        if (cihaz.Durum == CihazDurumu.KullanimDisi && cihaz.EldenCikarmaTipi == EldenCikarmaTipi.Yok)
        {
            cihaz.EldenCikarmaTipi = EldenCikarmaTipi.Diger;
        }

        if (cihaz.Durum is CihazDurumu.Kayip or CihazDurumu.Calindi)
        {
            cihaz.EldenCikarmaTipi = EldenCikarmaTipi.Yok;
            cihaz.EldenCikarmaAciklamasi = null;
            cihaz.SatilanKisiVeyaKurum = null;
            return;
        }

        if (cihaz.EldenCikarmaTipi != EldenCikarmaTipi.Satildi)
        {
            cihaz.SatilanKisiVeyaKurum = null;
        }
    }

    private static StokHareketTipi CihazDurumHareketTipiniBelirle(CihazDurumHareketiIstek istek)
        => (istek.Neden is StokHareketNedeni.ManuelStokCikisi
                or StokHareketNedeni.FizikselSayimDuzeltmesi
                or StokHareketNedeni.Calinma
                or StokHareketNedeni.Kaybolma
                or StokHareketNedeni.KullanimOmruBitti)
            || (istek.Neden == StokHareketNedeni.HurdaIskarta && istek.EldenCikarmaTipi != EldenCikarmaTipi.Yok)
            ? StokHareketTipi.Cikis
            : StokHareketTipi.Duzeltme;

    private static bool SarfMalzemeStokHareketNedeniMi(StokHareketNedeni neden)
        => neden is not StokHareketNedeni.BakimdanDondu
            and not StokHareketNedeni.IncelemeyeAlindi
            and not StokHareketNedeni.HasarliTeslimAlindi
            and not StokHareketNedeni.Zimmetlendi
            and not StokHareketNedeni.ZimmetIadeAlindi;

    private async Task<Sonuc<bool>> CihazReferanslariniDogrulaAsync(Guid kategoriId, Guid lokasyonId, CancellationToken cancellationToken)
    {
        if (!await kategoriRepository.AktifVarMiAsync(kategoriId, VarlikTuru.SeriNumarali, cancellationToken))
        {
            return Sonuc<bool>.Basarisiz(EnvanterMesajlari.SeriNumaraliCihazAktifKategoriYok);
        }

        if (!await lokasyonRepository.AktifVarMiAsync(lokasyonId, cancellationToken))
        {
            return Sonuc<bool>.Basarisiz(EnvanterMesajlari.AktifLokasyonBulunamadi);
        }

        return Sonuc<bool>.Basarili(true);
    }

    private async Task<Sonuc<bool>> SarfMalzemeReferanslariniDogrulaAsync(Guid kategoriId, Guid lokasyonId, CancellationToken cancellationToken)
    {
        if (!await kategoriRepository.AktifVarMiAsync(kategoriId, VarlikTuru.SarfMalzeme, cancellationToken))
        {
            return Sonuc<bool>.Basarisiz(EnvanterMesajlari.SarfMalzemeAktifKategoriYok);
        }

        if (!await lokasyonRepository.AktifVarMiAsync(lokasyonId, cancellationToken))
        {
            return Sonuc<bool>.Basarisiz(EnvanterMesajlari.AktifLokasyonBulunamadi);
        }

        return Sonuc<bool>.Basarili(true);
    }

    private async Task<Sonuc<bool>> KritikStokKuraliDogrulaAsync(Guid lokasyonId, Guid kategoriId, int kritikStokSeviyesi, CancellationToken cancellationToken)
    {
        if (kritikStokSeviyesi < 0)
        {
            return Sonuc<bool>.Basarisiz(EnvanterMesajlari.KritikStokNegatifOlamaz);
        }

        if (!await lokasyonRepository.AktifVarMiAsync(lokasyonId, cancellationToken))
        {
            return Sonuc<bool>.Basarisiz(EnvanterMesajlari.AktifLokasyonBulunamadi);
        }

        if (!await kategoriRepository.AktifVarMiAsync(kategoriId, VarlikTuru.SeriNumarali, cancellationToken))
        {
            return Sonuc<bool>.Basarisiz(EnvanterMesajlari.KritikStokKuraliSeriNumaraliKategoriOlmali);
        }

        return Sonuc<bool>.Basarili(true);
    }

    private async Task<Sonuc<bool>> PasifKritikStokKuraliDogrulaAsync(Guid lokasyonId, Guid kategoriId, int kritikStokSeviyesi, CancellationToken cancellationToken)
    {
        if (kritikStokSeviyesi < 0)
        {
            return Sonuc<bool>.Basarisiz(EnvanterMesajlari.KritikStokNegatifOlamaz);
        }

        if (!await lokasyonRepository.VarMiAsync(lokasyonId, cancellationToken))
        {
            return Sonuc<bool>.Basarisiz(EnvanterMesajlari.LokasyonBulunamadi);
        }

        if (!await kategoriRepository.VarMiAsync(kategoriId, cancellationToken))
        {
            return Sonuc<bool>.Basarisiz(EnvanterMesajlari.KategoriBulunamadi);
        }

        return Sonuc<bool>.Basarili(true);
    }

    private async Task<bool> KritikStokKuraliKullaniliyorMuAsync(Guid lokasyonId, Guid kategoriId, string? cihazModeli, Guid? haricKuralId, CancellationToken cancellationToken)
    {
        var kurallar = await kritikStokKuraliRepository.ListeleAsync(cancellationToken);
        return kurallar.Any(kural =>
            kural.LokasyonId == lokasyonId
            && kural.KategoriId == kategoriId
            && (!haricKuralId.HasValue || kural.Id != haricKuralId.Value)
            && string.Equals(kural.CihazModeli ?? string.Empty, cihazModeli ?? string.Empty, StringComparison.OrdinalIgnoreCase));
    }

    private TimeSpan ReferansVeriCacheSuresi()
        => TimeSpan.FromMinutes(Math.Max(cacheAyarlari.Value.ReferansVeriDakika, 1));

    private static string? BosIseNull(string? deger)
        => string.IsNullOrWhiteSpace(deger) ? null : deger.Trim();

    private static KategoriCevap KategoriCevabaDonustur(Kategori kategori)
        => new(kategori.Id, kategori.Ad, kategori.UstKategoriId, kategori.VarlikTuru, kategori.KritikStokSeviyesi, kategori.AktifMi);

    private static LokasyonCevap LokasyonCevabaDonustur(Lokasyon lokasyon)
        => new(lokasyon.Id, lokasyon.Ad, lokasyon.UstLokasyonId, lokasyon.AktifMi);

    private static CihazCevap CihazCevabaDonustur(Cihaz cihaz)
        => new(
            cihaz.Id,
            cihaz.SeriNumarasi,
            cihaz.AssetTag,
            cihaz.Ad,
            cihaz.Marka,
            cihaz.Model,
            cihaz.KategoriId,
            cihaz.LokasyonId,
            cihaz.Durum,
            cihaz.EnvantereGirisTarihi,
            cihaz.EnvanterdenCikisTarihi,
            cihaz.EldenCikarmaTipi,
            cihaz.EldenCikarmaAciklamasi,
            cihaz.SatilanKisiVeyaKurum,
            cihaz.AktifMi,
            cihaz.ToplamVarligaDahilMi);

    private static SarfMalzemeCevap SarfMalzemeCevabaDonustur(SarfMalzeme sarfMalzeme)
        => new(
            sarfMalzeme.Id,
            sarfMalzeme.Ad,
            sarfMalzeme.KategoriId,
            sarfMalzeme.LokasyonId,
            sarfMalzeme.EldekiMiktar,
            sarfMalzeme.KritikStokSeviyesi,
            sarfMalzeme.Birim,
            sarfMalzeme.AktifMi);

    private static KritikStokKuraliCevap KritikStokKuraliCevabaDonustur(KritikStokKurali kural)
        => new(kural.Id, kural.LokasyonId, kural.KategoriId, kural.CihazModeli, kural.KritikStokSeviyesi, kural.AktifMi);

    private static StokHareketiCevap StokHareketiCevabaDonustur(StokHareketi stokHareketi)
        => new(
            stokHareketi.Id,
            stokHareketi.CihazId,
            stokHareketi.SarfMalzemeId,
            stokHareketi.HareketTipi,
            stokHareketi.Neden,
            stokHareketi.Miktar,
            stokHareketi.Aciklama,
            stokHareketi.OlusturanKullaniciId,
            stokHareketi.OlusturulmaTarihi);
}
