using EnvanterServisi.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnvanterServisi.Api.Data;

public sealed class EnvanterDbContext(DbContextOptions<EnvanterDbContext> options) : DbContext(options)
{
    public DbSet<Kategori> Kategoriler => Set<Kategori>();
    public DbSet<Lokasyon> Lokasyonlar => Set<Lokasyon>();
    public DbSet<Cihaz> Cihazlar => Set<Cihaz>();
    public DbSet<SarfMalzeme> SarfMalzemeler => Set<SarfMalzeme>();
    public DbSet<StokHareketi> StokHareketleri => Set<StokHareketi>();
    public DbSet<KritikStokKurali> KritikStokKurallari => Set<KritikStokKurali>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("envanter");

        KategoriModeliniOlustur(modelBuilder);
        LokasyonModeliniOlustur(modelBuilder);
        CihazModeliniOlustur(modelBuilder);
        SarfMalzemeModeliniOlustur(modelBuilder);
        StokHareketiModeliniOlustur(modelBuilder);
        KritikStokKuraliModeliniOlustur(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var guncellenenKayitlar = ChangeTracker.Entries<TemelEntity>()
            .Where(entry => entry.State == EntityState.Modified);

        foreach (var entry in guncellenenKayitlar)
        {
            entry.Entity.GuncellenmeTarihi = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    private static void KategoriModeliniOlustur(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Kategori>(entity =>
        {
            entity.ToTable("Kategoriler");
            entity.HasKey(kategori => kategori.Id);

            entity.Property(kategori => kategori.Ad)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(kategori => kategori.VarlikTuru)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.HasIndex(kategori => new { kategori.Ad, kategori.UstKategoriId })
                .IsUnique();

            entity.HasOne<Kategori>()
                .WithMany()
                .HasForeignKey(kategori => kategori.UstKategoriId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void LokasyonModeliniOlustur(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lokasyon>(entity =>
        {
            entity.ToTable("Lokasyonlar");
            entity.HasKey(lokasyon => lokasyon.Id);

            entity.Property(lokasyon => lokasyon.Ad)
                .HasMaxLength(150)
                .IsRequired();

            entity.HasIndex(lokasyon => new { lokasyon.Ad, lokasyon.UstLokasyonId })
                .IsUnique();

            entity.HasOne<Lokasyon>()
                .WithMany()
                .HasForeignKey(lokasyon => lokasyon.UstLokasyonId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void CihazModeliniOlustur(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cihaz>(entity =>
        {
            entity.ToTable("Cihazlar", table =>
            {
                table.HasCheckConstraint(
                    "CK_Cihazlar_SeriNumarasi_Veya_AssetTag",
                    "\"SeriNumarasi\" IS NOT NULL OR \"AssetTag\" IS NOT NULL");
            });

            entity.HasKey(cihaz => cihaz.Id);

            entity.Property(cihaz => cihaz.SeriNumarasi).HasMaxLength(150);
            entity.Property(cihaz => cihaz.AssetTag).HasMaxLength(150);

            entity.Property(cihaz => cihaz.Ad)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(cihaz => cihaz.Marka)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(cihaz => cihaz.Model)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(cihaz => cihaz.Durum)
                .HasConversion<string>()
                .HasMaxLength(40)
                .IsRequired();

            entity.Property(cihaz => cihaz.EldenCikarmaTipi)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(cihaz => cihaz.EldenCikarmaAciklamasi).HasMaxLength(500);
            entity.Property(cihaz => cihaz.SatilanKisiVeyaKurum).HasMaxLength(250);

            entity.HasIndex(cihaz => cihaz.SeriNumarasi)
                .IsUnique();

            entity.HasIndex(cihaz => cihaz.AssetTag)
                .IsUnique();

            entity.HasOne<Kategori>()
                .WithMany()
                .HasForeignKey(cihaz => cihaz.KategoriId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Lokasyon>()
                .WithMany()
                .HasForeignKey(cihaz => cihaz.LokasyonId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void SarfMalzemeModeliniOlustur(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SarfMalzeme>(entity =>
        {
            entity.ToTable("SarfMalzemeler");
            entity.HasKey(sarfMalzeme => sarfMalzeme.Id);

            entity.Property(sarfMalzeme => sarfMalzeme.Ad)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(sarfMalzeme => sarfMalzeme.Birim)
                .HasMaxLength(30)
                .IsRequired();

            entity.HasIndex(sarfMalzeme => new { sarfMalzeme.Ad, sarfMalzeme.KategoriId, sarfMalzeme.LokasyonId })
                .IsUnique();

            entity.HasOne<Kategori>()
                .WithMany()
                .HasForeignKey(sarfMalzeme => sarfMalzeme.KategoriId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Lokasyon>()
                .WithMany()
                .HasForeignKey(sarfMalzeme => sarfMalzeme.LokasyonId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void StokHareketiModeliniOlustur(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StokHareketi>(entity =>
        {
            entity.ToTable("StokHareketleri");
            entity.HasKey(stokHareketi => stokHareketi.Id);

            entity.Property(stokHareketi => stokHareketi.HareketTipi)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(stokHareketi => stokHareketi.Neden)
                .HasConversion<string>()
                .HasMaxLength(40)
                .IsRequired();

            entity.Property(stokHareketi => stokHareketi.Aciklama).HasMaxLength(500);
        });
    }

    private static void KritikStokKuraliModeliniOlustur(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<KritikStokKurali>(entity =>
        {
            entity.ToTable("KritikStokKurallari");
            entity.HasKey(kritikStokKurali => kritikStokKurali.Id);

            entity.Property(kritikStokKurali => kritikStokKurali.CihazModeli).HasMaxLength(150);

            entity.HasIndex(kritikStokKurali => new
                {
                    kritikStokKurali.LokasyonId,
                    kritikStokKurali.KategoriId,
                    kritikStokKurali.CihazModeli
                })
                .IsUnique();
        });
    }
}
