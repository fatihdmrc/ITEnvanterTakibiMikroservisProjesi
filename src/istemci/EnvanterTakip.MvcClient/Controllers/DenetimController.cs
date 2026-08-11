using EnvanterTakip.MvcClient.Models;
using EnvanterTakip.MvcClient.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnvanterTakip.MvcClient.Controllers;

public sealed class DenetimController(DenetimApiClient denetimApiClient) : Controller
{
    private const string TokenSessionKey = "KimlikToken";
    private const string RolSessionKey = "Rol";

    public async Task<IActionResult> Index([FromQuery] DenetimFiltreModel filtre)
    {
        filtre.Sayfa = Math.Max(filtre.Sayfa, 1);
        filtre.SayfaBoyutu = filtre.SayfaBoyutu <= 0 ? 25 : Math.Clamp(filtre.SayfaBoyutu, 1, 100);

        var token = TokenAl();
        var model = new DenetimPanelModel
        {
            OturumVarMi = !string.IsNullOrWhiteSpace(token),
            YonetimYetkisiVarMi = YonetimYetkisiVarMi(),
            Rol = RolAl(),
            HataMesaji = TempData["HataMesaji"] as string,
            Filtre = filtre
        };

        if (string.IsNullOrWhiteSpace(token))
        {
            model.HataMesaji ??= "Denetim kayitlarini gormek icin once kontrol panelinden giris yapmalisin.";
            return View(model);
        }

        if (!model.YonetimYetkisiVarMi)
        {
            model.HataMesaji ??= "Denetim kayitlarini yalnizca Admin veya ITPersoneli rolu gorebilir.";
            return View(model);
        }

        var sonuc = await denetimApiClient.DenetimKayitlariniListeleAsync(filtre, token);
        if (sonuc.BasariliMi && sonuc.Veri is not null)
        {
            model.Liste = sonuc.Veri;
        }
        else
        {
            model.HataMesaji = sonuc.Hata;
        }

        return View(model);
    }

    public async Task<IActionResult> Detay(string id)
    {
        var token = TokenAl();
        if (string.IsNullOrWhiteSpace(token))
        {
            TempData["HataMesaji"] = "Denetim kaydini gormek icin once giris yapmalisin.";
            return RedirectToAction(nameof(Index));
        }

        if (!YonetimYetkisiVarMi())
        {
            TempData["HataMesaji"] = "Denetim kaydini goruntulemek icin yetkin yok.";
            return RedirectToAction(nameof(Index));
        }

        var sonuc = await denetimApiClient.DenetimKaydiGetirAsync(id, token);
        if (!sonuc.BasariliMi || sonuc.Veri is null)
        {
            TempData["HataMesaji"] = sonuc.Hata;
            return RedirectToAction(nameof(Index));
        }

        return View(sonuc.Veri);
    }

    private string? TokenAl()
        => HttpContext.Session.GetString(TokenSessionKey);

    private string? RolAl()
        => HttpContext.Session.GetString(RolSessionKey);

    private bool YonetimYetkisiVarMi()
    {
        var rol = RolAl();
        return rol is "Admin" or "ITPersoneli";
    }
}
