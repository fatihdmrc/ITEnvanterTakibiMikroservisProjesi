using KimlikVePersonelServisi.Api.Domain.Entities;
using KimlikVePersonelServisi.Api.Domain.Enums;
using KimlikVePersonelServisi.Api.Services;

namespace KimlikVePersonelServisi.Api.Data;

public static class DemoVeriSeeder
{
    public static void Seed(KimlikPersonelDbContext dbContext, ISifreServisi sifreServisi)
    {
        if (dbContext.Departmanlar.Any() || dbContext.Personeller.Any() || dbContext.Kullanicilar.Any())
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

        dbContext.Kullanicilar.AddRange(
            new Kullanici
            {
                KullaniciAdi = "admin",
                SifreHash = sifreServisi.HashOlustur("Admin123!"),
                Rol = KullaniciRolu.Admin,
                PersonelId = adminPersonel.Id
            },
            new Kullanici
            {
                KullaniciAdi = "it.personel",
                SifreHash = sifreServisi.HashOlustur("ItPersonel123!"),
                Rol = KullaniciRolu.ITPersoneli,
                PersonelId = itPersoneli.Id
            },
            new Kullanici
            {
                KullaniciAdi = "personel",
                SifreHash = sifreServisi.HashOlustur("Personel123!"),
                Rol = KullaniciRolu.PersonelKullanicisi,
                PersonelId = personelKullanicisi.Id
            });

        
        dbContext.SaveChanges();
    }
}
