using KimlikVePersonelServisi.Api.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KimlikVePersonelServisi.Api.Data;

public sealed class KimlikPersonelDbContext(DbContextOptions<KimlikPersonelDbContext> options)
    : IdentityDbContext<UygulamaKullanici, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Departman> Departmanlar => Set<Departman>();
    public DbSet<Personel> Personeller => Set<Personel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("kimlik_personel");

        IdentityModeliniOlustur(modelBuilder);
        DepartmanModeliniOlustur(modelBuilder);
        PersonelModeliniOlustur(modelBuilder);
    }

    private static void IdentityModeliniOlustur(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UygulamaKullanici>(entity =>
        {
            entity.ToTable("Kullanicilar");

            entity.Property(kullanici => kullanici.PersonelId)
                .IsRequired();

            entity.Property(kullanici => kullanici.AktifMi)
                .IsRequired();

            entity.HasIndex(kullanici => kullanici.PersonelId)
                .IsUnique();

            entity.HasOne<Personel>()
                .WithOne()
                .HasForeignKey<UygulamaKullanici>(kullanici => kullanici.PersonelId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<IdentityRole<Guid>>().ToTable("Roller");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("KullaniciRolleri");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("KullaniciClaimleri");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("KullaniciLoginleri");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RolClaimleri");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("KullaniciTokenlari");
    }

    private static void DepartmanModeliniOlustur(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Departman>(entity =>
        {
            entity.ToTable("Departmanlar");
            entity.HasKey(departman => departman.Id);

            entity.Property(departman => departman.Ad)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(departman => departman.AktifMi)
                .IsRequired();

            entity.HasIndex(departman => departman.Ad)
                .IsUnique();

            entity.HasOne<Personel>()
                .WithMany()
                .HasForeignKey(departman => departman.SorumluPersonelId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void PersonelModeliniOlustur(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Personel>(entity =>
        {
            entity.ToTable("Personeller");
            entity.HasKey(personel => personel.Id);

            entity.Property(personel => personel.Ad)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(personel => personel.Soyad)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(personel => personel.Email)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(personel => personel.Unvan)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(personel => personel.Durum)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.HasIndex(personel => personel.Email)
                .IsUnique();

            entity.HasOne<Departman>()
                .WithMany()
                .HasForeignKey(personel => personel.DepartmanId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

}
