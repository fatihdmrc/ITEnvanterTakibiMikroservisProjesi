using ZimmetServisi.Api.Contracts.Events;
using ZimmetServisi.Api.Contracts.Zimmetler;
using ZimmetServisi.Api.Data;
using ZimmetServisi.Api.Domain.Entities;
using ZimmetServisi.Api.Domain.Enums;
using ZimmetServisi.Api.Repositories;
using ZimmetServisi.Api.Services.Harici;
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;

namespace ZimmetServisi.Api.Services;

public sealed class ZimmetYonetimServisi(
    ZimmetDbContext dbContext,
    IZimmetRepository zimmetRepository,
    KimlikPersonelApiClient kimlikPersonelApiClient,
    EnvanterApiClient envanterApiClient,
    ICapPublisher capPublisher) : IZimmetServisi
{
    public async Task<IReadOnlyCollection<ZimmetCevap>> ZimmetleriListeleAsync(
        Guid? personelId = null,
        Guid? cihazId = null,
        ZimmetDurumu? durum = null,
        CancellationToken cancellationToken = default)
    {
        var zimmetler = await zimmetRepository.FiltreleAsync(personelId, cihazId, durum, cancellationToken);
        return zimmetler.Select(ZimmetCevabaDonustur).ToList();
    }

    public async Task<ZimmetCevap?> ZimmetGetirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var zimmet = await zimmetRepository.GetirAsync(id, cancellationToken);
        return zimmet is null ? null : ZimmetCevabaDonustur(zimmet);
    }

    public async Task<Sonuc<ZimmetCevap>> ZimmetOlusturAsync(
        ZimmetOlusturIstek istek,
        Guid kullaniciId,
        string bearerToken,
        CancellationToken cancellationToken = default)
    {
        if (istek.CihazId == Guid.Empty || istek.PersonelId == Guid.Empty)
        {
            return Sonuc<ZimmetCevap>.Basarisiz("Cihaz ve personel seçimi zorunludur.");
        }

        if (await zimmetRepository.AcikZimmetVarMiAsync(istek.CihazId, cancellationToken: cancellationToken))
        {
            return Sonuc<ZimmetCevap>.Basarisiz("Bu cihaz için açık bir zimmet zaten var.");
        }

        var personelSonucu = await kimlikPersonelApiClient.PersonelGetirAsync(istek.PersonelId, bearerToken, cancellationToken);
        if (!personelSonucu.BasariliMi || personelSonucu.Veri is null)
        {
            return Sonuc<ZimmetCevap>.Basarisiz($"Personel doğrulanamadı: {personelSonucu.Hata}");
        }

        if (!personelSonucu.Veri.AktifMi || personelSonucu.Veri.Durum != HariciPersonelDurumu.Aktif)
        {
            return Sonuc<ZimmetCevap>.Basarisiz("Aktif olmayan veya işten ayrılmış personele zimmet oluşturulamaz.");
        }

        var cihazSonucu = await envanterApiClient.CihazGetirAsync(istek.CihazId, bearerToken, cancellationToken);
        if (!cihazSonucu.BasariliMi || cihazSonucu.Veri is null)
        {
            return Sonuc<ZimmetCevap>.Basarisiz($"Cihaz doğrulanamadı: {cihazSonucu.Hata}");
        }

        if (!cihazSonucu.Veri.AktifMi || cihazSonucu.Veri.Durum != HariciCihazDurumu.Kullanilabilir)
        {
            return Sonuc<ZimmetCevap>.Basarisiz("Yalnızca aktif ve kullanılabilir durumdaki cihazlar zimmetlenebilir.");
        }

        var personelAdSoyad = $"{personelSonucu.Veri.Ad} {personelSonucu.Veri.Soyad}".Trim();
        var zimmet = new Zimmet
        {
            CihazId = cihazSonucu.Veri.Id,
            CihazAd = CihazAdiniOlustur(cihazSonucu.Veri),
            CihazAssetTag = BosIseNull(cihazSonucu.Veri.AssetTag),
            CihazSeriNumarasi = BosIseNull(cihazSonucu.Veri.SeriNumarasi),
            PersonelId = personelSonucu.Veri.Id,
            PersonelAdSoyad = personelAdSoyad,
            PersonelEmail = personelSonucu.Veri.Email,
            ZimmetTarihi = istek.ZimmetTarihi ?? Bugun(),
            ZimmetleyenKullaniciId = kullaniciId,
            Durum = ZimmetDurumu.Aktif
        };

        using var transaction = dbContext.Database.BeginTransaction(capPublisher, autoCommit: false);

        try
        {
            zimmetRepository.Ekle(zimmet);
            await zimmetRepository.KaydetAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Sonuc<ZimmetCevap>.Basarisiz("Zimmet kaydı oluşturulamadı. Bu cihaz için açık zimmet olup olmadığını kontrol et.");
        }

        var cihazDurumSonucu = await envanterApiClient.CihazDurumHareketiIsleAsync(
            istek.CihazId,
            new HariciCihazDurumHareketiIstek(
                HariciStokHareketNedeni.Zimmetlendi,
                $"{personelAdSoyad} personeline zimmetlendi.",
                HariciEldenCikarmaTipi.Yok,
                null),
            bearerToken,
            cancellationToken);

        if (!cihazDurumSonucu.BasariliMi)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Sonuc<ZimmetCevap>.Basarisiz($"Cihaz durumu zimmetli yapılamadı: {cihazDurumSonucu.Hata}");
        }

        await ZimmetOlusturulduEventiYayinlaAsync(zimmet, cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Sonuc<ZimmetCevap>.Basarili(ZimmetCevabaDonustur(zimmet));
    }

    public async Task<Sonuc<ZimmetCevap>> IadeAlindiAsync(
        Guid id,
        ZimmetIadeAlindiIstek istek,
        Guid kullaniciId,
        string bearerToken,
        CancellationToken cancellationToken = default)
    {
        var zimmet = await zimmetRepository.GetirAsync(id, cancellationToken);
        if (zimmet is null)
        {
            return Sonuc<ZimmetCevap>.Basarisiz("Zimmet kaydı bulunamadı.");
        }

        if (zimmet.Durum != ZimmetDurumu.Aktif)
        {
            return Sonuc<ZimmetCevap>.Basarisiz("Yalnızca aktif zimmetler iade sürecine alınabilir.");
        }

        using var transaction = dbContext.Database.BeginTransaction(capPublisher, autoCommit: false);

        zimmet.Durum = ZimmetDurumu.IadeSurecinde;
        zimmet.IadeTarihi = istek.IadeTarihi ?? Bugun();
        zimmet.IadeAlanKullaniciId = kullaniciId;
        zimmet.IadeNotu = BosIseNull(istek.IadeNotu);

        await zimmetRepository.KaydetAsync(cancellationToken);

        var cihazDurumSonucu = await envanterApiClient.CihazDurumHareketiIsleAsync(
            zimmet.CihazId,
            new HariciCihazDurumHareketiIstek(
                HariciStokHareketNedeni.ZimmetIadeAlindi,
                $"{zimmet.PersonelAdSoyad} personelinden zimmet iadesi alındı.",
                HariciEldenCikarmaTipi.Yok,
                null),
            bearerToken,
            cancellationToken);

        if (!cihazDurumSonucu.BasariliMi)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Sonuc<ZimmetCevap>.Basarisiz($"Cihaz iade incelemesine alınamadı: {cihazDurumSonucu.Hata}");
        }

        await ZimmetIadeAlindiEventleriniYayinlaAsync(zimmet, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Sonuc<ZimmetCevap>.Basarili(ZimmetCevabaDonustur(zimmet));
    }

    public async Task<Sonuc<ZimmetCevap>> IadeKontroluTamamlaAsync(
        Guid id,
        ZimmetIadeKontroluIstek istek,
        Guid kullaniciId,
        string bearerToken,
        CancellationToken cancellationToken = default)
    {
        var zimmet = await zimmetRepository.GetirAsync(id, cancellationToken);
        if (zimmet is null)
        {
            return Sonuc<ZimmetCevap>.Basarisiz("Zimmet kaydı bulunamadı.");
        }

        if (zimmet.Durum != ZimmetDurumu.IadeSurecinde)
        {
            return Sonuc<ZimmetCevap>.Basarisiz("Fiziki kontrol yalnızca iade sürecindeki zimmetler için tamamlanabilir.");
        }

        var neden = IadeKontrolDurumunuHareketeDonustur(istek.IadeKontrolDurumu);
        using var transaction = dbContext.Database.BeginTransaction(capPublisher, autoCommit: false);

        zimmet.Durum = ZimmetDurumu.IadeEdildi;
        zimmet.IadeKontrolDurumu = istek.IadeKontrolDurumu;
        zimmet.IadeKontroluYapanKullaniciId = kullaniciId;
        zimmet.IadeNotu = BosIseNull(istek.IadeNotu) ?? zimmet.IadeNotu;

        await zimmetRepository.KaydetAsync(cancellationToken);

        var cihazDurumSonucu = await envanterApiClient.CihazDurumHareketiIsleAsync(
            zimmet.CihazId,
            new HariciCihazDurumHareketiIstek(
                neden,
                IadeKontrolAciklamasiOlustur(zimmet, istek),
                HariciEldenCikarmaTipi.Yok,
                null),
            bearerToken,
            cancellationToken);

        if (!cihazDurumSonucu.BasariliMi)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Sonuc<ZimmetCevap>.Basarisiz($"Cihaz durumu iade kontrol sonucuna göre güncellenemedi: {cihazDurumSonucu.Hata}");
        }

        await ZimmetIadeEdildiEventleriniYayinlaAsync(zimmet, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Sonuc<ZimmetCevap>.Basarili(ZimmetCevabaDonustur(zimmet));
    }

    private Task ZimmetOlusturulduEventiYayinlaAsync(Zimmet zimmet, CancellationToken cancellationToken)
    {
        var eventPayload = new ZimmetOlusturulduEvent(
            Guid.NewGuid(),
            zimmet.Id,
            zimmet.CihazId,
            zimmet.PersonelId,
            zimmet.PersonelAdSoyad,
            zimmet.CihazAd,
            zimmet.CihazAssetTag,
            zimmet.ZimmetTarihi,
            zimmet.ZimmetleyenKullaniciId,
            DateTime.UtcNow);

        return capPublisher.PublishAsync(EventAdlari.ZimmetOlusturuldu, eventPayload, cancellationToken: cancellationToken);
    }

    private async Task ZimmetIadeAlindiEventleriniYayinlaAsync(Zimmet zimmet, CancellationToken cancellationToken)
    {
        var iadeTarihi = zimmet.IadeTarihi ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var iadeAlanKullaniciId = zimmet.IadeAlanKullaniciId ?? Guid.Empty;

        await capPublisher.PublishAsync(
            EventAdlari.ZimmetIadeAlindi,
            new ZimmetIadeAlindiEvent(
                Guid.NewGuid(),
                zimmet.Id,
                zimmet.CihazId,
                zimmet.PersonelId,
                zimmet.PersonelAdSoyad,
                iadeTarihi,
                iadeAlanKullaniciId,
                zimmet.IadeNotu,
                DateTime.UtcNow),
            cancellationToken: cancellationToken);

        await capPublisher.PublishAsync(
            EventAdlari.CihazKontroleAlindi,
            new CihazKontroleAlindiEvent(
                Guid.NewGuid(),
                zimmet.Id,
                zimmet.CihazId,
                zimmet.PersonelId,
                zimmet.PersonelAdSoyad,
                iadeTarihi,
                iadeAlanKullaniciId,
                DateTime.UtcNow),
            cancellationToken: cancellationToken);
    }

    private async Task ZimmetIadeEdildiEventleriniYayinlaAsync(Zimmet zimmet, CancellationToken cancellationToken)
    {
        var kontrolYapanKullaniciId = zimmet.IadeKontroluYapanKullaniciId ?? Guid.Empty;
        var kontrolDurumu = zimmet.IadeKontrolDurumu ?? IadeKontrolDurumu.Saglam;

        await capPublisher.PublishAsync(
            EventAdlari.ZimmetIadeEdildi,
            new ZimmetIadeEdildiEvent(
                Guid.NewGuid(),
                zimmet.Id,
                zimmet.CihazId,
                zimmet.PersonelId,
                zimmet.PersonelAdSoyad,
                kontrolDurumu,
                kontrolYapanKullaniciId,
                zimmet.IadeNotu,
                DateTime.UtcNow),
            cancellationToken: cancellationToken);

        if (kontrolDurumu != IadeKontrolDurumu.HasarliTeslimAlindi)
        {
            return;
        }

        await capPublisher.PublishAsync(
            EventAdlari.CihazHasarliTeslimAlindi,
            new CihazHasarliTeslimAlindiEvent(
                Guid.NewGuid(),
                zimmet.Id,
                zimmet.CihazId,
                zimmet.PersonelId,
                zimmet.PersonelAdSoyad,
                zimmet.IadeNotu,
                kontrolYapanKullaniciId,
                DateTime.UtcNow),
            cancellationToken: cancellationToken);
    }

    private static HariciStokHareketNedeni IadeKontrolDurumunuHareketeDonustur(IadeKontrolDurumu durum)
        => durum switch
        {
            IadeKontrolDurumu.Saglam => HariciStokHareketNedeni.BakimdanDondu,
            IadeKontrolDurumu.Bakimda => HariciStokHareketNedeni.Ariza,
            IadeKontrolDurumu.HurdaIskarta => HariciStokHareketNedeni.HurdaIskarta,
            IadeKontrolDurumu.HasarliTeslimAlindi => HariciStokHareketNedeni.HasarliTeslimAlindi,
            _ => HariciStokHareketNedeni.IncelemeyeAlindi
        };

    private static string IadeKontrolAciklamasiOlustur(Zimmet zimmet, ZimmetIadeKontroluIstek istek)
    {
        var not = BosIseNull(istek.IadeNotu);
        var sonuc = istek.IadeKontrolDurumu switch
        {
            IadeKontrolDurumu.Saglam => "sağlam, tekrar kullanılabilir",
            IadeKontrolDurumu.Bakimda => "bakıma alınacak",
            IadeKontrolDurumu.HurdaIskarta => "hurda/ıskarta olarak ayrıldı",
            IadeKontrolDurumu.HasarliTeslimAlindi => "hasarlı teslim alındı",
            _ => "incelemede"
        };

        return string.IsNullOrWhiteSpace(not)
            ? $"{zimmet.PersonelAdSoyad} personelinden alınan zimmetin fiziki kontrol sonucu: {sonuc}."
            : $"{zimmet.PersonelAdSoyad} personelinden alınan zimmetin fiziki kontrol sonucu: {sonuc}. Not: {not}";
    }

    private static string CihazAdiniOlustur(HariciCihazCevap cihaz)
        => $"{cihaz.Ad} {cihaz.Marka} {cihaz.Model}".Trim();

    private static DateOnly Bugun()
        => DateOnly.FromDateTime(DateTime.UtcNow);

    private static string? BosIseNull(string? deger)
        => string.IsNullOrWhiteSpace(deger) ? null : deger.Trim();

    private static ZimmetCevap ZimmetCevabaDonustur(Zimmet zimmet)
        => new(
            zimmet.Id,
            zimmet.CihazId,
            zimmet.CihazAd,
            zimmet.CihazAssetTag,
            zimmet.CihazSeriNumarasi,
            zimmet.PersonelId,
            zimmet.PersonelAdSoyad,
            zimmet.PersonelEmail,
            zimmet.ZimmetTarihi,
            zimmet.ZimmetleyenKullaniciId,
            zimmet.IadeTarihi,
            zimmet.IadeAlanKullaniciId,
            zimmet.Durum,
            zimmet.IadeKontrolDurumu,
            zimmet.IadeKontroluYapanKullaniciId,
            zimmet.IadeNotu,
            zimmet.OlusturulmaTarihi,
            zimmet.GuncellenmeTarihi);
}
