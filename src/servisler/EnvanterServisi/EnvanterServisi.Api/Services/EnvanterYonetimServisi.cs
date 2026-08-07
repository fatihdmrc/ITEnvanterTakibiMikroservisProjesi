using EnvanterServisi.Api.Contracts.Cihazlar;
using EnvanterServisi.Api.Contracts.Kategoriler;
using EnvanterServisi.Api.Contracts.Lokasyonlar;
using EnvanterServisi.Api.Contracts.SarfMalzemeler;
using EnvanterServisi.Api.Contracts.Stok;
using EnvanterServisi.Api.Domain.Entities;
using EnvanterServisi.Api.Domain.Enums;
using EnvanterServisi.Api.Repositories;

namespace EnvanterServisi.Api.Services;

public sealed class EnvanterYonetimServisi(
    IKategoriRepository kategoriRepository,
    ILokasyonRepository lokasyonRepository,
    ICihazRepository cihazRepository,
    ISarfMalzemeRepository sarfMalzemeRepository,
    IKritikStokKuraliRepository kritikStokKuraliRepository,
    IStokHareketiRepository stokHareketiRepository) : IEnvanterServisi
{
    // Servis katmanı, HTTP detayından bağımsız olarak envanter iş kurallarını uygular.
    public async Task<IReadOnlyCollection<KategoriCevap>> KategorileriListeleAsync(CancellationToken cancellationToken = default)
    {
        var kategoriler = await kategoriRepository.ListeleAsync(cancellationToken);
        return kategoriler.Select(KategoriCevabaDonustur).ToList();
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
            return Sonuc<KategoriCevap>.Basarisiz("Kategori adı zorunludur.");
        }

        if (istek.UstKategoriId.HasValue && !await kategoriRepository.VarMiAsync(istek.UstKategoriId.Value, cancellationToken))
        {
            return Sonuc<KategoriCevap>.Basarisiz("Üst kategori bulunamadı.");
        }

        var ad = istek.Ad.Trim();
        if (await kategoriRepository.AdKullaniliyorMuAsync(ad, istek.UstKategoriId, cancellationToken: cancellationToken))
        {
            return Sonuc<KategoriCevap>.Basarisiz("Aynı üst kategori altında bu ada sahip kategori zaten var.");
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

        return Sonuc<KategoriCevap>.Basarili(KategoriCevabaDonustur(kategori));
    }

    public async Task<Sonuc<KategoriCevap>> KategoriGuncelleAsync(Guid id, KategoriGuncelleIstek istek, CancellationToken cancellationToken = default)
    {
        var kategori = await kategoriRepository.GetirAsync(id, cancellationToken);
        if (kategori is null)
        {
            return Sonuc<KategoriCevap>.Basarisiz("Kategori bulunamadı.");
        }

        if (id == istek.UstKategoriId)
        {
            return Sonuc<KategoriCevap>.Basarisiz("Kategori kendi üst kategorisi olamaz.");
        }

        if (istek.UstKategoriId.HasValue && !await kategoriRepository.VarMiAsync(istek.UstKategoriId.Value, cancellationToken))
        {
            return Sonuc<KategoriCevap>.Basarisiz("Üst kategori bulunamadı.");
        }

        var ad = istek.Ad.Trim();
        if (string.IsNullOrWhiteSpace(ad))
        {
            return Sonuc<KategoriCevap>.Basarisiz("Kategori adı zorunludur.");
        }

        if (await kategoriRepository.AdKullaniliyorMuAsync(ad, istek.UstKategoriId, id, cancellationToken))
        {
            return Sonuc<KategoriCevap>.Basarisiz("Aynı üst kategori altında bu ada sahip kategori zaten var.");
        }

        kategori.Ad = ad;
        kategori.UstKategoriId = istek.UstKategoriId;
        kategori.VarlikTuru = istek.VarlikTuru;
        kategori.KritikStokSeviyesi = istek.KritikStokSeviyesi;
        kategori.AktifMi = istek.AktifMi;

        await kategoriRepository.KaydetAsync(cancellationToken);
        return Sonuc<KategoriCevap>.Basarili(KategoriCevabaDonustur(kategori));
    }

    public async Task<IReadOnlyCollection<LokasyonCevap>> LokasyonlariListeleAsync(CancellationToken cancellationToken = default)
    {
        var lokasyonlar = await lokasyonRepository.ListeleAsync(cancellationToken);
        return lokasyonlar.Select(LokasyonCevabaDonustur).ToList();
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
            return Sonuc<LokasyonCevap>.Basarisiz("Lokasyon adı zorunludur.");
        }

        if (istek.UstLokasyonId.HasValue && !await lokasyonRepository.VarMiAsync(istek.UstLokasyonId.Value, cancellationToken))
        {
            return Sonuc<LokasyonCevap>.Basarisiz("Üst lokasyon bulunamadı.");
        }

        if (await lokasyonRepository.AdKullaniliyorMuAsync(ad, istek.UstLokasyonId, cancellationToken: cancellationToken))
        {
            return Sonuc<LokasyonCevap>.Basarisiz("Aynı üst lokasyon altında bu ada sahip lokasyon zaten var.");
        }

        var lokasyon = new Lokasyon
        {
            Ad = ad,
            UstLokasyonId = istek.UstLokasyonId
        };

        lokasyonRepository.Ekle(lokasyon);
        await lokasyonRepository.KaydetAsync(cancellationToken);

        return Sonuc<LokasyonCevap>.Basarili(LokasyonCevabaDonustur(lokasyon));
    }

    public async Task<Sonuc<LokasyonCevap>> LokasyonGuncelleAsync(Guid id, LokasyonGuncelleIstek istek, CancellationToken cancellationToken = default)
    {
        var lokasyon = await lokasyonRepository.GetirAsync(id, cancellationToken);
        if (lokasyon is null)
        {
            return Sonuc<LokasyonCevap>.Basarisiz("Lokasyon bulunamadı.");
        }

        if (id == istek.UstLokasyonId)
        {
            return Sonuc<LokasyonCevap>.Basarisiz("Lokasyon kendi üst lokasyonu olamaz.");
        }

        if (istek.UstLokasyonId.HasValue && !await lokasyonRepository.VarMiAsync(istek.UstLokasyonId.Value, cancellationToken))
        {
            return Sonuc<LokasyonCevap>.Basarisiz("Üst lokasyon bulunamadı.");
        }

        var ad = istek.Ad.Trim();
        if (string.IsNullOrWhiteSpace(ad))
        {
            return Sonuc<LokasyonCevap>.Basarisiz("Lokasyon adı zorunludur.");
        }

        if (await lokasyonRepository.AdKullaniliyorMuAsync(ad, istek.UstLokasyonId, id, cancellationToken))
        {
            return Sonuc<LokasyonCevap>.Basarisiz("Aynı üst lokasyon altında bu ada sahip lokasyon zaten var.");
        }

        lokasyon.Ad = ad;
        lokasyon.UstLokasyonId = istek.UstLokasyonId;
        lokasyon.AktifMi = istek.AktifMi;

        await lokasyonRepository.KaydetAsync(cancellationToken);
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

        cihazRepository.Ekle(cihaz);
        await cihazRepository.KaydetAsync(cancellationToken);

        return Sonuc<CihazCevap>.Basarili(CihazCevabaDonustur(cihaz));
    }

    public async Task<Sonuc<CihazCevap>> CihazGuncelleAsync(Guid id, CihazGuncelleIstek istek, CancellationToken cancellationToken = default)
    {
        var cihaz = await cihazRepository.GetirAsync(id, cancellationToken);
        if (cihaz is null)
        {
            return Sonuc<CihazCevap>.Basarisiz("Cihaz bulunamadı.");
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
        cihaz.Durum = istek.Durum;
        cihaz.EnvantereGirisTarihi = istek.EnvantereGirisTarihi;
        cihaz.EnvanterdenCikisTarihi = istek.EnvanterdenCikisTarihi;
        cihaz.EldenCikarmaTipi = istek.EldenCikarmaTipi;
        cihaz.EldenCikarmaAciklamasi = BosIseNull(istek.EldenCikarmaAciklamasi);
        cihaz.SatilanKisiVeyaKurum = BosIseNull(istek.SatilanKisiVeyaKurum);
        cihaz.AktifMi = istek.AktifMi;
        cihaz.ToplamVarligaDahilMi = istek.Durum == CihazDurumu.KullanimDisi ? false : istek.ToplamVarligaDahilMi;

        if (cihaz.Durum == CihazDurumu.KullanimDisi)
        {
            cihaz.EnvanterdenCikisTarihi ??= DateOnly.FromDateTime(DateTime.UtcNow);
        }

        await cihazRepository.KaydetAsync(cancellationToken);
        return Sonuc<CihazCevap>.Basarili(CihazCevabaDonustur(cihaz));
    }

    public async Task<Sonuc<CihazCevap>> CihazStokHareketiIsleAsync(Guid id, CihazStokHareketiIstek istek, Guid olusturanKullaniciId, CancellationToken cancellationToken = default)
    {
        var cihaz = await cihazRepository.GetirAsync(id, cancellationToken);
        if (cihaz is null)
        {
            return Sonuc<CihazCevap>.Basarisiz("Cihaz bulunamadı.");
        }

        var durumSonucu = CihazDurumunuStokHareketineGoreGuncelle(cihaz, istek);
        if (!durumSonucu.BasariliMi)
        {
            return Sonuc<CihazCevap>.Basarisiz(durumSonucu.Hata!);
        }

        stokHareketiRepository.Ekle(new StokHareketi
        {
            CihazId = cihaz.Id,
            HareketTipi = StokHareketTipi.Cikis,
            Neden = istek.Neden,
            Aciklama = BosIseNull(istek.Aciklama),
            OlusturanKullaniciId = olusturanKullaniciId
        });

        await cihazRepository.KaydetAsync(cancellationToken);
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
            return Sonuc<SarfMalzemeCevap>.Basarisiz("Sarf malzeme adı zorunludur.");
        }

        if (istek.EldekiMiktar < 0 || istek.KritikStokSeviyesi < 0)
        {
            return Sonuc<SarfMalzemeCevap>.Basarisiz("Miktar ve kritik stok seviyesi negatif olamaz.");
        }

        if (await sarfMalzemeRepository.AdKullaniliyorMuAsync(ad, istek.KategoriId, istek.LokasyonId, cancellationToken: cancellationToken))
        {
            return Sonuc<SarfMalzemeCevap>.Basarisiz("Aynı kategori ve lokasyonda bu sarf malzeme zaten var.");
        }

        var sarfMalzeme = new SarfMalzeme
        {
            Ad = ad,
            KategoriId = istek.KategoriId,
            LokasyonId = istek.LokasyonId,
            EldekiMiktar = istek.EldekiMiktar,
            KritikStokSeviyesi = istek.KritikStokSeviyesi,
            Birim = string.IsNullOrWhiteSpace(istek.Birim) ? "Adet" : istek.Birim.Trim()
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
            return Sonuc<SarfMalzemeCevap>.Basarisiz("Sarf malzeme bulunamadı.");
        }

        var sonuc = await SarfMalzemeReferanslariniDogrulaAsync(istek.KategoriId, istek.LokasyonId, cancellationToken);
        if (!sonuc.BasariliMi)
        {
            return Sonuc<SarfMalzemeCevap>.Basarisiz(sonuc.Hata!);
        }

        var ad = istek.Ad.Trim();
        if (string.IsNullOrWhiteSpace(ad))
        {
            return Sonuc<SarfMalzemeCevap>.Basarisiz("Sarf malzeme adı zorunludur.");
        }

        if (istek.EldekiMiktar < 0 || istek.KritikStokSeviyesi < 0)
        {
            return Sonuc<SarfMalzemeCevap>.Basarisiz("Miktar ve kritik stok seviyesi negatif olamaz.");
        }

        if (await sarfMalzemeRepository.AdKullaniliyorMuAsync(ad, istek.KategoriId, istek.LokasyonId, id, cancellationToken))
        {
            return Sonuc<SarfMalzemeCevap>.Basarisiz("Aynı kategori ve lokasyonda bu sarf malzeme zaten var.");
        }

        sarfMalzeme.Ad = ad;
        sarfMalzeme.KategoriId = istek.KategoriId;
        sarfMalzeme.LokasyonId = istek.LokasyonId;
        sarfMalzeme.EldekiMiktar = istek.EldekiMiktar;
        sarfMalzeme.KritikStokSeviyesi = istek.KritikStokSeviyesi;
        sarfMalzeme.Birim = string.IsNullOrWhiteSpace(istek.Birim) ? "Adet" : istek.Birim.Trim();
        sarfMalzeme.AktifMi = istek.AktifMi;

        await sarfMalzemeRepository.KaydetAsync(cancellationToken);
        return Sonuc<SarfMalzemeCevap>.Basarili(SarfMalzemeCevabaDonustur(sarfMalzeme));
    }

    public async Task<Sonuc<SarfMalzemeCevap>> SarfMalzemeStokHareketiIsleAsync(Guid id, SarfMalzemeStokHareketiIstek istek, Guid olusturanKullaniciId, CancellationToken cancellationToken = default)
    {
        var sarfMalzeme = await sarfMalzemeRepository.GetirAsync(id, cancellationToken);
        if (sarfMalzeme is null)
        {
            return Sonuc<SarfMalzemeCevap>.Basarisiz("Sarf malzeme bulunamadı.");
        }

        if (istek.Miktar <= 0)
        {
            return Sonuc<SarfMalzemeCevap>.Basarisiz("Stok hareket miktarı sıfırdan büyük olmalıdır.");
        }

        if (istek.HareketTipi == StokHareketTipi.Cikis && sarfMalzeme.EldekiMiktar < istek.Miktar)
        {
            return Sonuc<SarfMalzemeCevap>.Basarisiz("Eldeki miktardan fazla stok çıkışı yapılamaz.");
        }

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

        var kural = new KritikStokKurali
        {
            LokasyonId = istek.LokasyonId,
            KategoriId = istek.KategoriId,
            CihazModeli = BosIseNull(istek.CihazModeli),
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
            return Sonuc<KritikStokKuraliCevap>.Basarisiz("Kritik stok kuralı bulunamadı.");
        }

        var sonuc = await KritikStokKuraliDogrulaAsync(istek.LokasyonId, istek.KategoriId, istek.KritikStokSeviyesi, cancellationToken);
        if (!sonuc.BasariliMi)
        {
            return Sonuc<KritikStokKuraliCevap>.Basarisiz(sonuc.Hata!);
        }

        kural.LokasyonId = istek.LokasyonId;
        kural.KategoriId = istek.KategoriId;
        kural.CihazModeli = BosIseNull(istek.CihazModeli);
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

        var kurallar = (await kritikStokKuraliRepository.ListeleAsync(cancellationToken))
            .Where(kural => kural.AktifMi)
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

    private async Task<Sonuc<bool>> CihazKimlikBilgisiniDogrulaAsync(string? seriNumarasi, string? assetTag, Guid? haricCihazId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(seriNumarasi) && string.IsNullOrWhiteSpace(assetTag))
        {
            return Sonuc<bool>.Basarisiz("Seri numarası veya asset tag alanlarından en az biri zorunludur.");
        }

        if (await cihazRepository.SeriNumarasiVeyaAssetTagKullaniliyorMuAsync(BosIseNull(seriNumarasi), BosIseNull(assetTag), haricCihazId, cancellationToken))
        {
            return Sonuc<bool>.Basarisiz("Seri numarası veya asset tag başka bir cihazda kullanılıyor.");
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

    private static Sonuc<bool> CihazDurumunuStokHareketineGoreGuncelle(Cihaz cihaz, CihazStokHareketiIstek istek)
    {
        switch (istek.Neden)
        {
            case StokHareketNedeni.Ariza:
                cihaz.Durum = CihazDurumu.Bakimda;
                cihaz.ToplamVarligaDahilMi = true;
                return Sonuc<bool>.Basarili(true);

            case StokHareketNedeni.Calinma:
                cihaz.Durum = CihazDurumu.Calindi;
                cihaz.AktifMi = false;
                cihaz.ToplamVarligaDahilMi = false;
                cihaz.EnvanterdenCikisTarihi ??= DateOnly.FromDateTime(DateTime.UtcNow);
                return Sonuc<bool>.Basarili(true);

            case StokHareketNedeni.Kaybolma:
                cihaz.Durum = CihazDurumu.Kayip;
                cihaz.AktifMi = false;
                cihaz.ToplamVarligaDahilMi = false;
                cihaz.EnvanterdenCikisTarihi ??= DateOnly.FromDateTime(DateTime.UtcNow);
                return Sonuc<bool>.Basarili(true);

            case StokHareketNedeni.HurdaIskarta:
                if (istek.EldenCikarmaTipi == EldenCikarmaTipi.Yok)
                {
                    cihaz.Durum = CihazDurumu.HurdaIskarta;
                    cihaz.ToplamVarligaDahilMi = true;
                    cihaz.EldenCikarmaTipi = EldenCikarmaTipi.Yok;
                    return Sonuc<bool>.Basarili(true);
                }

                cihaz.Durum = CihazDurumu.KullanimDisi;
                cihaz.AktifMi = false;
                cihaz.ToplamVarligaDahilMi = false;
                cihaz.EnvanterdenCikisTarihi ??= DateOnly.FromDateTime(DateTime.UtcNow);
                cihaz.EldenCikarmaTipi = istek.EldenCikarmaTipi;
                cihaz.EldenCikarmaAciklamasi = BosIseNull(istek.Aciklama);
                cihaz.SatilanKisiVeyaKurum = BosIseNull(istek.SatilanKisiVeyaKurum);
                return Sonuc<bool>.Basarili(true);

            case StokHareketNedeni.ManuelStokCikisi:
            case StokHareketNedeni.KullanimOmruBitti:
            case StokHareketNedeni.FizikselSayimDuzeltmesi:
                cihaz.Durum = CihazDurumu.KullanimDisi;
                cihaz.AktifMi = false;
                cihaz.ToplamVarligaDahilMi = false;
                cihaz.EnvanterdenCikisTarihi ??= DateOnly.FromDateTime(DateTime.UtcNow);
                cihaz.EldenCikarmaTipi = istek.EldenCikarmaTipi == EldenCikarmaTipi.Yok
                    ? EldenCikarmaTipi.Diger
                    : istek.EldenCikarmaTipi;
                cihaz.EldenCikarmaAciklamasi = BosIseNull(istek.Aciklama);
                cihaz.SatilanKisiVeyaKurum = BosIseNull(istek.SatilanKisiVeyaKurum);
                return Sonuc<bool>.Basarili(true);

            default:
                return Sonuc<bool>.Basarisiz("Bu neden cihaz stok çıkışı için desteklenmiyor.");
        }
    }

    private async Task<Sonuc<bool>> CihazReferanslariniDogrulaAsync(Guid kategoriId, Guid lokasyonId, CancellationToken cancellationToken)
    {
        if (!await kategoriRepository.AktifVarMiAsync(kategoriId, VarlikTuru.SeriNumarali, cancellationToken))
        {
            return Sonuc<bool>.Basarisiz("Seri numaralı cihaz için aktif kategori bulunamadı.");
        }

        if (!await lokasyonRepository.AktifVarMiAsync(lokasyonId, cancellationToken))
        {
            return Sonuc<bool>.Basarisiz("Aktif lokasyon bulunamadı.");
        }

        return Sonuc<bool>.Basarili(true);
    }

    private async Task<Sonuc<bool>> SarfMalzemeReferanslariniDogrulaAsync(Guid kategoriId, Guid lokasyonId, CancellationToken cancellationToken)
    {
        if (!await kategoriRepository.AktifVarMiAsync(kategoriId, VarlikTuru.SarfMalzeme, cancellationToken))
        {
            return Sonuc<bool>.Basarisiz("Sarf malzeme için aktif kategori bulunamadı.");
        }

        if (!await lokasyonRepository.AktifVarMiAsync(lokasyonId, cancellationToken))
        {
            return Sonuc<bool>.Basarisiz("Aktif lokasyon bulunamadı.");
        }

        return Sonuc<bool>.Basarili(true);
    }

    private async Task<Sonuc<bool>> KritikStokKuraliDogrulaAsync(Guid lokasyonId, Guid kategoriId, int kritikStokSeviyesi, CancellationToken cancellationToken)
    {
        if (kritikStokSeviyesi < 0)
        {
            return Sonuc<bool>.Basarisiz("Kritik stok seviyesi negatif olamaz.");
        }

        if (!await lokasyonRepository.AktifVarMiAsync(lokasyonId, cancellationToken))
        {
            return Sonuc<bool>.Basarisiz("Aktif lokasyon bulunamadı.");
        }

        if (!await kategoriRepository.AktifVarMiAsync(kategoriId, cancellationToken: cancellationToken))
        {
            return Sonuc<bool>.Basarisiz("Aktif kategori bulunamadı.");
        }

        return Sonuc<bool>.Basarili(true);
    }

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
