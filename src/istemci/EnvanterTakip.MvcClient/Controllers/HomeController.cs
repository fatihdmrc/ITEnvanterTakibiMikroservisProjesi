using System.Diagnostics;
using EnvanterTakip.MvcClient.Models;
using EnvanterTakip.MvcClient.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnvanterTakip.MvcClient.Controllers;

public class HomeController(
    ILogger<HomeController> logger,
    KimlikPersonelApiClient kimlikPersonelApiClient) : Controller
{
    private const string TokenSessionKey = "KimlikToken";
    private const string KullaniciAdiSessionKey = "KullaniciAdi";
    private const string KullaniciIdSessionKey = "KullaniciId";
    private const string PersonelIdSessionKey = "PersonelId";
    private const string RolSessionKey = "Rol";

    public async Task<IActionResult> Index()
    {
        var model = await PanelModeliOlustur();

        model.BasariMesaji = TempData["BasariMesaji"] as string;
        model.HataMesaji = TempData["HataMesaji"] as string;

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Giris(GirisFormModel form)
    {
        if (!ModelState.IsValid)
        {
            TempData["HataMesaji"] = "Kullanıcı adı ve şifre girilmelidir.";
            return RedirectToAction(nameof(Index));
        }

        var sonuc = await kimlikPersonelApiClient.GirisYapAsync(form);
        if (!sonuc.BasariliMi || sonuc.Veri is null)
        {
            TempData["HataMesaji"] = sonuc.Hata;
            return RedirectToAction(nameof(Index));
        }

        HttpContext.Session.SetString(TokenSessionKey, sonuc.Veri.Token);
        HttpContext.Session.SetString(KullaniciAdiSessionKey, form.KullaniciAdi);
        HttpContext.Session.SetString(KullaniciIdSessionKey, sonuc.Veri.KullaniciId.ToString());
        HttpContext.Session.SetString(PersonelIdSessionKey, sonuc.Veri.PersonelId.ToString());
        HttpContext.Session.SetString(RolSessionKey, sonuc.Veri.Rol.ToString());

        TempData["BasariMesaji"] = $"{form.KullaniciAdi} kullanıcısı ile giriş yapıldı.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Cikis()
    {
        HttpContext.Session.Clear();
        TempData["BasariMesaji"] = "Oturum kapatıldı.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DepartmanOlustur(DepartmanOlusturFormModel form)
    {
        var token = HttpContext.Session.GetString(TokenSessionKey);
        if (string.IsNullOrWhiteSpace(token))
        {
            TempData["HataMesaji"] = "Bu işlem için önce giriş yapmalısın.";
            return RedirectToAction(nameof(Index));
        }

        var sonuc = await kimlikPersonelApiClient.DepartmanOlusturAsync(form, token);
        TempData[sonuc.BasariliMi ? "BasariMesaji" : "HataMesaji"] = sonuc.BasariliMi
            ? "Departman oluşturuldu."
            : sonuc.Hata;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PersonelOlustur(PersonelOlusturFormModel form)
    {
        var token = HttpContext.Session.GetString(TokenSessionKey);
        if (string.IsNullOrWhiteSpace(token))
        {
            TempData["HataMesaji"] = "Bu işlem için önce giriş yapmalısın.";
            return RedirectToAction(nameof(Index));
        }

        var sonuc = await kimlikPersonelApiClient.PersonelOlusturAsync(form, token);
        TempData[sonuc.BasariliMi ? "BasariMesaji" : "HataMesaji"] = sonuc.BasariliMi
            ? "Personel oluşturuldu."
            : sonuc.Hata;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KullaniciOlustur(KullaniciOlusturFormModel form)
    {
        var token = HttpContext.Session.GetString(TokenSessionKey);
        if (string.IsNullOrWhiteSpace(token))
        {
            TempData["HataMesaji"] = "Bu işlem için önce giriş yapmalısın.";
            return RedirectToAction(nameof(Index));
        }

        var sonuc = await kimlikPersonelApiClient.KullaniciOlusturAsync(form, token);
        TempData[sonuc.BasariliMi ? "BasariMesaji" : "HataMesaji"] = sonuc.BasariliMi
            ? "Kullanıcı oluşturuldu."
            : sonuc.Hata;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PersoneliIstenAyrildiYap(Guid id)
    {
        var token = HttpContext.Session.GetString(TokenSessionKey);
        if (string.IsNullOrWhiteSpace(token))
        {
            TempData["HataMesaji"] = "Bu işlem için önce giriş yapmalısın.";
            return RedirectToAction(nameof(Index));
        }

        var sonuc = await kimlikPersonelApiClient.PersoneliIstenAyrildiYapAsync(id, token);
        TempData[sonuc.BasariliMi ? "BasariMesaji" : "HataMesaji"] = sonuc.BasariliMi
            ? "Personel işten ayrıldı olarak işaretlendi."
            : sonuc.Hata;

        return RedirectToAction(nameof(Index));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        logger.LogError("MVC client hata sayfası gösterildi. RequestId: {RequestId}", Activity.Current?.Id ?? HttpContext.TraceIdentifier);
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private async Task<KimlikPersonelPanelModel> PanelModeliOlustur()
    {
        var token = HttpContext.Session.GetString(TokenSessionKey);

        // MVC ekranı API'nin durumunu yansıtır; servis kapalıysa listeler boş gelir ve kullanıcı bilgilendirilir.
        var model = new KimlikPersonelPanelModel
        {
            Departmanlar = await kimlikPersonelApiClient.DepartmanlariListeleAsync(token),
            Personeller = await kimlikPersonelApiClient.PersonelleriListeleAsync(token),
            Kullanicilar = await kimlikPersonelApiClient.KullanicilariListeleAsync(token)
        };

        if (!string.IsNullOrWhiteSpace(token))
        {
            model.OturumKullanici = new OturumKullaniciModel(
                HttpContext.Session.GetString(KullaniciIdSessionKey),
                HttpContext.Session.GetString(KullaniciAdiSessionKey),
                HttpContext.Session.GetString(PersonelIdSessionKey),
                HttpContext.Session.GetString(RolSessionKey));
        }

        return model;
    }
}
