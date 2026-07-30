using EnvanterServisi.Api.Domain.Entities;
using EnvanterServisi.Api.Domain.Enums;

namespace EnvanterServisi.Api.Data;

public static class DemoVeriSeeder
{
    public static async Task SeedAsync(EnvanterDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (dbContext.Kategoriler.Any() || dbContext.Lokasyonlar.Any())
        {
            return;
        }

        var bilgisayar = new Kategori { Ad = "Bilgisayar", VarlikTuru = VarlikTuru.SeriNumarali };
        var laptop = new Kategori { Ad = "Laptop", UstKategoriId = bilgisayar.Id, VarlikTuru = VarlikTuru.SeriNumarali };
        var monitor = new Kategori { Ad = "Harici Monitör", VarlikTuru = VarlikTuru.SeriNumarali };
        var kablo = new Kategori { Ad = "Kablolar", VarlikTuru = VarlikTuru.SarfMalzeme, KritikStokSeviyesi = 10 };

        dbContext.Kategoriler.AddRange(bilgisayar, laptop, monitor, kablo);

        var merkezBina = new Lokasyon { Ad = "Merkez Bina" };
        var birinciKat = new Lokasyon { Ad = "1. Kat", UstLokasyonId = merkezBina.Id };
        var depo = new Lokasyon { Ad = "IT Depo", UstLokasyonId = birinciKat.Id };

        dbContext.Lokasyonlar.AddRange(merkezBina, birinciKat, depo);

        dbContext.Cihazlar.AddRange(
            new Cihaz
            {
                SeriNumarasi = "SN-LTP-001",
                AssetTag = "BT-0001",
                Ad = "Dell Latitude 5440",
                Marka = "Dell",
                Model = "Latitude 5440",
                KategoriId = laptop.Id,
                LokasyonId = depo.Id,
                Durum = CihazDurumu.DepodaHazir
            },
            new Cihaz
            {
                SeriNumarasi = "SN-MON-001",
                AssetTag = "BT-0002",
                Ad = "Dell 24 Monitor",
                Marka = "Dell",
                Model = "P2422H",
                KategoriId = monitor.Id,
                LokasyonId = depo.Id,
                Durum = CihazDurumu.DepodaHazir
            });

        dbContext.SarfMalzemeler.Add(new SarfMalzeme
        {
            Ad = "HDMI Kablo",
            KategoriId = kablo.Id,
            LokasyonId = depo.Id,
            EldekiMiktar = 8,
            KritikStokSeviyesi = 10,
            Birim = "Adet"
        });

        dbContext.KritikStokKurallari.Add(new KritikStokKurali
        {
            LokasyonId = depo.Id,
            KategoriId = laptop.Id,
            CihazModeli = "Latitude 5440",
            KritikStokSeviyesi = 2
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
