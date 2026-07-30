using EnvanterServisi.Api.Data;
using EnvanterServisi.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnvanterServisi.Api.Repositories;

public sealed class EfLokasyonRepository(EnvanterDbContext dbContext)
    : EfGenericRepository<Lokasyon>(dbContext), ILokasyonRepository
{
    public override async Task<IReadOnlyCollection<Lokasyon>> ListeleAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .OrderBy(lokasyon => lokasyon.Ad)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> AktifVarMiAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(lokasyon => lokasyon.Id == id && lokasyon.AktifMi, cancellationToken);
    }

    public async Task<bool> AdKullaniliyorMuAsync(string ad, Guid? ustLokasyonId, Guid? haricLokasyonId = null, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(lokasyon =>
            lokasyon.Ad == ad &&
            lokasyon.UstLokasyonId == ustLokasyonId &&
            (!haricLokasyonId.HasValue || lokasyon.Id != haricLokasyonId.Value),
            cancellationToken);
    }
}
