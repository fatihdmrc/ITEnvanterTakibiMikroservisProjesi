using System.Diagnostics;
using EnvanterTakip.MvcClient.Models;
using EnvanterTakip.MvcClient.Sabitler;
using EnvanterTakip.MvcClient.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnvanterTakip.MvcClient.Controllers;

public sealed class ZimmetController(
    ILogger<ZimmetController> logger,
    ZimmetApiClient zimmetApiClient,
    KimlikPersonelApiClient kimlikPersonelApiClient,
    EnvanterApiClient envanterApiClient) : Controller
{
    public async Task<IActionResult> Index()
    {
        var token = TokenAl();
        var model = new ZimmetPanelModel
        {
            OturumVarMi = !string.IsNullOrWhiteSpace(token),
            Rol = RolAl(),
            YonetimYetkisiVarMi = YonetimYetkisiVarMi(),
            BasariMesaji = TempData[MvcSabitleri.BasariMesajiTempDataKey] as string,
            HataMesaji = TempData[MvcSabitleri.HataMesajiTempDataKey] as string
        };

        if (string.IsNullOrWhiteSpace(token))
        {
            return View(model);
        }

        var zimmetSonucu = model.YonetimYetkisiVarMi
            ? await zimmetApiClient.ZimmetleriListeleAsync(token)
            : await zimmetApiClient.BenimZimmetlerimiListeleAsync(token);

        model.Zimmetler = ListeSonucunuYansit(model, "Zimmetler", zimmetSonucu);

        if (model.YonetimYetkisiVarMi)
        {
            var personelSonucu = await kimlikPersonelApiClient.ZimmetSecimiPersonelleriListeleAsync(token);
            var personeller = ListeSonucunuYansit(model, "Personeller", personelSonucu);
            model.AktifPersoneller = personeller
                .Where(personel => personel.AktifMi && personel.Durum == PersonelDurumuModel.Aktif)
                .OrderBy(personel => personel.Ad)
                .ThenBy(personel => personel.Soyad)
                .ToList();

            var cihazSonucu = await envanterApiClient.CihazlariListeleAsync(token);
            var cihazlar = ListeSonucunuYansit(model, "Cihazlar", cihazSonucu);
            model.KullanilabilirCihazlar = cihazlar
                .Where(cihaz => cihaz.AktifMi && cihaz.Durum == CihazDurumuModel.Kullanilabilir)
                .OrderBy(cihaz => cihaz.Ad)
                .ThenBy(cihaz => cihaz.AssetTag)
                .ToList();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ZimmetOlustur(ZimmetOlusturFormModel form)
    {
        var token = TokenAl();
        if (token is null)
        {
            return OturumYok();
        }

        if (!YonetimYetkisiVarMi())
        {
            TempData[MvcSabitleri.HataMesajiTempDataKey] = MvcMesajlari.ZimmetOlusturmaYetkisiYok;
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            TempData[MvcSabitleri.HataMesajiTempDataKey] = MvcMesajlari.ZimmetOlusturmaBilgileriHatali;
            return RedirectToAction(nameof(Index));
        }

        var sonuc = await zimmetApiClient.ZimmetOlusturAsync(form, token);
        TempData[sonuc.BasariliMi ? MvcSabitleri.BasariMesajiTempDataKey : MvcSabitleri.HataMesajiTempDataKey] = sonuc.BasariliMi
            ? MvcMesajlari.ZimmetOlusturuldu
            : sonuc.Hata;

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> IadeAl(Guid id)
    {
        var token = TokenAl();
        if (token is null)
        {
            return OturumYok();
        }

        if (!YonetimYetkisiVarMi())
        {
            TempData[MvcSabitleri.HataMesajiTempDataKey] = MvcMesajlari.ZimmetIadeYetkisiYok;
            return RedirectToAction(nameof(Index));
        }

        var zimmet = await ZimmetGetirVeyaYansit(id, token);
        if (zimmet is null)
        {
            return RedirectToAction(nameof(Index));
        }

        if (zimmet.Durum != ZimmetDurumuModel.Aktif)
        {
            TempData[MvcSabitleri.HataMesajiTempDataKey] = MvcMesajlari.SadeceAktifZimmetIadeSurecineAlinir;
            return RedirectToAction(nameof(Index));
        }

        return View(new ZimmetIadeAlSayfaModel
        {
            Zimmet = zimmet,
            Form = new ZimmetIadeAlindiFormModel { Id = zimmet.Id },
            HataMesaji = TempData[MvcSabitleri.HataMesajiTempDataKey] as string
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IadeAlOnayla(ZimmetIadeAlindiFormModel form)
    {
        var token = TokenAl();
        if (token is null)
        {
            return OturumYok();
        }

        if (!YonetimYetkisiVarMi())
        {
            TempData[MvcSabitleri.HataMesajiTempDataKey] = MvcMesajlari.ZimmetIadeYetkisiYok;
            return RedirectToAction(nameof(Index));
        }

        var sonuc = await zimmetApiClient.IadeAlindiAsync(form, token);
        TempData[sonuc.BasariliMi ? MvcSabitleri.BasariMesajiTempDataKey : MvcSabitleri.HataMesajiTempDataKey] = sonuc.BasariliMi
            ? MvcMesajlari.ZimmetIadesiAlindi
            : sonuc.Hata;

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> IadeKontrolu(Guid id)
    {
        var token = TokenAl();
        if (token is null)
        {
            return OturumYok();
        }

        if (!YonetimYetkisiVarMi())
        {
            TempData[MvcSabitleri.HataMesajiTempDataKey] = MvcMesajlari.IadeKontrolYetkisiYok;
            return RedirectToAction(nameof(Index));
        }

        var zimmet = await ZimmetGetirVeyaYansit(id, token);
        if (zimmet is null)
        {
            return RedirectToAction(nameof(Index));
        }

        if (zimmet.Durum != ZimmetDurumuModel.IadeSurecinde)
        {
            TempData[MvcSabitleri.HataMesajiTempDataKey] = MvcMesajlari.SadeceIadeSurecindekiZimmetKontrolEdilir;
            return RedirectToAction(nameof(Index));
        }

        return View(new ZimmetIadeKontrolSayfaModel
        {
            Zimmet = zimmet,
            Form = new ZimmetIadeKontroluFormModel { Id = zimmet.Id },
            HataMesaji = TempData[MvcSabitleri.HataMesajiTempDataKey] as string
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IadeKontroluTamamla(ZimmetIadeKontroluFormModel form)
    {
        var token = TokenAl();
        if (token is null)
        {
            return OturumYok();
        }

        if (!YonetimYetkisiVarMi())
        {
            TempData[MvcSabitleri.HataMesajiTempDataKey] = MvcMesajlari.IadeKontrolYetkisiYok;
            return RedirectToAction(nameof(Index));
        }

        var sonuc = await zimmetApiClient.IadeKontroluTamamlaAsync(form, token);
        TempData[sonuc.BasariliMi ? MvcSabitleri.BasariMesajiTempDataKey : MvcSabitleri.HataMesajiTempDataKey] = sonuc.BasariliMi
            ? MvcMesajlari.IadeKontroluTamamlandi
            : sonuc.Hata;

        return RedirectToAction(nameof(Index));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        logger.LogError(MvcMesajlari.ZimmetHataSayfasiLogu, Activity.Current?.Id ?? HttpContext.TraceIdentifier);
        return View("~/Views/Shared/Error.cshtml", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private async Task<ZimmetModel?> ZimmetGetirVeyaYansit(Guid id, string token)
    {
        var sonuc = await zimmetApiClient.ZimmetGetirAsync(id, token);
        if (sonuc.BasariliMi && sonuc.Veri is not null)
        {
            return sonuc.Veri;
        }

        TempData[MvcSabitleri.HataMesajiTempDataKey] = sonuc.Hata;
        return null;
    }

    private string? TokenAl()
        => HttpContext.Session.GetString(MvcSabitleri.TokenSessionKey);

    private string? RolAl()
        => HttpContext.Session.GetString(MvcSabitleri.RolSessionKey);

    private bool YonetimYetkisiVarMi()
    {
        var rol = RolAl();
        return rol is MvcSabitleri.AdminRolu or MvcSabitleri.ITPersoneliRolu;
    }

    private IActionResult OturumYok()
    {
        TempData[MvcSabitleri.HataMesajiTempDataKey] = MvcMesajlari.KontrolPanelindenOturumYok;
        return RedirectToAction(nameof(Index));
    }

    private static IReadOnlyCollection<T> ListeSonucunuYansit<T>(
        ZimmetPanelModel model,
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
}
