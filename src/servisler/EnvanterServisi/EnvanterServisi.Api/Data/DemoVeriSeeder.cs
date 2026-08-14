using EnvanterServisi.Api.Domain.Entities;
using EnvanterServisi.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnvanterServisi.Api.Data;

public static class DemoVeriSeeder
{
    public static async Task SeedAsync(EnvanterDbContext dbContext, bool sifirla = false, CancellationToken cancellationToken = default)
    {
        if (sifirla)
        {
            await DemoVerileriniTemizleAsync(dbContext, cancellationToken);
        }

        if (await dbContext.Kategoriler.AnyAsync(cancellationToken) || await dbContext.Lokasyonlar.AnyAsync(cancellationToken))
        {
            return;
        }

        var kategoriler = KategorileriOlustur();
        var lokasyonlar = LokasyonlariOlustur();
        dbContext.Kategoriler.AddRange(kategoriler);
        dbContext.Lokasyonlar.AddRange(lokasyonlar);
        await dbContext.SaveChangesAsync(cancellationToken);

        var cihazlar = CihazlariOlustur();
        var sarflar = SarflariOlustur();
        dbContext.Cihazlar.AddRange(cihazlar);
        dbContext.SarfMalzemeler.AddRange(sarflar);
        dbContext.KritikStokKurallari.AddRange(KritikStokKurallariniOlustur());
        dbContext.StokHareketleri.AddRange(StokHareketleriniOlustur(cihazlar, sarflar));

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static List<Kategori> KategorileriOlustur()
        =>
        [
            new() { Id = DemoKimlikleri.KategoriBilgisayar, Ad = "Bilgisayar", VarlikTuru = VarlikTuru.SeriNumarali },
            new() { Id = DemoKimlikleri.KategoriLaptop, Ad = "Laptop", UstKategoriId = DemoKimlikleri.KategoriBilgisayar, VarlikTuru = VarlikTuru.SeriNumarali },
            new() { Id = DemoKimlikleri.KategoriMasaustu, Ad = "Masaüstü", UstKategoriId = DemoKimlikleri.KategoriBilgisayar, VarlikTuru = VarlikTuru.SeriNumarali },
            new() { Id = DemoKimlikleri.KategoriMonitor, Ad = "Monitör", VarlikTuru = VarlikTuru.SeriNumarali },
            new() { Id = DemoKimlikleri.KategoriTelefon, Ad = "Telefon", VarlikTuru = VarlikTuru.SeriNumarali },
            new() { Id = DemoKimlikleri.KategoriNetwork, Ad = "Ağ Ekipmanı", VarlikTuru = VarlikTuru.SeriNumarali },
            new() { Id = DemoKimlikleri.KategoriYazici, Ad = "Yazıcı", VarlikTuru = VarlikTuru.SeriNumarali },
            new() { Id = DemoKimlikleri.KategoriKablo, Ad = "Kablo", VarlikTuru = VarlikTuru.SarfMalzeme, KritikStokSeviyesi = 20 },
            new() { Id = DemoKimlikleri.KategoriToner, Ad = "Toner", VarlikTuru = VarlikTuru.SarfMalzeme, KritikStokSeviyesi = 8 },
            new() { Id = DemoKimlikleri.KategoriAksesuar, Ad = "Aksesuar", VarlikTuru = VarlikTuru.SarfMalzeme, KritikStokSeviyesi = 15 }
        ];

    private static List<Lokasyon> LokasyonlariOlustur()
        =>
        [
            new() { Id = DemoKimlikleri.LokasyonMerkez, Ad = "Merkez Bina" },
            new() { Id = DemoKimlikleri.LokasyonKat1, Ad = "1. Kat", UstLokasyonId = DemoKimlikleri.LokasyonMerkez },
            new() { Id = DemoKimlikleri.LokasyonKat2, Ad = "2. Kat", UstLokasyonId = DemoKimlikleri.LokasyonMerkez },
            new() { Id = DemoKimlikleri.LokasyonItDepo, Ad = "IT Depo", UstLokasyonId = DemoKimlikleri.LokasyonKat1 },
            new() { Id = DemoKimlikleri.LokasyonToplanti, Ad = "Toplantı Odaları", UstLokasyonId = DemoKimlikleri.LokasyonKat2 },
            new() { Id = DemoKimlikleri.LokasyonUretim, Ad = "Üretim Sahası" },
            new() { Id = DemoKimlikleri.LokasyonAnkara, Ad = "Ankara Ofis" },
            new() { Id = DemoKimlikleri.LokasyonIzmir, Ad = "İzmir Ofis" }
        ];

    private static List<Cihaz> CihazlariOlustur()
    {
        var cihazlar = new List<Cihaz>();
        var bugun = DateOnly.FromDateTime(DateTime.UtcNow);

        void Ekle(Guid id, string assetTag, string seriNo, string ad, string marka, string model, Guid kategoriId, Guid lokasyonId, CihazDurumu durum, DateOnly giris, EldenCikarmaTipi eldenCikarmaTipi = EldenCikarmaTipi.Yok, string? aciklama = null)
        {
            var pasif = durum is CihazDurumu.Kayip or CihazDurumu.Calindi or CihazDurumu.KullanimDisi ||
                (durum == CihazDurumu.HurdaIskarta && eldenCikarmaTipi != EldenCikarmaTipi.Yok);

            cihazlar.Add(new Cihaz
            {
                Id = id,
                AssetTag = assetTag,
                SeriNumarasi = seriNo,
                Ad = ad,
                Marka = marka,
                Model = model,
                KategoriId = kategoriId,
                LokasyonId = lokasyonId,
                Durum = durum,
                EnvantereGirisTarihi = giris,
                EnvanterdenCikisTarihi = pasif ? bugun.AddDays(-20) : null,
                EldenCikarmaTipi = eldenCikarmaTipi,
                EldenCikarmaAciklamasi = aciklama,
                AktifMi = !pasif,
                ToplamVarligaDahilMi = !pasif
            });
        }

        Ekle(DemoKimlikleri.Cihaz001, "BT-000101", "LTP-2024-0001", "Dell Latitude 5440", "Dell", "Latitude 5440", DemoKimlikleri.KategoriLaptop, DemoKimlikleri.LokasyonKat1, CihazDurumu.Zimmetli, new DateOnly(2024, 1, 10));
        Ekle(DemoKimlikleri.Cihaz002, "BT-000102", "LTP-2024-0002", "Dell Latitude 5440", "Dell", "Latitude 5440", DemoKimlikleri.KategoriLaptop, DemoKimlikleri.LokasyonKat1, CihazDurumu.Zimmetli, new DateOnly(2024, 1, 10));
        Ekle(DemoKimlikleri.Cihaz003, "BT-000103", "MBP-2024-0003", "MacBook Pro 14", "Apple", "M3 Pro", DemoKimlikleri.KategoriLaptop, DemoKimlikleri.LokasyonKat2, CihazDurumu.Zimmetli, new DateOnly(2024, 3, 8));
        Ekle(DemoKimlikleri.Cihaz004, "BT-000104", "THP-2023-0004", "Lenovo ThinkPad T14", "Lenovo", "T14 Gen 4", DemoKimlikleri.KategoriLaptop, DemoKimlikleri.LokasyonAnkara, CihazDurumu.Zimmetli, new DateOnly(2023, 11, 13));
        Ekle(DemoKimlikleri.Cihaz005, "BT-000105", "IPH-2024-0005", "iPhone 15", "Apple", "15", DemoKimlikleri.KategoriTelefon, DemoKimlikleri.LokasyonKat2, CihazDurumu.Zimmetli, new DateOnly(2024, 4, 1));
        Ekle(DemoKimlikleri.Cihaz006, "BT-000106", "MON-2023-0006", "Dell 24 Monitör", "Dell", "P2422H", DemoKimlikleri.KategoriMonitor, DemoKimlikleri.LokasyonKat1, CihazDurumu.Zimmetli, new DateOnly(2023, 5, 20));
        Ekle(DemoKimlikleri.Cihaz007, "BT-000107", "LTP-2024-0007", "HP EliteBook 840", "HP", "840 G10", DemoKimlikleri.KategoriLaptop, DemoKimlikleri.LokasyonIzmir, CihazDurumu.Zimmetli, new DateOnly(2024, 2, 18));
        Ekle(DemoKimlikleri.Cihaz008, "BT-000108", "PRN-2022-0008", "HP LaserJet Pro", "HP", "M404dn", DemoKimlikleri.KategoriYazici, DemoKimlikleri.LokasyonFinans(), CihazDurumu.Zimmetli, new DateOnly(2022, 10, 5));
        Ekle(DemoKimlikleri.Cihaz009, "BT-000109", "LTP-2023-0009", "Lenovo ThinkPad E14", "Lenovo", "E14 Gen 5", DemoKimlikleri.KategoriLaptop, DemoKimlikleri.LokasyonItDepo, CihazDurumu.Incelemede, new DateOnly(2023, 9, 11));
        Ekle(DemoKimlikleri.Cihaz010, "BT-000110", "MON-2022-0010", "LG UltraWide", "LG", "29WN600", DemoKimlikleri.KategoriMonitor, DemoKimlikleri.LokasyonItDepo, CihazDurumu.Incelemede, new DateOnly(2022, 7, 26));
        Ekle(DemoKimlikleri.Cihaz011, "BT-000111", "LTP-2023-0011", "Dell Latitude 5530", "Dell", "Latitude 5530", DemoKimlikleri.KategoriLaptop, DemoKimlikleri.LokasyonItDepo, CihazDurumu.Kullanilabilir, new DateOnly(2023, 2, 15));
        Ekle(DemoKimlikleri.Cihaz012, "BT-000112", "LTP-2022-0012", "HP ProBook 450", "HP", "450 G9", DemoKimlikleri.KategoriLaptop, DemoKimlikleri.LokasyonItDepo, CihazDurumu.Bakimda, new DateOnly(2022, 11, 22));
        Ekle(DemoKimlikleri.Cihaz013, "BT-000113", "DSK-2021-0013", "Dell OptiPlex 7090", "Dell", "OptiPlex 7090", DemoKimlikleri.KategoriMasaustu, DemoKimlikleri.LokasyonItDepo, CihazDurumu.HurdaIskarta, new DateOnly(2021, 6, 9));
        Ekle(DemoKimlikleri.Cihaz014, "BT-000114", "SWT-2024-0014", "Cisco Switch", "Cisco", "CBS350", DemoKimlikleri.KategoriNetwork, DemoKimlikleri.LokasyonItDepo, CihazDurumu.Kullanilabilir, new DateOnly(2024, 5, 6));
        Ekle(DemoKimlikleri.Cihaz015, "BT-000115", "LTP-2022-0015", "Asus ExpertBook", "Asus", "B5", DemoKimlikleri.KategoriLaptop, DemoKimlikleri.LokasyonItDepo, CihazDurumu.KullanimDisi, new DateOnly(2022, 1, 14), EldenCikarmaTipi.Satildi, "Yaşam döngüsü tamamlandıktan sonra satıldı.");

        for (var sira = 16; sira <= 35; sira++)
        {
            var id = Guid.Parse($"30000000-0000-0000-0000-{sira:000000000000}");
            var durum = sira % 11 == 0 ? CihazDurumu.Bakimda : CihazDurumu.Kullanilabilir;
            var kategori = sira % 5 == 0 ? DemoKimlikleri.KategoriMonitor : DemoKimlikleri.KategoriLaptop;
            var lokasyon = sira % 3 == 0 ? DemoKimlikleri.LokasyonItDepo : DemoKimlikleri.LokasyonKat2;
            Ekle(id, $"BT-{sira + 100:000000}", $"AUTO-2024-{sira:0000}", sira % 2 == 0 ? "Dell Latitude 5450" : "Lenovo ThinkPad T14", sira % 2 == 0 ? "Dell" : "Lenovo", sira % 2 == 0 ? "Latitude 5450" : "T14 Gen 5", kategori, lokasyon, durum, new DateOnly(2024, 6, Math.Min(sira, 28)));
        }

        return cihazlar;
    }

    private static List<SarfMalzeme> SarflariOlustur()
        =>
        [
            Sarf(Guid.Parse("40000000-0000-0000-0000-000000000001"), "HDMI Kablo 2m", DemoKimlikleri.KategoriKablo, DemoKimlikleri.LokasyonItDepo, 14, 20),
            Sarf(Guid.Parse("40000000-0000-0000-0000-000000000002"), "USB-C Dock Adaptörü", DemoKimlikleri.KategoriAksesuar, DemoKimlikleri.LokasyonItDepo, 7, 10),
            Sarf(Guid.Parse("40000000-0000-0000-0000-000000000003"), "HP 59A Toner", DemoKimlikleri.KategoriToner, DemoKimlikleri.LokasyonItDepo, 3, 8),
            Sarf(Guid.Parse("40000000-0000-0000-0000-000000000004"), "Logitech Kablosuz Mouse", DemoKimlikleri.KategoriAksesuar, DemoKimlikleri.LokasyonItDepo, 18, 15),
            Sarf(Guid.Parse("40000000-0000-0000-0000-000000000005"), "Klavye TR Q", DemoKimlikleri.KategoriAksesuar, DemoKimlikleri.LokasyonItDepo, 22, 15),
            Sarf(Guid.Parse("40000000-0000-0000-0000-000000000006"), "CAT6 Patch Kablo", DemoKimlikleri.KategoriKablo, DemoKimlikleri.LokasyonAnkara, 11, 20),
            Sarf(Guid.Parse("40000000-0000-0000-0000-000000000007"), "USB Bellek 64GB", DemoKimlikleri.KategoriAksesuar, DemoKimlikleri.LokasyonItDepo, 9, 12),
            Sarf(Guid.Parse("40000000-0000-0000-0000-000000000008"), "Canon 057 Toner", DemoKimlikleri.KategoriToner, DemoKimlikleri.LokasyonIzmir, 6, 8),
            Sarf(Guid.Parse("40000000-0000-0000-0000-000000000009"), "DisplayPort Kablo", DemoKimlikleri.KategoriKablo, DemoKimlikleri.LokasyonItDepo, 24, 20),
            Sarf(Guid.Parse("40000000-0000-0000-0000-000000000010"), "Laptop Çantası", DemoKimlikleri.KategoriAksesuar, DemoKimlikleri.LokasyonItDepo, 13, 10)
        ];

    private static List<KritikStokKurali> KritikStokKurallariniOlustur()
        =>
        [
            new() { Id = Guid.Parse("50000000-0000-0000-0000-000000000001"), LokasyonId = DemoKimlikleri.LokasyonItDepo, KategoriId = DemoKimlikleri.KategoriLaptop, CihazModeli = "Latitude 5450", KritikStokSeviyesi = 5 }
        ];

    private static List<StokHareketi> StokHareketleriniOlustur(IReadOnlyCollection<Cihaz> cihazlar, IReadOnlyCollection<SarfMalzeme> sarflar)
    {
        var kullaniciId = Guid.Parse("10000000-0000-0000-0000-000000000002");
        var hareketler = cihazlar.Take(15).Select((cihaz, index) => new StokHareketi
        {
            Id = Guid.Parse($"60000000-0000-0000-0000-{index + 1:000000000000}"),
            CihazId = cihaz.Id,
            HareketTipi = cihaz.Durum == CihazDurumu.KullanimDisi ? StokHareketTipi.Cikis : StokHareketTipi.Duzeltme,
            Neden = cihaz.Durum switch
            {
                CihazDurumu.Zimmetli => StokHareketNedeni.Zimmetlendi,
                CihazDurumu.Incelemede => StokHareketNedeni.ZimmetIadeAlindi,
                CihazDurumu.Bakimda => StokHareketNedeni.Ariza,
                CihazDurumu.HurdaIskarta => StokHareketNedeni.HurdaIskarta,
                CihazDurumu.KullanimDisi => StokHareketNedeni.ManuelStokCikisi,
                _ => StokHareketNedeni.EnvantereGiris
            },
            Aciklama = $"{cihaz.AssetTag} için demo cihaz durum hareketi.",
            OlusturanKullaniciId = kullaniciId
        }).ToList();

        hareketler.AddRange(sarflar.Select((sarf, index) => new StokHareketi
        {
            Id = Guid.Parse($"60000000-0000-0000-0000-{index + 101:000000000000}"),
            SarfMalzemeId = sarf.Id,
            HareketTipi = index % 3 == 0 ? StokHareketTipi.Cikis : StokHareketTipi.Giris,
            Neden = index % 3 == 0 ? StokHareketNedeni.ManuelStokCikisi : StokHareketNedeni.EnvantereGiris,
            Miktar = index % 3 == 0 ? 4 : 12,
            Aciklama = $"{sarf.Ad} için demo stok hareketi.",
            OlusturanKullaniciId = kullaniciId
        }));

        return hareketler;
    }

    private static SarfMalzeme Sarf(Guid id, string ad, Guid kategoriId, Guid lokasyonId, int miktar, int kritik)
        => new()
        {
            Id = id,
            Ad = ad,
            KategoriId = kategoriId,
            LokasyonId = lokasyonId,
            EldekiMiktar = miktar,
            KritikStokSeviyesi = kritik,
            Birim = "Adet",
            AktifMi = true
        };

    private static Task DemoVerileriniTemizleAsync(EnvanterDbContext dbContext, CancellationToken cancellationToken)
        => dbContext.Database.ExecuteSqlRawAsync(
            """
            TRUNCATE TABLE
                envanter."StokHareketleri",
                envanter."KritikStokKurallari",
                envanter."SarfMalzemeler",
                envanter."Cihazlar",
                envanter."Kategoriler",
                envanter."Lokasyonlar"
            RESTART IDENTITY CASCADE;
            """,
            cancellationToken);

    private static class DemoKimlikleri
    {
        public static readonly Guid KategoriBilgisayar = Guid.Parse("31000000-0000-0000-0000-000000000001");
        public static readonly Guid KategoriLaptop = Guid.Parse("31000000-0000-0000-0000-000000000002");
        public static readonly Guid KategoriMasaustu = Guid.Parse("31000000-0000-0000-0000-000000000003");
        public static readonly Guid KategoriMonitor = Guid.Parse("31000000-0000-0000-0000-000000000004");
        public static readonly Guid KategoriTelefon = Guid.Parse("31000000-0000-0000-0000-000000000005");
        public static readonly Guid KategoriNetwork = Guid.Parse("31000000-0000-0000-0000-000000000006");
        public static readonly Guid KategoriYazici = Guid.Parse("31000000-0000-0000-0000-000000000007");
        public static readonly Guid KategoriKablo = Guid.Parse("31000000-0000-0000-0000-000000000008");
        public static readonly Guid KategoriToner = Guid.Parse("31000000-0000-0000-0000-000000000009");
        public static readonly Guid KategoriAksesuar = Guid.Parse("31000000-0000-0000-0000-000000000010");

        public static readonly Guid LokasyonMerkez = Guid.Parse("32000000-0000-0000-0000-000000000001");
        public static readonly Guid LokasyonKat1 = Guid.Parse("32000000-0000-0000-0000-000000000002");
        public static readonly Guid LokasyonKat2 = Guid.Parse("32000000-0000-0000-0000-000000000003");
        public static readonly Guid LokasyonItDepo = Guid.Parse("32000000-0000-0000-0000-000000000004");
        public static readonly Guid LokasyonToplanti = Guid.Parse("32000000-0000-0000-0000-000000000005");
        public static readonly Guid LokasyonUretim = Guid.Parse("32000000-0000-0000-0000-000000000006");
        public static readonly Guid LokasyonAnkara = Guid.Parse("32000000-0000-0000-0000-000000000007");
        public static readonly Guid LokasyonIzmir = Guid.Parse("32000000-0000-0000-0000-000000000008");

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
        public static readonly Guid Cihaz014 = Guid.Parse("30000000-0000-0000-0000-000000000014");
        public static readonly Guid Cihaz015 = Guid.Parse("30000000-0000-0000-0000-000000000015");

        public static Guid LokasyonFinans() => LokasyonKat2;
    }
}
