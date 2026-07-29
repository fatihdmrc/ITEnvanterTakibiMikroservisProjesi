using KimlikVePersonelServisi.Api.Domain.Entities;
using KimlikVePersonelServisi.Api.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace KimlikVePersonelServisi.Api.Data;

public static class DemoVeriSeeder
{
    public static void Seed(
        KimlikPersonelDbContext dbContext,
        UserManager<UygulamaKullanici> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        foreach (var rol in Enum.GetNames<KullaniciRolu>())
        {
            if (!roleManager.RoleExistsAsync(rol).GetAwaiter().GetResult())
            {
                roleManager.CreateAsync(new IdentityRole<Guid>(rol)).GetAwaiter().GetResult();
            }
        }

        if (dbContext.Departmanlar.Any() || dbContext.Personeller.Any() || userManager.Users.Any())
        {
            return;
        }

        var bilgiIslem = new Departman { Ad = "Bilgi İşlem" };
        var insanKaynaklari = new Departman { Ad = "İnsan Kaynakları" };

        dbContext.Departmanlar.AddRange(bilgiIslem, insanKaynaklari);
        dbContext.SaveChanges();

        var adminPersonel = new Personel
        {
            Ad = "Ayşe",
            Soyad = "Yılmaz",
            Email = "ayse.yilmaz@example.com",
            DepartmanId = bilgiIslem.Id,
            Unvan = "IT Müdürü",
            DepartmanSorumlusuMu = true,
            IseGirisTarihi = new DateOnly(2024, 1, 15)
        };

        var itPersoneli = new Personel
        {
            Ad = "Mehmet",
            Soyad = "Kaya",
            Email = "mehmet.kaya@example.com",
            DepartmanId = bilgiIslem.Id,
            Unvan = "IT Uzmanı",
            IseGirisTarihi = new DateOnly(2025, 3, 10)
        };

        var personelKullanicisi = new Personel
        {
            Ad = "Elif",
            Soyad = "Demir",
            Email = "elif.demir@example.com",
            DepartmanId = insanKaynaklari.Id,
            Unvan = "İK Uzmanı",
            IseGirisTarihi = new DateOnly(2025, 6, 1)
        };

        dbContext.Personeller.AddRange(adminPersonel, itPersoneli, personelKullanicisi);
        dbContext.SaveChanges();

        bilgiIslem.SorumluPersonelId = adminPersonel.Id;
        insanKaynaklari.SorumluPersonelId = personelKullanicisi.Id;

        KullaniciOlustur(userManager, "admin", "Admin123!", KullaniciRolu.Admin, adminPersonel.Id);
        KullaniciOlustur(userManager, "it.personel", "ItPersonel123!", KullaniciRolu.ITPersoneli, itPersoneli.Id);
        KullaniciOlustur(userManager, "personel", "Personel123!", KullaniciRolu.PersonelKullanicisi, personelKullanicisi.Id);
    }

    private static void KullaniciOlustur(
        UserManager<UygulamaKullanici> userManager,
        string kullaniciAdi,
        string sifre,
        KullaniciRolu rol,
        Guid personelId)
    {
        var kullanici = new UygulamaKullanici
        {
            UserName = kullaniciAdi,
            PersonelId = personelId
        };

        var sonuc = userManager.CreateAsync(kullanici, sifre).GetAwaiter().GetResult();
        if (!sonuc.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", sonuc.Errors.Select(hata => hata.Description)));
        }

        var rolSonucu = userManager.AddToRoleAsync(kullanici, rol.ToString()).GetAwaiter().GetResult();
        if (!rolSonucu.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", rolSonucu.Errors.Select(hata => hata.Description)));
        }
    }
}
