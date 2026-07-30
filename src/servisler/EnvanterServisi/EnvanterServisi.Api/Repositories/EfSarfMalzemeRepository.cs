using EnvanterServisi.Api.Data;
using EnvanterServisi.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnvanterServisi.Api.Repositories;

public sealed class EfSarfMalzemeRepository(EnvanterDbContext dbContext)
    : EfGenericRepository<SarfMalzeme>(dbContext), ISarfMalzemeRepository
{
    public override async Task<IReadOnlyCollection<SarfMalzeme>> ListeleAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .OrderBy(sarfMalzeme => sarfMalzeme.Ad)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<SarfMalzeme>> FiltreleAsync(Guid? kategoriId = null, Guid? lokasyonId = null, string? arama = null, CancellationToken cancellationToken = default)
    {
        var sorgu = DbSet.AsNoTracking();

        if (kategoriId.HasValue)
        {
            sorgu = sorgu.Where(sarfMalzeme => sarfMalzeme.KategoriId == kategoriId.Value);
        }

        if (lokasyonId.HasValue)
        {
            sorgu = sorgu.Where(sarfMalzeme => sarfMalzeme.LokasyonId == lokasyonId.Value);
        }

        if (!string.IsNullOrWhiteSpace(arama))
        {
            var temizArama = arama.Trim();
            sorgu = sorgu.Where(sarfMalzeme => sarfMalzeme.Ad.Contains(temizArama));
        }

        return await sorgu
            .OrderBy(sarfMalzeme => sarfMalzeme.Ad)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> AdKullaniliyorMuAsync(string ad, Guid kategoriId, Guid lokasyonId, Guid? haricSarfMalzemeId = null, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(sarfMalzeme =>
            sarfMalzeme.Ad == ad &&
            sarfMalzeme.KategoriId == kategoriId &&
            sarfMalzeme.LokasyonId == lokasyonId &&
            (!haricSarfMalzemeId.HasValue || sarfMalzeme.Id != haricSarfMalzemeId.Value),
            cancellationToken);
    }

    public async Task<int> ToplamMiktarAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(sarfMalzeme => sarfMalzeme.AktifMi)
            .SumAsync(sarfMalzeme => sarfMalzeme.EldekiMiktar, cancellationToken);
    }
}
