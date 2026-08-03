using System.Diagnostics;
using EnvanterTakip.MvcClient.Models;
using EnvanterTakip.MvcClient.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnvanterTakip.MvcClient.Controllers;

public sealed class EnvanterController(
    ILogger<EnvanterController> logger,
    EnvanterApiClient envanterApiClient) : Controller
{
    private const string TokenSessionKey = "KimlikToken";

    public async Task<IActionResult> Index(string? sekme)
    {
        var token = TokenAl();
        var model = new EnvanterPanelModel
        {
            OturumVarMi = !string.IsNullOrWhiteSpace(token),
            AktifSekme = SekmeDogrula(sekme),
            BasariMesaji = TempData["BasariMesaji"] as string,
            HataMesaji = TempData["HataMesaji"] as string
        };

        if (!string.IsNullOrWhiteSpace(token))
        {
            var kategoriSonucu = await envanterApiClient.KategorileriListeleAsync(token);
            model.Kategoriler = ListeSonucunuYansit(model, "Kategoriler", kategoriSonucu);

            var lokasyonSonucu = await envanterApiClient.LokasyonlariListeleAsync(token);
            model.Lokasyonlar = ListeSonucunuYansit(model, "Lokasyonlar", lokasyonSonucu);

            var cihazSonucu = await envanterApiClient.CihazlariListeleAsync(token);
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
                model.ListelemeHatalari.Add($"Stok özeti alınamadı: {stokOzetSonucu.Hata}");
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

        model.BasariMesaji = TempData["BasariMesaji"] as string;
        model.HataMesaji ??= TempData["HataMesaji"] as string;
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

        model.BasariMesaji = TempData["BasariMesaji"] as string;
        model.HataMesaji ??= TempData["HataMesaji"] as string;
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
            TempData["HataMesaji"] = "Kategori bilgileri eksik veya hatalı.";
            return RedirectToSekme("kategori");
        }

        var sonuc = await envanterApiClient.KategoriOlusturAsync(form, token);
        IslemSonucunuYansit(sonuc, "Kategori oluşturuldu.");
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
            TempData["HataMesaji"] = "Kategori güncelleme bilgileri eksik veya hatalı.";
            return RedirectToSekme("kategori");
        }

        var sonuc = await envanterApiClient.KategoriGuncelleAsync(form, token);
        IslemSonucunuYansit(sonuc, form.AktifMi ? "Kategori güncellendi." : "Kategori pasifleştirildi.");
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
            TempData["HataMesaji"] = "Lokasyon bilgileri eksik veya hatalı.";
            return RedirectToSekme("lokasyon");
        }

        var sonuc = await envanterApiClient.LokasyonOlusturAsync(form, token);
        IslemSonucunuYansit(sonuc, "Lokasyon oluşturuldu.");
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
            TempData["HataMesaji"] = "Lokasyon güncelleme bilgileri eksik veya hatalı.";
            return RedirectToSekme("lokasyon");
        }

        var sonuc = await envanterApiClient.LokasyonGuncelleAsync(form, token);
        IslemSonucunuYansit(sonuc, form.AktifMi ? "Lokasyon güncellendi." : "Lokasyon pasifleştirildi.");
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
            TempData["HataMesaji"] = "Cihaz bilgileri eksik veya hatalı.";
            return RedirectToSekme("cihaz");
        }

        var sonuc = await envanterApiClient.CihazOlusturAsync(form, token);
        IslemSonucunuYansit(sonuc, "Cihaz oluşturuldu.");
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
            return await CihazIslemleriFormHatasi(form, token, "Cihaz güncelleme bilgileri eksik veya hatalı.");
        }

        var sonuc = await envanterApiClient.CihazGuncelleAsync(form, token);
        if (!sonuc.BasariliMi)
        {
            return await CihazIslemleriFormHatasi(form, token, sonuc.Hata ?? "Cihaz güncellenemedi.");
        }

        TempData["BasariMesaji"] = form.AktifMi ? "Cihaz güncellendi." : "Cihaz pasifleştirildi.";
        return RedirectToAction(nameof(CihazIslemleri), new { id = form.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CihazStokHareketi(CihazStokHareketiFormModel form)
    {
        var token = TokenAl();
        if (token is null)
        {
            return OturumYok();
        }

        if (!ModelState.IsValid)
        {
            TempData["HataMesaji"] = "Cihaz stok hareketi bilgileri eksik veya hatalı.";
            return RedirectToAction(nameof(CihazIslemleri), new { id = form.Id });
        }

        var sonuc = await envanterApiClient.CihazStokHareketiIsleAsync(form, token);
        if (!sonuc.BasariliMi)
        {
            TempData["HataMesaji"] = sonuc.Hata;
            return RedirectToAction(nameof(CihazIslemleri), new { id = form.Id });
        }

        TempData["BasariMesaji"] = "Cihaz stok hareketi işlendi.";
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
            TempData["HataMesaji"] = "Sarf malzeme bilgileri eksik veya hatalı.";
            return RedirectToSekme("sarf");
        }

        var sonuc = await envanterApiClient.SarfMalzemeOlusturAsync(form, token);
        IslemSonucunuYansit(sonuc, "Sarf malzeme oluşturuldu.");
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
            return await SarfMalzemeIslemleriFormHatasi(form, token, "Sarf malzeme güncelleme bilgileri eksik veya hatalı.");
        }

        var sonuc = await envanterApiClient.SarfMalzemeGuncelleAsync(form, token);
        if (!sonuc.BasariliMi)
        {
            return await SarfMalzemeIslemleriFormHatasi(form, token, sonuc.Hata ?? "Sarf malzeme güncellenemedi.");
        }

        TempData["BasariMesaji"] = form.AktifMi ? "Sarf malzeme güncellendi." : "Sarf malzeme pasifleştirildi.";
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
            TempData["HataMesaji"] = "Sarf malzeme stok hareketi bilgileri eksik veya hatalı.";
            return RedirectToAction(nameof(SarfMalzemeIslemleri), new { id = form.Id });
        }

        var sonuc = await envanterApiClient.SarfMalzemeStokHareketiIsleAsync(form, token);
        if (!sonuc.BasariliMi)
        {
            TempData["HataMesaji"] = sonuc.Hata;
            return RedirectToAction(nameof(SarfMalzemeIslemleri), new { id = form.Id });
        }

        TempData["BasariMesaji"] = "Sarf malzeme stok hareketi işlendi.";
        return RedirectToAction(nameof(SarfMalzemeIslemleri), new { id = form.Id });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        logger.LogError("Envanter MVC hata sayfası gösterildi. RequestId: {RequestId}", Activity.Current?.Id ?? HttpContext.TraceIdentifier);
        return View("~/Views/Shared/Error.cshtml", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private async Task<IActionResult> CihazIslemleriFormHatasi(CihazGuncelleFormModel form, string token, string hataMesaji)
    {
        var model = await CihazIslemleriModeliOlustur(form.Id, token);
        if (model is null)
        {
            TempData["HataMesaji"] = hataMesaji;
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
            TempData["HataMesaji"] = hataMesaji;
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
            TempData["HataMesaji"] = cihazSonucu.Hata;
            return null;
        }

        var kategoriler = await KategorileriGetir(token, VarlikTuruModel.SeriNumarali, cihazSonucu.Veri.KategoriId);
        var lokasyonlar = await LokasyonlariGetir(token, cihazSonucu.Veri.LokasyonId);

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
            StokHareketi = new CihazStokHareketiFormModel { Id = cihazSonucu.Veri.Id },
            Kategoriler = kategoriler,
            Lokasyonlar = lokasyonlar
        };
    }

    private async Task<SarfMalzemeIslemleriSayfaModel?> SarfMalzemeIslemleriModeliOlustur(Guid id, string token)
    {
        var sarfSonucu = await envanterApiClient.SarfMalzemeGetirAsync(id, token);
        if (!sarfSonucu.BasariliMi || sarfSonucu.Veri is null)
        {
            TempData["HataMesaji"] = sarfSonucu.Hata;
            return null;
        }

        var kategoriler = await KategorileriGetir(token, VarlikTuruModel.SarfMalzeme, sarfSonucu.Veri.KategoriId);
        var lokasyonlar = await LokasyonlariGetir(token, sarfSonucu.Veri.LokasyonId);

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
            Kategoriler = kategoriler,
            Lokasyonlar = lokasyonlar
        };
    }

    private async Task<IReadOnlyCollection<KategoriModel>> KategorileriGetir(string token, VarlikTuruModel varlikTuru, Guid mevcutKategoriId)
    {
        var sonuc = await envanterApiClient.KategorileriListeleAsync(token);
        if (!sonuc.BasariliMi)
        {
            TempData["HataMesaji"] = sonuc.Hata;
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
            TempData["HataMesaji"] = sonuc.Hata;
            return [];
        }

        return sonuc.Veri
            .Where(lokasyon => lokasyon.AktifMi || lokasyon.Id == mevcutLokasyonId)
            .ToList();
    }

    private string? TokenAl()
        => HttpContext.Session.GetString(TokenSessionKey);

    private IActionResult OturumYok()
    {
        TempData["HataMesaji"] = "Bu işlem için önce kontrol panelinden giriş yapmalısın.";
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

        model.ListelemeHatalari.Add($"{listeAdi} alınamadı: {sonuc.Hata}");
        return [];
    }

    private void IslemSonucunuYansit<T>(ApiIslemSonucu<T> sonuc, string basariMesaji)
    {
        TempData[sonuc.BasariliMi ? "BasariMesaji" : "HataMesaji"] = sonuc.BasariliMi
            ? basariMesaji
            : sonuc.Hata;
    }
}
