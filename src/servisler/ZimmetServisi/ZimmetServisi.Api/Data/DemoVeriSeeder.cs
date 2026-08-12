using Microsoft.EntityFrameworkCore;
using ZimmetServisi.Api.Domain.Entities;
using ZimmetServisi.Api.Domain.Enums;

namespace ZimmetServisi.Api.Data;

public static class DemoVeriSeeder
{
    public static async Task SeedAsync(ZimmetDbContext dbContext, bool sifirla = false, CancellationToken cancellationToken = default)
    {
        if (sifirla)
        {
            await DemoVerileriniTemizleAsync(dbContext, cancellationToken);
        }

        if (await dbContext.Zimmetler.AnyAsync(cancellationToken))
        {
            return;
        }

        dbContext.Zimmetler.AddRange(ZimmetleriOlustur());
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static List<Zimmet> ZimmetleriOlustur()
    {
        var adminId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var itId = Guid.Parse("10000000-0000-0000-0000-000000000002");

        return
        [
            Aktif(Guid.Parse("70000000-0000-0000-0000-000000000001"), DemoKimlikleri.Cihaz001, "Dell Latitude 5440 Dell Latitude 5440", "BT-000101", "LTP-2024-0001", DemoKimlikleri.ElifDemir, "Elif Demir", "elif.demir@example.com", new DateOnly(2026, 1, 12), adminId),
            Aktif(Guid.Parse("70000000-0000-0000-0000-000000000002"), DemoKimlikleri.Cihaz002, "Dell Latitude 5440 Dell Latitude 5440", "BT-000102", "LTP-2024-0002", DemoKimlikleri.ZeynepAksoy, "Zeynep Aksoy", "zeynep.aksoy@example.com", new DateOnly(2026, 2, 3), itId),
            Aktif(Guid.Parse("70000000-0000-0000-0000-000000000003"), DemoKimlikleri.Cihaz003, "MacBook Pro 14 Apple M3 Pro", "BT-000103", "MBP-2024-0003", DemoKimlikleri.BurakCelik, "Burak Çelik", "burak.celik@example.com", new DateOnly(2026, 2, 20), adminId),
            Aktif(Guid.Parse("70000000-0000-0000-0000-000000000004"), DemoKimlikleri.Cihaz004, "Lenovo ThinkPad T14 Lenovo T14 Gen 4", "BT-000104", "THP-2023-0004", DemoKimlikleri.DeryaKurt, "Derya Kurt", "derya.kurt@example.com", new DateOnly(2026, 3, 5), itId),
            Aktif(Guid.Parse("70000000-0000-0000-0000-000000000005"), DemoKimlikleri.Cihaz005, "iPhone 15 Apple 15", "BT-000105", "IPH-2024-0005", DemoKimlikleri.OnurAydin, "Onur Aydın", "onur.aydin@example.com", new DateOnly(2026, 3, 18), adminId),
            Aktif(Guid.Parse("70000000-0000-0000-0000-000000000006"), DemoKimlikleri.Cihaz006, "Dell 24 Monitör Dell P2422H", "BT-000106", "MON-2023-0006", DemoKimlikleri.SelinTas, "Selin Taş", "selin.tas@example.com", new DateOnly(2026, 4, 2), itId),
            Aktif(Guid.Parse("70000000-0000-0000-0000-000000000007"), DemoKimlikleri.Cihaz007, "HP EliteBook 840 HP 840 G10", "BT-000107", "LTP-2024-0007", DemoKimlikleri.MertCan, "Mert Can", "mert.can@example.com", new DateOnly(2026, 4, 22), adminId),
            Aktif(Guid.Parse("70000000-0000-0000-0000-000000000008"), DemoKimlikleri.Cihaz008, "HP LaserJet Pro HP M404dn", "BT-000108", "PRN-2022-0008", DemoKimlikleri.EceSahin, "Ece Şahin", "ece.sahin@example.com", new DateOnly(2026, 5, 8), itId),
            IadeSurecinde(Guid.Parse("70000000-0000-0000-0000-000000000009"), DemoKimlikleri.Cihaz009, "Lenovo ThinkPad E14 Lenovo E14 Gen 5", "BT-000109", "LTP-2023-0009", DemoKimlikleri.KaanOz, "Kaan Öz", "kaan.oz@example.com", new DateOnly(2026, 1, 28), new DateOnly(2026, 8, 4), adminId, itId, "Kullanıcı teslim etti, teknik kontrol bekliyor."),
            IadeSurecinde(Guid.Parse("70000000-0000-0000-0000-000000000010"), DemoKimlikleri.Cihaz010, "LG UltraWide LG 29WN600", "BT-000110", "MON-2022-0010", DemoKimlikleri.CemArslan, "Cem Arslan", "cem.arslan@example.com", new DateOnly(2026, 2, 14), new DateOnly(2026, 8, 7), adminId, itId, "Panelde çizik bildirildi."),
            IadeEdildi(Guid.Parse("70000000-0000-0000-0000-000000000011"), DemoKimlikleri.Cihaz011, "Dell Latitude 5530 Dell Latitude 5530", "BT-000111", "LTP-2023-0011", DemoKimlikleri.MehmetKaya, "Mehmet Kaya", "mehmet.kaya@example.com", new DateOnly(2025, 10, 10), new DateOnly(2026, 7, 24), IadeKontrolDurumu.Saglam, adminId, itId, "Sağlam teslim alındı."),
            IadeEdildi(Guid.Parse("70000000-0000-0000-0000-000000000012"), DemoKimlikleri.Cihaz012, "HP ProBook 450 HP 450 G9", "BT-000112", "LTP-2022-0012", DemoKimlikleri.AyseYilmaz, "Ayşe Yılmaz", "ayse.yilmaz@example.com", new DateOnly(2025, 11, 6), new DateOnly(2026, 7, 28), IadeKontrolDurumu.Bakimda, adminId, itId, "Batarya değişimi gerekiyor."),
            IadeEdildi(Guid.Parse("70000000-0000-0000-0000-000000000013"), DemoKimlikleri.Cihaz013, "Dell OptiPlex 7090 Dell OptiPlex 7090", "BT-000113", "DSK-2021-0013", Guid.Parse("10000000-0000-0000-0000-000000000016"), "Tolga Yıldız", "tolga.yildiz@example.com", new DateOnly(2025, 6, 3), new DateOnly(2026, 6, 19), IadeKontrolDurumu.HurdaIskarta, adminId, itId, "Ekonomik tamir sınırının üzerinde.")
        ];
    }

    private static Zimmet Aktif(Guid id, Guid cihazId, string cihazAd, string assetTag, string seriNo, Guid personelId, string personelAdSoyad, string email, DateOnly zimmetTarihi, Guid zimmetleyenId)
        => Temel(id, cihazId, cihazAd, assetTag, seriNo, personelId, personelAdSoyad, email, zimmetTarihi, zimmetleyenId, ZimmetDurumu.Aktif);

    private static Zimmet IadeSurecinde(Guid id, Guid cihazId, string cihazAd, string assetTag, string seriNo, Guid personelId, string personelAdSoyad, string email, DateOnly zimmetTarihi, DateOnly iadeTarihi, Guid zimmetleyenId, Guid iadeAlanId, string not)
    {
        var zimmet = Temel(id, cihazId, cihazAd, assetTag, seriNo, personelId, personelAdSoyad, email, zimmetTarihi, zimmetleyenId, ZimmetDurumu.IadeSurecinde);
        zimmet.IadeTarihi = iadeTarihi;
        zimmet.IadeAlanKullaniciId = iadeAlanId;
        zimmet.IadeNotu = not;
        return zimmet;
    }

    private static Zimmet IadeEdildi(Guid id, Guid cihazId, string cihazAd, string assetTag, string seriNo, Guid personelId, string personelAdSoyad, string email, DateOnly zimmetTarihi, DateOnly iadeTarihi, IadeKontrolDurumu kontrolDurumu, Guid zimmetleyenId, Guid kontrolYapanId, string not)
    {
        var zimmet = Temel(id, cihazId, cihazAd, assetTag, seriNo, personelId, personelAdSoyad, email, zimmetTarihi, zimmetleyenId, ZimmetDurumu.IadeEdildi);
        zimmet.IadeTarihi = iadeTarihi;
        zimmet.IadeAlanKullaniciId = kontrolYapanId;
        zimmet.IadeKontrolDurumu = kontrolDurumu;
        zimmet.IadeKontroluYapanKullaniciId = kontrolYapanId;
        zimmet.IadeNotu = not;
        return zimmet;
    }

    private static Zimmet Temel(Guid id, Guid cihazId, string cihazAd, string assetTag, string seriNo, Guid personelId, string personelAdSoyad, string email, DateOnly zimmetTarihi, Guid zimmetleyenId, ZimmetDurumu durum)
        => new()
        {
            Id = id,
            CihazId = cihazId,
            CihazAd = cihazAd,
            CihazAssetTag = assetTag,
            CihazSeriNumarasi = seriNo,
            PersonelId = personelId,
            PersonelAdSoyad = personelAdSoyad,
            PersonelEmail = email,
            ZimmetTarihi = zimmetTarihi,
            ZimmetleyenKullaniciId = zimmetleyenId,
            Durum = durum
        };

    private static Task DemoVerileriniTemizleAsync(ZimmetDbContext dbContext, CancellationToken cancellationToken)
        => dbContext.Database.ExecuteSqlRawAsync(
            """
            TRUNCATE TABLE zimmet."Zimmetler" RESTART IDENTITY CASCADE;
            """,
            cancellationToken);

    private static class DemoKimlikleri
    {
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

        public static readonly Guid Cihaz001 = Guid.Parse("30000000-0000-0000-0000-000000000001");
        public static readonly Guid Cihaz002 = Guid.Parse("30000000-0000-0000-0000-000000000002");
        public static readonly Guid Cihaz003 = Guid.Parse("30000000-0000-0000-0000-000000000003");
        public static readonly Guid Cihaz004 = Guid.Parse("30000000-0000-0000-0000-000000000004");
        public static readonly Guid Cihaz005 = Guid.Parse("30000000-0000-0000-0000-000000000005");
        public static readonly Guid Cihaz006 = Guid.Parse("30000000-0000-0000-0000-000000000006");
        public static readonly Guid Cihaz007 = Guid.Parse("30000000-0000-0000-0000-000000000007");
        public static readonly Guid Cihaz008 = Guid.Parse("30000000-0000-0000-0000-000000000008");
        public static readonly Guid Cihaz009 = Guid.Parse("30000000-0000-0000-0000-000000000009");
        public static readonly Guid Cihaz010 = Guid.Parse("30000000-0000-0000-0000-000000000010");
        public static readonly Guid Cihaz011 = Guid.Parse("30000000-0000-0000-0000-000000000011");
        public static readonly Guid Cihaz012 = Guid.Parse("30000000-0000-0000-0000-000000000012");
        public static readonly Guid Cihaz013 = Guid.Parse("30000000-0000-0000-0000-000000000013");
    }
}
