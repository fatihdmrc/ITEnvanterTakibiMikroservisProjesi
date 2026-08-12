using System.Diagnostics;
using EnvanterTakip.MvcClient.Models;
using EnvanterTakip.MvcClient.Sabitler;
using EnvanterTakip.MvcClient.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnvanterTakip.MvcClient.Controllers;

public class HomeController(
    ILogger<HomeController> logger,
    KimlikPersonelApiClient kimlikPersonelApiClient) : Controller
{
    public async Task<IActionResult> Index(string? personelArama, Guid? personelDepartmanId, string? sekme)
    {
        var model = await PanelModeliOlustur(personelArama, personelDepartmanId, sekme);

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

        HttpContext.Session.SetString(MvcSabitleri.TokenSessionKey, sonuc.Veri.Token);
        HttpContext.Session.SetString(MvcSabitleri.KullaniciAdiSessionKey, form.KullaniciAdi);
        HttpContext.Session.SetString(MvcSabitleri.KullaniciIdSessionKey, sonuc.Veri.KullaniciId.ToString());
        HttpContext.Session.SetString(MvcSabitleri.PersonelIdSessionKey, sonuc.Veri.PersonelId.ToString());
        HttpContext.Session.SetString(MvcSabitleri.RolSessionKey, sonuc.Veri.Rol.ToString());

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
        var token = TokenAl();
        if (token is null)
        {
            return OturumYok();
        }

        if (!ModelState.IsValid)
        {
            TempData["HataMesaji"] = "Departman bilgileri eksik veya hatalı.";
            return RedirectToAction(nameof(Index));
        }

        var sonuc = await kimlikPersonelApiClient.DepartmanOlusturAsync(form, token);
        IslemSonucunuYansit(sonuc, "Departman oluşturuldu.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DepartmanGuncelle(DepartmanGuncelleFormModel form)
    {
        var token = TokenAl();
        if (token is null)
        {
            return OturumYok();
        }

        if (!ModelState.IsValid)
        {
            TempData["HataMesaji"] = "Departman güncelleme bilgileri eksik veya hatalı.";
            return RedirectToAction(nameof(Index));
        }

        var sonuc = await kimlikPersonelApiClient.DepartmanGuncelleAsync(form, token);
        IslemSonucunuYansit(sonuc, form.AktifMi ? "Departman güncellendi." : "Departman pasifleştirildi.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PersonelOlustur(PersonelOlusturFormModel form)
    {
        var token = TokenAl();
        if (token is null)
        {
            return OturumYok();
        }

        if (!ModelState.IsValid)
        {
            TempData["HataMesaji"] = "Personel bilgileri eksik veya hatalı.";
            return RedirectToPersonelSekmesi();
        }

        var sonuc = await kimlikPersonelApiClient.PersonelOlusturAsync(form, token);
        IslemSonucunuYansit(sonuc, "Personel oluşturuldu.");
        return RedirectToPersonelSekmesi();
    }

    [HttpGet]
    public async Task<IActionResult> PersonelDuzenle(Guid id)
    {
        var token = TokenAl();
        if (token is null)
        {
            return OturumYok();
        }

        var model = await PersonelDuzenleModeliOlustur(id, token);
        if (model is null)
        {
            return RedirectToPersonelSekmesi();
        }

        model.HataMesaji = TempData["HataMesaji"] as string;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PersonelDuzenle(PersonelDuzenleSayfaModel model)
    {
        var token = TokenAl();
        if (token is null)
        {
            return OturumYok();
        }

        if (!ModelState.IsValid)
        {
            model.Departmanlar = await AktifDepartmanlariGetir(token);
            model.HataMesaji = "Personel güncelleme bilgileri eksik veya hatalı.";
            return View(model);
        }

        var sonuc = await kimlikPersonelApiClient.PersonelGuncelleAsync(model.Form, token);
        if (!sonuc.BasariliMi)
        {
            model.Departmanlar = await AktifDepartmanlariGetir(token);
            model.HataMesaji = sonuc.Hata;
            return View(model);
        }

        TempData["BasariMesaji"] = $"{model.Form.Ad} {model.Form.Soyad} personeli güncellendi.";
        return RedirectToPersonelSekmesi();
    }

    [HttpGet]
    public async Task<IActionResult> PersonelIstenAyrilOnay(Guid id)
    {
        var token = TokenAl();
        if (token is null)
        {
            return OturumYok();
        }

        var model = await PersonelIstenAyrilOnayModeliOlustur(id, token);
        if (model is null)
        {
            return RedirectToPersonelSekmesi();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PersonelIstenAyrilOnayla(Guid id)
    {
        var token = TokenAl();
        if (token is null)
        {
            return OturumYok();
        }

        var model = await PersonelIstenAyrilOnayModeliOlustur(id, token);
        if (model is null)
        {
            return RedirectToPersonelSekmesi();
        }

        var sonuc = await kimlikPersonelApiClient.PersoneliIstenAyrildiYapAsync(id, token);
        if (!sonuc.BasariliMi)
        {
            TempData["HataMesaji"] = sonuc.Hata;
            return RedirectToPersonelSekmesi();
        }

        TempData["BasariMesaji"] = $"{model.AdSoyad} {model.DepartmanAdi} personeli işten ayrıldı yapıldı.";
        return RedirectToPersonelSekmesi();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KullaniciOlustur(KullaniciOlusturFormModel form)
    {
        var token = TokenAl();
        if (token is null)
        {
            return OturumYok();
        }

        if (!ModelState.IsValid)
        {
            TempData["HataMesaji"] = "Kullanıcı bilgileri eksik veya hatalı.";
            return RedirectToKullaniciSekmesi();
        }

        var sonuc = await kimlikPersonelApiClient.KullaniciOlusturAsync(form, token);
        IslemSonucunuYansit(sonuc, "Kullanıcı oluşturuldu.");
        return RedirectToKullaniciSekmesi();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        logger.LogError("MVC client hata sayfası gösterildi. RequestId: {RequestId}", Activity.Current?.Id ?? HttpContext.TraceIdentifier);
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private async Task<KimlikPersonelPanelModel> PanelModeliOlustur(string? personelArama, Guid? personelDepartmanId, string? sekme)
    {
        var token = TokenAl();
        var model = new KimlikPersonelPanelModel
        {
            AktifSekme = SekmeDogrula(sekme),
            PersonelArama = personelArama?.Trim(),
            PersonelDepartmanId = personelDepartmanId
        };

        if (!string.IsNullOrWhiteSpace(token))
        {
            model.OturumKullanici = new OturumKullaniciModel(
                HttpContext.Session.GetString(MvcSabitleri.KullaniciIdSessionKey),
                HttpContext.Session.GetString(MvcSabitleri.KullaniciAdiSessionKey),
                HttpContext.Session.GetString(MvcSabitleri.PersonelIdSessionKey),
                HttpContext.Session.GetString(MvcSabitleri.RolSessionKey));

            var departmanSonucu = await kimlikPersonelApiClient.DepartmanlariListeleAsync(token);
            model.Departmanlar = ListeSonucunuYansit(model, "Departmanlar", departmanSonucu);

            var personelSonucu = await kimlikPersonelApiClient.PersonelleriListeleAsync(token);
            model.Personeller = ListeSonucunuYansit(model, "Personeller", personelSonucu);

            var kullaniciSonucu = await kimlikPersonelApiClient.KullanicilariListeleAsync(token);
            model.Kullanicilar = ListeSonucunuYansit(model, "Kullanıcılar", kullaniciSonucu);
        }

        model.FiltreliPersoneller = PersonelleriFiltrele(model.Personeller, model.PersonelArama, model.PersonelDepartmanId);
        return model;
    }

    private async Task<PersonelDuzenleSayfaModel?> PersonelDuzenleModeliOlustur(Guid id, string token)
    {
        var personelSonucu = await kimlikPersonelApiClient.PersonelGetirAsync(id, token);
        if (!personelSonucu.BasariliMi || personelSonucu.Veri is null)
        {
            TempData["HataMesaji"] = personelSonucu.Hata;
            return null;
        }

        return new PersonelDuzenleSayfaModel
        {
            Form = new PersonelGuncelleFormModel
            {
                Id = personelSonucu.Veri.Id,
                Ad = personelSonucu.Veri.Ad,
                Soyad = personelSonucu.Veri.Soyad,
                Email = personelSonucu.Veri.Email,
                DepartmanId = personelSonucu.Veri.DepartmanId,
                Unvan = personelSonucu.Veri.Unvan,
                DepartmanSorumlusuMu = personelSonucu.Veri.DepartmanSorumlusuMu,
                Durum = personelSonucu.Veri.Durum,
                AktifMi = personelSonucu.Veri.AktifMi
            },
            Departmanlar = await AktifDepartmanlariGetir(token, personelSonucu.Veri.DepartmanId)
        };
    }

    private async Task<PersonelIstenAyrilOnayModel?> PersonelIstenAyrilOnayModeliOlustur(Guid id, string token)
    {
        var personelSonucu = await kimlikPersonelApiClient.PersonelGetirAsync(id, token);
        if (!personelSonucu.BasariliMi || personelSonucu.Veri is null)
        {
            TempData["HataMesaji"] = personelSonucu.Hata;
            return null;
        }

        var departmanlar = await AktifDepartmanlariGetir(token, personelSonucu.Veri.DepartmanId);
        var departmanAdi = departmanlar.FirstOrDefault(departman => departman.Id == personelSonucu.Veri.DepartmanId)?.Ad ?? "-";

        return new PersonelIstenAyrilOnayModel
        {
            Id = personelSonucu.Veri.Id,
            AdSoyad = $"{personelSonucu.Veri.Ad} {personelSonucu.Veri.Soyad}",
            DepartmanAdi = departmanAdi,
            Email = personelSonucu.Veri.Email,
            Unvan = personelSonucu.Veri.Unvan,
            Durum = personelSonucu.Veri.Durum
        };
    }

    private async Task<IReadOnlyCollection<DepartmanModel>> AktifDepartmanlariGetir(string token, Guid? mevcutDepartmanId = null)
    {
        var departmanSonucu = await kimlikPersonelApiClient.DepartmanlariListeleAsync(token);
        if (!departmanSonucu.BasariliMi)
        {
            TempData["HataMesaji"] = departmanSonucu.Hata;
            return [];
        }

        return departmanSonucu.Veri
            .Where(departman => departman.AktifMi || departman.Id == mevcutDepartmanId)
            .ToList();
    }

    private static IReadOnlyCollection<PersonelModel> PersonelleriFiltrele(
        IReadOnlyCollection<PersonelModel> personeller,
        string? arama,
        Guid? departmanId)
    {
        var filtreli = personeller.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(arama))
        {
            filtreli = filtreli.Where(personel =>
                personel.Ad.Contains(arama, StringComparison.OrdinalIgnoreCase)
                || personel.Soyad.Contains(arama, StringComparison.OrdinalIgnoreCase)
                || $"{personel.Ad} {personel.Soyad}".Contains(arama, StringComparison.OrdinalIgnoreCase)
                || personel.Email.Contains(arama, StringComparison.OrdinalIgnoreCase));
        }

        if (departmanId.HasValue)
        {
            filtreli = filtreli.Where(personel => personel.DepartmanId == departmanId.Value);
        }

        return filtreli.ToList();
    }

    private static string SekmeDogrula(string? sekme)
        => sekme is "personel" or "kullanici" ? sekme : "departman";

    private string? TokenAl()
        => HttpContext.Session.GetString(MvcSabitleri.TokenSessionKey);

    private IActionResult OturumYok()
    {
        TempData["HataMesaji"] = "Bu işlem için önce giriş yapmalısın.";
        return RedirectToAction(nameof(Index));
    }

    private IActionResult RedirectToPersonelSekmesi()
        => RedirectToAction(nameof(Index), new { sekme = "personel" });

    private IActionResult RedirectToKullaniciSekmesi()
        => RedirectToAction(nameof(Index), new { sekme = "kullanici" });

    private static IReadOnlyCollection<T> ListeSonucunuYansit<T>(
        KimlikPersonelPanelModel model,
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
