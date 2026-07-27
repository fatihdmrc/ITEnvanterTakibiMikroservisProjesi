using KimlikVePersonelServisi.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KimlikVePersonelServisi.Api.Data;

public sealed class KimlikPersonelDbContext(DbContextOptions<KimlikPersonelDbContext> options) : DbContext(options)
{
    public DbSet<Departman> Departmanlar => Set<Departman>();
    public DbSet<Personel> Personeller => Set<Personel>();
    public DbSet<Kullanici> Kullanicilar => Set<Kullanici>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("kimlik_personel");

        DepartmanModeliniOlustur(modelBuilder);
        PersonelModeliniOlustur(modelBuilder);
        KullaniciModeliniOlustur(modelBuilder);
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

    private static void KullaniciModeliniOlustur(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Kullanici>(entity =>
        {
            entity.ToTable("Kullanicilar");
            entity.HasKey(kullanici => kullanici.Id);

            entity.Property(kullanici => kullanici.KullaniciAdi)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(kullanici => kullanici.SifreHash)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(kullanici => kullanici.Rol)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(kullanici => kullanici.KullaniciAdi)
                .IsUnique();

            entity.HasIndex(kullanici => kullanici.PersonelId)
                .IsUnique();

            entity.HasOne<Personel>()
                .WithOne()
                .HasForeignKey<Kullanici>(kullanici => kullanici.PersonelId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
