using EnvanterTakip.MvcClient.Models;
using EnvanterTakip.MvcClient.Sabitler;
using EnvanterTakip.MvcClient.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnvanterTakip.MvcClient.Controllers;

public sealed class DenetimController(DenetimApiClient denetimApiClient) : Controller
{
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
            HataMesaji = TempData[MvcSabitleri.HataMesajiTempDataKey] as string,
            Filtre = filtre
        };

        if (string.IsNullOrWhiteSpace(token))
        {
            model.HataMesaji ??= MvcMesajlari.DenetimOturumYok;
            return View(model);
        }

        if (!model.YonetimYetkisiVarMi)
        {
            model.HataMesaji ??= MvcMesajlari.DenetimYetkisiYok;
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
            TempData[MvcSabitleri.HataMesajiTempDataKey] = MvcMesajlari.DenetimDetayOturumYok;
            return RedirectToAction(nameof(Index));
        }

        if (!YonetimYetkisiVarMi())
        {
            TempData[MvcSabitleri.HataMesajiTempDataKey] = MvcMesajlari.DenetimDetayYetkisiYok;
            return RedirectToAction(nameof(Index));
        }

        var sonuc = await denetimApiClient.DenetimKaydiGetirAsync(id, token);
        if (!sonuc.BasariliMi || sonuc.Veri is null)
        {
            TempData[MvcSabitleri.HataMesajiTempDataKey] = sonuc.Hata;
            return RedirectToAction(nameof(Index));
        }

        return View(sonuc.Veri);
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
}
