using System.Diagnostics;
using EnvanterTakip.MvcClient.Models;
using EnvanterTakip.MvcClient.Sabitler;
using EnvanterTakip.MvcClient.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnvanterTakip.MvcClient.Controllers;

public sealed class EnvanterController(
    ILogger<EnvanterController> logger,
    EnvanterApiClient envanterApiClient) : Controller
{
    public async Task<IActionResult> Index(string? sekme, Guid? cihazKategoriId, Guid? cihazLokasyonId, bool? cihazAktifMi)
    {
        var token = TokenAl();
        var model = new EnvanterPanelModel
        {
            OturumVarMi = !string.IsNullOrWhiteSpace(token),
            AktifSekme = SekmeDogrula(sekme),
            CihazFiltre = new CihazFiltreModel
            {
                KategoriId = cihazKategoriId,
                LokasyonId = cihazLokasyonId,
                AktifMi = cihazAktifMi
            },
            BasariMesaji = TempData[MvcSabitleri.BasariMesajiTempDataKey] as string,
            HataMesaji = TempData[MvcSabitleri.HataMesajiTempDataKey] as string
        };

        if (!string.IsNullOrWhiteSpace(token))
        {
            var kategoriSonucu = await envanterApiClient.KategorileriListeleAsync(token);
            model.Kategoriler = ListeSonucunuYansit(model, "Kategoriler", kategoriSonucu);

            var lokasyonSonucu = await envanterApiClient.LokasyonlariListeleAsync(token);
            model.Lokasyonlar = ListeSonucunuYansit(model, "Lokasyonlar", lokasyonSonucu);

            var cihazSonucu = await envanterApiClient.CihazlariListeleAsync(model.CihazFiltre, token);
            model.Cihazlar = ListeSonucunuYansit(model, "Cihazlar", cihazSonucu);

            var sarfSonucu = await envanterApiClient.SarfMalzemeleriListeleAsync(token);
            model.SarfMalzemeler = ListeSonucunuYansit(model, "Sarf malzemeler", sarfSonucu);

            var stokOzetSonucu = await envanterApiClient.StokOzetiniGetirAsync(token);
            if (stokOzetSonucu.BasariliMi && stokOzetSonucu.Veri is not null)
            {
                model.StokOzet = stokOzetSonucu.Veri;
            }
            else
            {
                model.ListelemeHatalari.Add(MvcMesajlari.ListeAlinamadi("Stok özeti", stokOzetSonucu.Hata));
            }
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> CihazIslemleri(Guid id)
    {
        var token = TokenAl();
        if (token is null)
        {
            return OturumYok();
        }

        var model = await CihazIslemleriModeliOlustur(id, token);
        if (model is null)
        {
            return RedirectToSekme("cihaz");
        }

        model.BasariMesaji = TempData[MvcSabitleri.BasariMesajiTempDataKey] as string;
        model.HataMesaji ??= TempData[MvcSabitleri.HataMesajiTempDataKey] as string;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> SarfMalzemeIslemleri(Guid id)
    {
        var token = TokenAl();
        if (token is null)
        {
            return OturumYok();
        }

        var model = await SarfMalzemeIslemleriModeliOlustur(id, token);
        if (model is null)
        {
            return RedirectToSekme("sarf");
        }

        model.BasariMesaji = TempData[MvcSabitleri.BasariMesajiTempDataKey] as string;
        model.HataMesaji ??= TempData[MvcSabitleri.HataMesajiTempDataKey] as string;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KategoriOlustur(KategoriOlusturFormModel form)
    {
        var token = TokenAl();
        if (token is null)
        {
            return OturumYok();
        }

        if (!ModelState.IsValid)
        {
            TempData[MvcSabitleri.HataMesajiTempDataKey] = MvcMesajlari.KategoriBilgileriHatali;
            return RedirectToSekme("kategori");
        }

        var sonuc = await envanterApiClient.KategoriOlusturAsync(form, token);
        IslemSonucunuYansit(sonuc, MvcMesajlari.KategoriOlusturuldu);
        return RedirectToSekme("kategori");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KategoriGuncelle(KategoriGuncelleFormModel form)
    {
        var token = TokenAl();
        if (token is null)
        {
            return OturumYok();
        }

        if (!ModelState.IsValid)
        {
            TempData[MvcSabitleri.HataMesajiTempDataKey] = MvcMesajlari.KategoriGuncellemeBilgileriHatali;
            return RedirectToSekme("kategori");
        }

        var sonuc = await envanterApiClient.KategoriGuncelleAsync(form, token);
        IslemSonucunuYansit(sonuc, form.AktifMi ? MvcMesajlari.KategoriGuncellendi : MvcMesajlari.KategoriPasiflestirildi);
        return RedirectToSekme("kategori");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LokasyonOlustur(LokasyonOlusturFormModel form)
    {
        var token = TokenAl();
        if (token is null)
        {
            return OturumYok();
        }

        if (!ModelState.IsValid)
        {
            TempData[MvcSabitleri.HataMesajiTempDataKey] = MvcMesajlari.LokasyonBilgileriHatali;
            return RedirectToSekme("lokasyon");
        }

        var sonuc = await envanterApiClient.LokasyonOlusturAsync(form, token);
        IslemSonucunuYansit(sonuc, MvcMesajlari.LokasyonOlusturuldu);
        return RedirectToSekme("lokasyon");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LokasyonGuncelle(LokasyonGuncelleFormModel form)
    {
        var token = TokenAl();
        if (token is null)
        {
            return OturumYok();
        }

        if (!ModelState.IsValid)
        {
            TempData[MvcSabitleri.HataMesajiTempDataKey] = MvcMesajlari.LokasyonGuncellemeBilgileriHatali;
            return RedirectToSekme("lokasyon");
        }

        var sonuc = await envanterApiClient.LokasyonGuncelleAsync(form, token);
        IslemSonucunuYansit(sonuc, form.AktifMi ? MvcMesajlari.LokasyonGuncellendi : MvcMesajlari.LokasyonPasiflestirildi);
        return RedirectToSekme("lokasyon");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CihazOlustur(CihazOlusturFormModel form)
    {
        var token = TokenAl();
        if (token is null)
        {
            return OturumYok();
        }

        if (!ModelState.IsValid)
        {
            TempData[MvcSabitleri.HataMesajiTempDataKey] = MvcMesajlari.CihazBilgileriHatali;
            return RedirectToSekme("cihaz");
        }

        var sonuc = await envanterApiClient.CihazOlusturAsync(form, token);
        IslemSonucunuYansit(sonuc, MvcMesajlari.CihazOlusturuldu);
        return RedirectToSekme("cihaz");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CihazGuncelle(CihazGuncelleFormModel form)
    {
        var token = TokenAl();
        if (token is null)
        {
            return OturumYok();
        }

        if (!ModelState.IsValid)
        {
            return await CihazIslemleriFormHatasi(form, token, MvcMesajlari.CihazGuncellemeBilgileriHatali);
        }

        var sonuc = await envanterApiClient.CihazGuncelleAsync(form, token);
        if (!sonuc.BasariliMi)
        {
            return await CihazIslemleriFormHatasi(form, token, sonuc.Hata ?? MvcMesajlari.CihazGuncellenemedi);
        }

        TempData[MvcSabitleri.BasariMesajiTempDataKey] = MvcMesajlari.CihazGuncellendi;
        return RedirectToAction(nameof(CihazIslemleri), new { id = form.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CihazDurumHareketi(CihazDurumHareketiFormModel form)
    {
        var token = TokenAl();
        if (token is null)
        {
            return OturumYok();
        }

        if (!ModelState.IsValid)
        {
            TempData[MvcSabitleri.HataMesajiTempDataKey] = MvcMesajlari.CihazDurumHareketiHatali;
            return RedirectToAction(nameof(CihazIslemleri), new { id = form.Id });
        }

        var sonuc = await envanterApiClient.CihazDurumHareketiIsleAsync(form, token);
        if (!sonuc.BasariliMi)
        {
            TempData[MvcSabitleri.HataMesajiTempDataKey] = sonuc.Hata;
            return RedirectToAction(nameof(CihazIslemleri), new { id = form.Id });
        }

        TempData[MvcSabitleri.BasariMesajiTempDataKey] = MvcMesajlari.CihazDurumHareketiIslendi;
        return RedirectToAction(nameof(CihazIslemleri), new { id = form.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SarfMalzemeOlustur(SarfMalzemeOlusturFormModel form)
    {
        var token = TokenAl();
        if (token is null)
        {
            return OturumYok();
        }

        if (!ModelState.IsValid)
        {
            TempData[MvcSabitleri.HataMesajiTempDataKey] = MvcMesajlari.SarfMalzemeBilgileriHatali;
            return RedirectToSekme("sarf");
        }

        var sonuc = await envanterApiClient.SarfMalzemeOlusturAsync(form, token);
        IslemSonucunuYansit(sonuc, MvcMesajlari.SarfMalzemeOlusturuldu);
        return RedirectToSekme("sarf");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SarfMalzemeGuncelle(SarfMalzemeGuncelleFormModel form)
    {
        var token = TokenAl();
        if (token is null)
        {
            return OturumYok();
        }

        if (!ModelState.IsValid)
        {
            return await SarfMalzemeIslemleriFormHatasi(form, token, MvcMesajlari.SarfMalzemeGuncellemeBilgileriHatali);
        }

        var sonuc = await envanterApiClient.SarfMalzemeGuncelleAsync(form, token);
        if (!sonuc.BasariliMi)
        {
            return await SarfMalzemeIslemleriFormHatasi(form, token, sonuc.Hata ?? MvcMesajlari.SarfMalzemeGuncellenemedi);
        }

        TempData[MvcSabitleri.BasariMesajiTempDataKey] = form.AktifMi ? MvcMesajlari.SarfMalzemeGuncellendi : MvcMesajlari.SarfMalzemePasiflestirildi;
        return RedirectToAction(nameof(SarfMalzemeIslemleri), new { id = form.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SarfMalzemeStokHareketi(SarfMalzemeStokHareketiFormModel form)
    {
        var token = TokenAl();
        if (token is null)
        {
            return OturumYok();
        }

        if (!ModelState.IsValid)
        {
            TempData[MvcSabitleri.HataMesajiTempDataKey] = MvcMesajlari.SarfStokHareketiHatali;
            return RedirectToAction(nameof(SarfMalzemeIslemleri), new { id = form.Id });
        }

        var sonuc = await envanterApiClient.SarfMalzemeStokHareketiIsleAsync(form, token);
        if (!sonuc.BasariliMi)
        {
            TempData[MvcSabitleri.HataMesajiTempDataKey] = sonuc.Hata;
            return RedirectToAction(nameof(SarfMalzemeIslemleri), new { id = form.Id });
        }

        TempData[MvcSabitleri.BasariMesajiTempDataKey] = MvcMesajlari.SarfStokHareketiIslendi;
        return RedirectToAction(nameof(SarfMalzemeIslemleri), new { id = form.Id });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        logger.LogError(MvcMesajlari.EnvanterHataSayfasiLogu, Activity.Current?.Id ?? HttpContext.TraceIdentifier);
        return View("~/Views/Shared/Error.cshtml", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private async Task<IActionResult> CihazIslemleriFormHatasi(CihazGuncelleFormModel form, string token, string hataMesaji)
    {
        var model = await CihazIslemleriModeliOlustur(form.Id, token);
        if (model is null)
        {
            TempData[MvcSabitleri.HataMesajiTempDataKey] = hataMesaji;
            return RedirectToSekme("cihaz");
        }

        model.Form = form;
        model.HataMesaji = hataMesaji;
        return View(nameof(CihazIslemleri), model);
    }

    private async Task<IActionResult> SarfMalzemeIslemleriFormHatasi(SarfMalzemeGuncelleFormModel form, string token, string hataMesaji)
    {
        var model = await SarfMalzemeIslemleriModeliOlustur(form.Id, token);
        if (model is null)
        {
            TempData[MvcSabitleri.HataMesajiTempDataKey] = hataMesaji;
            return RedirectToSekme("sarf");
        }

        model.Form = form;
        model.HataMesaji = hataMesaji;
        return View(nameof(SarfMalzemeIslemleri), model);
    }

    private async Task<CihazIslemleriSayfaModel?> CihazIslemleriModeliOlustur(Guid id, string token)
    {
        var cihazSonucu = await envanterApiClient.CihazGetirAsync(id, token);
        if (!cihazSonucu.BasariliMi || cihazSonucu.Veri is null)
        {
            TempData[MvcSabitleri.HataMesajiTempDataKey] = cihazSonucu.Hata;
            return null;
        }

        var kategoriler = await KategorileriGetir(token, VarlikTuruModel.SeriNumarali, cihazSonucu.Veri.KategoriId);
        var lokasyonlar = await LokasyonlariGetir(token, cihazSonucu.Veri.LokasyonId);
        var durumHareketleriSonucu = await envanterApiClient.StokHareketleriniListeleAsync(cihazSonucu.Veri.Id, null, token);

        return new CihazIslemleriSayfaModel
        {
            Form = new CihazGuncelleFormModel
            {
                Id = cihazSonucu.Veri.Id,
                SeriNumarasi = cihazSonucu.Veri.SeriNumarasi,
                AssetTag = cihazSonucu.Veri.AssetTag,
                Ad = cihazSonucu.Veri.Ad,
                Marka = cihazSonucu.Veri.Marka,
                Model = cihazSonucu.Veri.Model,
                KategoriId = cihazSonucu.Veri.KategoriId,
                LokasyonId = cihazSonucu.Veri.LokasyonId,
                Durum = cihazSonucu.Veri.Durum,
                EnvantereGirisTarihi = cihazSonucu.Veri.EnvantereGirisTarihi,
                EnvanterdenCikisTarihi = cihazSonucu.Veri.EnvanterdenCikisTarihi,
                EldenCikarmaTipi = cihazSonucu.Veri.EldenCikarmaTipi,
                EldenCikarmaAciklamasi = cihazSonucu.Veri.EldenCikarmaAciklamasi,
                SatilanKisiVeyaKurum = cihazSonucu.Veri.SatilanKisiVeyaKurum,
                AktifMi = cihazSonucu.Veri.AktifMi,
                ToplamVarligaDahilMi = cihazSonucu.Veri.ToplamVarligaDahilMi
            },
            DurumHareketi = new CihazDurumHareketiFormModel { Id = cihazSonucu.Veri.Id },
            DurumHareketleri = durumHareketleriSonucu.BasariliMi ? durumHareketleriSonucu.Veri : [],
            Kategoriler = kategoriler,
            Lokasyonlar = lokasyonlar,
            HataMesaji = durumHareketleriSonucu.BasariliMi
                ? null
                : MvcMesajlari.GecmisAlinamadi("Cihaz durum geçmişi", durumHareketleriSonucu.Hata)
        };
    }

    private async Task<SarfMalzemeIslemleriSayfaModel?> SarfMalzemeIslemleriModeliOlustur(Guid id, string token)
    {
        var sarfSonucu = await envanterApiClient.SarfMalzemeGetirAsync(id, token);
        if (!sarfSonucu.BasariliMi || sarfSonucu.Veri is null)
        {
            TempData[MvcSabitleri.HataMesajiTempDataKey] = sarfSonucu.Hata;
            return null;
        }

        var kategoriler = await KategorileriGetir(token, VarlikTuruModel.SarfMalzeme, sarfSonucu.Veri.KategoriId);
        var lokasyonlar = await LokasyonlariGetir(token, sarfSonucu.Veri.LokasyonId);
        var stokHareketleriSonucu = await envanterApiClient.StokHareketleriniListeleAsync(null, sarfSonucu.Veri.Id, token);

        return new SarfMalzemeIslemleriSayfaModel
        {
            Form = new SarfMalzemeGuncelleFormModel
            {
                Id = sarfSonucu.Veri.Id,
                Ad = sarfSonucu.Veri.Ad,
                KategoriId = sarfSonucu.Veri.KategoriId,
                LokasyonId = sarfSonucu.Veri.LokasyonId,
                EldekiMiktar = sarfSonucu.Veri.EldekiMiktar,
                KritikStokSeviyesi = sarfSonucu.Veri.KritikStokSeviyesi,
                Birim = sarfSonucu.Veri.Birim,
                AktifMi = sarfSonucu.Veri.AktifMi
            },
            StokHareketi = new SarfMalzemeStokHareketiFormModel { Id = sarfSonucu.Veri.Id },
            StokHareketleri = stokHareketleriSonucu.BasariliMi ? stokHareketleriSonucu.Veri : [],
            Kategoriler = kategoriler,
            Lokasyonlar = lokasyonlar,
            HataMesaji = stokHareketleriSonucu.BasariliMi
                ? null
                : MvcMesajlari.GecmisAlinamadi("Sarf malzeme stok hareketi geçmişi", stokHareketleriSonucu.Hata)
        };
    }

    private async Task<IReadOnlyCollection<KategoriModel>> KategorileriGetir(string token, VarlikTuruModel varlikTuru, Guid mevcutKategoriId)
    {
        var sonuc = await envanterApiClient.KategorileriListeleAsync(token);
        if (!sonuc.BasariliMi)
        {
            TempData[MvcSabitleri.HataMesajiTempDataKey] = sonuc.Hata;
            return [];
        }

        return sonuc.Veri
            .Where(kategori => (kategori.AktifMi && kategori.VarlikTuru == varlikTuru) || kategori.Id == mevcutKategoriId)
            .ToList();
    }

    private async Task<IReadOnlyCollection<LokasyonModel>> LokasyonlariGetir(string token, Guid mevcutLokasyonId)
    {
        var sonuc = await envanterApiClient.LokasyonlariListeleAsync(token);
        if (!sonuc.BasariliMi)
        {
            TempData[MvcSabitleri.HataMesajiTempDataKey] = sonuc.Hata;
            return [];
        }

        return sonuc.Veri
            .Where(lokasyon => lokasyon.AktifMi || lokasyon.Id == mevcutLokasyonId)
            .ToList();
    }

    private string? TokenAl()
        => HttpContext.Session.GetString(MvcSabitleri.TokenSessionKey);

    private IActionResult OturumYok()
    {
        TempData[MvcSabitleri.HataMesajiTempDataKey] = MvcMesajlari.KontrolPanelindenOturumYok;
        return RedirectToAction(nameof(Index));
    }

    private IActionResult RedirectToSekme(string sekme)
        => RedirectToAction(nameof(Index), new { sekme });

    private static string SekmeDogrula(string? sekme)
        => sekme is "kategori" or "lokasyon" or "cihaz" or "sarf" ? sekme : "stok";

    private static IReadOnlyCollection<T> ListeSonucunuYansit<T>(
        EnvanterPanelModel model,
        string listeAdi,
        ApiListeSonucu<T> sonuc)
    {
        if (sonuc.BasariliMi)
        {
            return sonuc.Veri;
        }

        model.ListelemeHatalari.Add(MvcMesajlari.ListeAlinamadi(listeAdi, sonuc.Hata));
        return [];
    }

    private void IslemSonucunuYansit<T>(ApiIslemSonucu<T> sonuc, string basariMesaji)
    {
        TempData[sonuc.BasariliMi ? MvcSabitleri.BasariMesajiTempDataKey : MvcSabitleri.HataMesajiTempDataKey] = sonuc.BasariliMi
            ? basariMesaji
            : sonuc.Hata;
    }
}
