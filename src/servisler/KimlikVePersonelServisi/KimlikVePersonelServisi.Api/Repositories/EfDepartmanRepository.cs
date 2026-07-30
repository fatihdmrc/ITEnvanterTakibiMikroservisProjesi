using KimlikVePersonelServisi.Api.Data;
using KimlikVePersonelServisi.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KimlikVePersonelServisi.Api.Repositories;

public sealed class EfDepartmanRepository(KimlikPersonelDbContext dbContext)
    : EfGenericRepository<Departman>(dbContext), IDepartmanRepository
{
    public override async Task<IReadOnlyCollection<Departman>> ListeleAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .OrderBy(departman => departman.Ad)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> AktifVarMiAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(departman => departman.Id == id && departman.AktifMi, cancellationToken);
    }

    public async Task<bool> AdKullaniliyorMuAsync(string ad, Guid? haricDepartmanId = null, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(departman =>
            departman.Ad == ad &&
            (!haricDepartmanId.HasValue || departman.Id != haricDepartmanId.Value),
            cancellationToken);
    }
}
