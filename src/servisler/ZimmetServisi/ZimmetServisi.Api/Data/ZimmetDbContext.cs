using ZimmetServisi.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ZimmetServisi.Api.Data;

public sealed class ZimmetDbContext(DbContextOptions<ZimmetDbContext> options) : DbContext(options)
{
    public DbSet<Zimmet> Zimmetler => Set<Zimmet>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("zimmet");

        modelBuilder.Entity<Zimmet>(entity =>
        {
            entity.ToTable("Zimmetler");
            entity.HasKey(zimmet => zimmet.Id);

            entity.Property(zimmet => zimmet.CihazAd)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(zimmet => zimmet.CihazAssetTag)
                .HasMaxLength(150);

            entity.Property(zimmet => zimmet.CihazSeriNumarasi)
                .HasMaxLength(150);

            entity.Property(zimmet => zimmet.PersonelAdSoyad)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(zimmet => zimmet.PersonelEmail)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(zimmet => zimmet.IadeNotu)
                .HasMaxLength(1000);

            entity.Property(zimmet => zimmet.Durum)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(zimmet => zimmet.IadeKontrolDurumu)
                .HasConversion<string>()
                .HasMaxLength(40);

            entity.HasIndex(zimmet => zimmet.CihazId)
                .IsUnique()
                .HasFilter("\"Durum\" IN ('Aktif', 'IadeSurecinde')");

            entity.HasIndex(zimmet => zimmet.PersonelId);
            entity.HasIndex(zimmet => zimmet.Durum);
        });
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
}
