using KimlikVePersonelServisi.Api.Domain.Entities;
using KimlikVePersonelServisi.Api.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KimlikVePersonelServisi.Api.Data;

public static class DemoVeriSeeder
{
    public static async Task SeedAsync(
        KimlikPersonelDbContext dbContext,
        UserManager<UygulamaKullanici> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        bool sifirla = false,
        CancellationToken cancellationToken = default)
    {
        if (sifirla)
        {
            await DemoVerileriniTemizleAsync(dbContext, cancellationToken);
        }

        foreach (var rol in Enum.GetNames<KullaniciRolu>())
        {
            if (!await roleManager.RoleExistsAsync(rol))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(rol));
            }
        }

        if (await dbContext.Departmanlar.AnyAsync(cancellationToken) ||
            await dbContext.Personeller.AnyAsync(cancellationToken) ||
            await userManager.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var departmanlar = DepartmanlariOlustur();
        dbContext.Departmanlar.AddRange(departmanlar);
        await dbContext.SaveChangesAsync(cancellationToken);

        var personeller = PersonelleriOlustur(departmanlar);
        dbContext.Personeller.AddRange(personeller);
        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var departman in departmanlar)
        {
            departman.SorumluPersonelId = personeller.FirstOrDefault(personel =>
                personel.DepartmanId == departman.Id &&
                personel.DepartmanSorumlusuMu)?.Id;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await KullaniciOlusturAsync(userManager, "admin", "Admin123!", KullaniciRolu.Admin, DemoKimlikleri.AyseYilmaz);
        await KullaniciOlusturAsync(userManager, "it.personel", "ItPersonel123!", KullaniciRolu.ITPersoneli, DemoKimlikleri.MehmetKaya);
        await KullaniciOlusturAsync(userManager, "personel", "Personel123!", KullaniciRolu.PersonelKullanicisi, DemoKimlikleri.ElifDemir);
        await KullaniciOlusturAsync(userManager, "it.destek", "ItDestek123!", KullaniciRolu.ITPersoneli, DemoKimlikleri.CemArslan);
        await KullaniciOlusturAsync(userManager, "finans.user", "Finans123!", KullaniciRolu.PersonelKullanicisi, DemoKimlikleri.ZeynepAksoy);
    }

    private static List<Departman> DepartmanlariOlustur()
        =>
        [
            new() { Id = DemoKimlikleri.BilgiIslem, Ad = "Bilgi İşlem" },
            new() { Id = DemoKimlikleri.InsanKaynaklari, Ad = "İnsan Kaynakları" },
            new() { Id = DemoKimlikleri.Finans, Ad = "Finans" },
            new() { Id = DemoKimlikleri.Satis, Ad = "Satış" },
            new() { Id = DemoKimlikleri.Pazarlama, Ad = "Pazarlama" },
            new() { Id = DemoKimlikleri.Operasyon, Ad = "Operasyon" },
            new() { Id = DemoKimlikleri.Lojistik, Ad = "Lojistik" },
            new() { Id = DemoKimlikleri.Hukuk, Ad = "Hukuk" },
            new() { Id = DemoKimlikleri.Uretim, Ad = "Üretim" },
            new() { Id = DemoKimlikleri.Yonetim, Ad = "Yönetim" }
        ];

    private static List<Personel> PersonelleriOlustur(IReadOnlyCollection<Departman> departmanlar)
    {
        Guid DepartmanId(string ad) => departmanlar.Single(departman => departman.Ad == ad).Id;

        return
        [
            Personel(DemoKimlikleri.AyseYilmaz, "Ayşe", "Yılmaz", "ayse.yilmaz@example.com", "Bilgi İşlem", "IT Müdürü", true, new DateOnly(2021, 1, 15)),
            Personel(DemoKimlikleri.MehmetKaya, "Mehmet", "Kaya", "mehmet.kaya@example.com", "Bilgi İşlem", "Kıdemli Sistem Uzmanı", false, new DateOnly(2022, 3, 10)),
            Personel(DemoKimlikleri.CemArslan, "Cem", "Arslan", "cem.arslan@example.com", "Bilgi İşlem", "Destek Uzmanı", false, new DateOnly(2024, 2, 5)),
            Personel(DemoKimlikleri.ElifDemir, "Elif", "Demir", "elif.demir@example.com", "İnsan Kaynakları", "İK Uzmanı", true, new DateOnly(2023, 6, 1)),
            Personel(DemoKimlikleri.ZeynepAksoy, "Zeynep", "Aksoy", "zeynep.aksoy@example.com", "Finans", "Finans Uzmanı", true, new DateOnly(2022, 9, 12)),
            Personel(DemoKimlikleri.BurakCelik, "Burak", "Çelik", "burak.celik@example.com", "Satış", "Satış Temsilcisi", true, new DateOnly(2023, 4, 3)),
            Personel(DemoKimlikleri.DeryaKurt, "Derya", "Kurt", "derya.kurt@example.com", "Pazarlama", "Pazarlama Uzmanı", true, new DateOnly(2023, 8, 21)),
            Personel(DemoKimlikleri.OnurAydin, "Onur", "Aydın", "onur.aydin@example.com", "Operasyon", "Operasyon Sorumlusu", true, new DateOnly(2021, 11, 8)),
            Personel(DemoKimlikleri.SelinTas, "Selin", "Taş", "selin.tas@example.com", "Lojistik", "Lojistik Uzmanı", true, new DateOnly(2024, 1, 9)),
            Personel(DemoKimlikleri.MertCan, "Mert", "Can", "mert.can@example.com", "Hukuk", "Hukuk Danışmanı", true, new DateOnly(2022, 5, 16)),
            Personel(DemoKimlikleri.EceSahin, "Ece", "Şahin", "ece.sahin@example.com", "Üretim", "Üretim Planlama Uzmanı", true, new DateOnly(2021, 7, 19)),
            Personel(DemoKimlikleri.KaanOz, "Kaan", "Öz", "kaan.oz@example.com", "Yönetim", "Genel Müdür Yardımcısı", true, new DateOnly(2020, 2, 11)),
            Personel(Guid.Parse("10000000-0000-0000-0000-000000000013"), "Nazlı", "Eren", "nazli.eren@example.com", "Finans", "Muhasebe Uzmanı", false, new DateOnly(2024, 3, 4)),
            Personel(Guid.Parse("10000000-0000-0000-0000-000000000014"), "Arda", "Koç", "arda.koc@example.com", "Satış", "Bölge Satış Sorumlusu", false, new DateOnly(2023, 1, 18)),
            Personel(Guid.Parse("10000000-0000-0000-0000-000000000015"), "İrem", "Polat", "irem.polat@example.com", "Pazarlama", "Dijital Pazarlama Uzmanı", false, new DateOnly(2024, 4, 22)),
            Personel(Guid.Parse("10000000-0000-0000-0000-000000000016"), "Tolga", "Yıldız", "tolga.yildiz@example.com", "Operasyon", "Saha Operasyon Uzmanı", false, new DateOnly(2022, 12, 2)),
            Personel(Guid.Parse("10000000-0000-0000-0000-000000000017"), "Gizem", "Acar", "gizem.acar@example.com", "İnsan Kaynakları", "Bordro Uzmanı", false, new DateOnly(2021, 10, 25)),
            Personel(Guid.Parse("10000000-0000-0000-0000-000000000018"), "Can", "Turan", "can.turan@example.com", "Bilgi İşlem", "Network Uzmanı", false, new DateOnly(2020, 9, 7)),
            Personel(Guid.Parse("10000000-0000-0000-0000-000000000019"), "Buse", "Güneş", "buse.gunes@example.com", "Lojistik", "Depo Planlama Uzmanı", false, new DateOnly(2024, 6, 13)),
            Personel(Guid.Parse("10000000-0000-0000-0000-000000000020"), "Kerem", "Uslu", "kerem.uslu@example.com", "Üretim", "Vardiya Amiri", false, new DateOnly(2023, 9, 29)),
            Personel(Guid.Parse("10000000-0000-0000-0000-000000000021"), "Aslı", "Bozkurt", "asli.bozkurt@example.com", "Yönetim", "Yönetici Asistanı", false, new DateOnly(2024, 5, 6)),
            Personel(Guid.Parse("10000000-0000-0000-0000-000000000022"), "Emre", "Kaplan", "emre.kaplan@example.com", "Satış", "Satış Operasyon Uzmanı", false, new DateOnly(2022, 8, 14)),
            Personel(Guid.Parse("10000000-0000-0000-0000-000000000023"), "Seda", "Alkan", "seda.alkan@example.com", "Finans", "Bütçe Planlama Uzmanı", false, new DateOnly(2021, 12, 20)),
            Personel(Guid.Parse("10000000-0000-0000-0000-000000000024"), "Fatih", "Mert", "fatih.mert@example.com", "Operasyon", "Kalite Kontrol Uzmanı", false, new DateOnly(2023, 2, 27)),
            Personel(Guid.Parse("10000000-0000-0000-0000-000000000025"), "Pelin", "Oral", "pelin.oral@example.com", "Pazarlama", "Marka Uzmanı", false, new DateOnly(2024, 7, 1)),
            Personel(Guid.Parse("10000000-0000-0000-0000-000000000026"), "Deniz", "Erdoğan", "deniz.erdogan@example.com", "Hukuk", "Sözleşme Uzmanı", false, new DateOnly(2022, 4, 12)),
            Personel(Guid.Parse("10000000-0000-0000-0000-000000000027"), "Serkan", "Kılıç", "serkan.kilic@example.com", "Bilgi İşlem", "Sistem Destek Uzmanı", false, new DateOnly(2023, 10, 10)),
            Personel(Guid.Parse("10000000-0000-0000-0000-000000000028"), "Ebru", "Yaman", "ebru.yaman@example.com", "İnsan Kaynakları", "Eğitim Uzmanı", false, new DateOnly(2020, 6, 15)),
            Personel(Guid.Parse("10000000-0000-0000-0000-000000000029"), "Murat", "Özer", "murat.ozer@example.com", "Lojistik", "Sevkiyat Uzmanı", false, new DateOnly(2023, 11, 17)),
            Personel(Guid.Parse("10000000-0000-0000-0000-000000000030"), "Lale", "Kara", "lale.kara@example.com", "Üretim", "Proses Uzmanı", false, new DateOnly(2021, 3, 23)),
            Personel(Guid.Parse("10000000-0000-0000-0000-000000000031"), "Hakan", "Sezer", "hakan.sezer@example.com", "Satış", "Eski Satış Temsilcisi", false, new DateOnly(2020, 1, 6), new DateOnly(2025, 12, 31)),
            Personel(Guid.Parse("10000000-0000-0000-0000-000000000032"), "Yasemin", "Bulut", "yasemin.bulut@example.com", "Finans", "Eski Finans Analisti", false, new DateOnly(2019, 9, 2), new DateOnly(2026, 2, 15))
        ];

        Personel Personel(Guid id, string ad, string soyad, string email, string departman, string unvan, bool sorumluMu, DateOnly iseGiris, DateOnly? ayrilis = null)
            => new()
            {
                Id = id,
                Ad = ad,
                Soyad = soyad,
                Email = email,
                DepartmanId = DepartmanId(departman),
                Unvan = unvan,
                DepartmanSorumlusuMu = sorumluMu,
                IseGirisTarihi = iseGiris,
                IstenAyrilisTarihi = ayrilis,
                Durum = ayrilis.HasValue ? PersonelDurumu.IstenAyrildi : PersonelDurumu.Aktif,
                AktifMi = !ayrilis.HasValue
            };
    }

    private static async Task KullaniciOlusturAsync(
        UserManager<UygulamaKullanici> userManager,
        string kullaniciAdi,
        string sifre,
        KullaniciRolu rol,
        Guid personelId)
    {
        var kullanici = new UygulamaKullanici
        {
            UserName = kullaniciAdi,
            PersonelId = personelId,
            AktifMi = true
        };

        var sonuc = await userManager.CreateAsync(kullanici, sifre);
        if (!sonuc.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", sonuc.Errors.Select(hata => hata.Description)));
        }

        var rolSonucu = await userManager.AddToRoleAsync(kullanici, rol.ToString());
        if (!rolSonucu.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", rolSonucu.Errors.Select(hata => hata.Description)));
        }
    }

    private static async Task DemoVerileriniTemizleAsync(KimlikPersonelDbContext dbContext, CancellationToken cancellationToken)
    {
        dbContext.UserRoles.RemoveRange(dbContext.UserRoles);
        dbContext.UserClaims.RemoveRange(dbContext.UserClaims);
        dbContext.UserLogins.RemoveRange(dbContext.UserLogins);
        dbContext.UserTokens.RemoveRange(dbContext.UserTokens);
        dbContext.RoleClaims.RemoveRange(dbContext.RoleClaims);
        dbContext.Users.RemoveRange(dbContext.Users);
        dbContext.Roles.RemoveRange(dbContext.Roles);
        dbContext.Personeller.RemoveRange(dbContext.Personeller);
        dbContext.Departmanlar.RemoveRange(dbContext.Departmanlar);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static class DemoKimlikleri
    {
        public static readonly Guid BilgiIslem = Guid.Parse("20000000-0000-0000-0000-000000000001");
        public static readonly Guid InsanKaynaklari = Guid.Parse("20000000-0000-0000-0000-000000000002");
        public static readonly Guid Finans = Guid.Parse("20000000-0000-0000-0000-000000000003");
        public static readonly Guid Satis = Guid.Parse("20000000-0000-0000-0000-000000000004");
        public static readonly Guid Pazarlama = Guid.Parse("20000000-0000-0000-0000-000000000005");
        public static readonly Guid Operasyon = Guid.Parse("20000000-0000-0000-0000-000000000006");
        public static readonly Guid Lojistik = Guid.Parse("20000000-0000-0000-0000-000000000007");
        public static readonly Guid Hukuk = Guid.Parse("20000000-0000-0000-0000-000000000008");
        public static readonly Guid Uretim = Guid.Parse("20000000-0000-0000-0000-000000000009");
        public static readonly Guid Yonetim = Guid.Parse("20000000-0000-0000-0000-000000000010");

        public static readonly Guid AyseYilmaz = Guid.Parse("10000000-0000-0000-0000-000000000001");
        public static readonly Guid MehmetKaya = Guid.Parse("10000000-0000-0000-0000-000000000002");
        public static readonly Guid CemArslan = Guid.Parse("10000000-0000-0000-0000-000000000003");
        public static readonly Guid ElifDemir = Guid.Parse("10000000-0000-0000-0000-000000000004");
        public static readonly Guid ZeynepAksoy = Guid.Parse("10000000-0000-0000-0000-000000000005");
        public static readonly Guid BurakCelik = Guid.Parse("10000000-0000-0000-0000-000000000006");
        public static readonly Guid DeryaKurt = Guid.Parse("10000000-0000-0000-0000-000000000007");
        public static readonly Guid OnurAydin = Guid.Parse("10000000-0000-0000-0000-000000000008");
        public static readonly Guid SelinTas = Guid.Parse("10000000-0000-0000-0000-000000000009");
        public static readonly Guid MertCan = Guid.Parse("10000000-0000-0000-0000-000000000010");
        public static readonly Guid EceSahin = Guid.Parse("10000000-0000-0000-0000-000000000011");
        public static readonly Guid KaanOz = Guid.Parse("10000000-0000-0000-0000-000000000012");
    }
}
