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

    public async Task<IActionResult> Index()
    {
        var token = HttpContext.Session.GetString(TokenSessionKey);
        var model = new EnvanterPanelModel
        {
            OturumVarMi = !string.IsNullOrWhiteSpace(token),
            Kategoriler = await envanterApiClient.KategorileriListeleAsync(token),
            Lokasyonlar = await envanterApiClient.LokasyonlariListeleAsync(token),
            Cihazlar = await envanterApiClient.CihazlariListeleAsync(token),
            SarfMalzemeler = await envanterApiClient.SarfMalzemeleriListeleAsync(token),
            StokOzet = await envanterApiClient.StokOzetiniGetirAsync(token),
            BasariMesaji = TempData["BasariMesaji"] as string,
            HataMesaji = TempData["HataMesaji"] as string
        };

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

        var sonuc = await envanterApiClient.KategoriOlusturAsync(form, token);
        IslemSonucunuYansit(sonuc, "Kategori oluşturuldu.");
        return RedirectToAction(nameof(Index));
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

        var sonuc = await envanterApiClient.KategoriGuncelleAsync(form, token);
        IslemSonucunuYansit(sonuc, "Kategori güncellendi.");
        return RedirectToAction(nameof(Index));
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

        var sonuc = await envanterApiClient.LokasyonOlusturAsync(form, token);
        IslemSonucunuYansit(sonuc, "Lokasyon oluşturuldu.");
        return RedirectToAction(nameof(Index));
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

        var sonuc = await envanterApiClient.LokasyonGuncelleAsync(form, token);
        IslemSonucunuYansit(sonuc, "Lokasyon güncellendi.");
        return RedirectToAction(nameof(Index));
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

        var sonuc = await envanterApiClient.CihazOlusturAsync(form, token);
        IslemSonucunuYansit(sonuc, "Cihaz oluşturuldu.");
        return RedirectToAction(nameof(Index));
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

        var sonuc = await envanterApiClient.CihazGuncelleAsync(form, token);
        IslemSonucunuYansit(sonuc, "Cihaz güncellendi.");
        return RedirectToAction(nameof(Index));
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

        var sonuc = await envanterApiClient.CihazStokHareketiIsleAsync(form, token);
        IslemSonucunuYansit(sonuc, "Cihaz stok hareketi işlendi.");
        return RedirectToAction(nameof(Index));
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

        var sonuc = await envanterApiClient.SarfMalzemeOlusturAsync(form, token);
        IslemSonucunuYansit(sonuc, "Sarf malzeme oluşturuldu.");
        return RedirectToAction(nameof(Index));
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

        var sonuc = await envanterApiClient.SarfMalzemeGuncelleAsync(form, token);
        IslemSonucunuYansit(sonuc, "Sarf malzeme güncellendi.");
        return RedirectToAction(nameof(Index));
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

        var sonuc = await envanterApiClient.SarfMalzemeStokHareketiIsleAsync(form, token);
        IslemSonucunuYansit(sonuc, "Sarf malzeme stok hareketi işlendi.");
        return RedirectToAction(nameof(Index));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        logger.LogError("Envanter MVC hata sayfası gösterildi. RequestId: {RequestId}", Activity.Current?.Id ?? HttpContext.TraceIdentifier);
        return View("~/Views/Shared/Error.cshtml", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private string? TokenAl()
        => HttpContext.Session.GetString(TokenSessionKey);

    private IActionResult OturumYok()
    {
        TempData["HataMesaji"] = "Bu işlem için önce kontrol panelinden giriş yapmalısın.";
        return RedirectToAction(nameof(Index));
    }

    private void IslemSonucunuYansit<T>(ApiIslemSonucu<T> sonuc, string basariMesaji)
    {
        TempData[sonuc.BasariliMi ? "BasariMesaji" : "HataMesaji"] = sonuc.BasariliMi
            ? basariMesaji
            : sonuc.Hata;
    }
}
