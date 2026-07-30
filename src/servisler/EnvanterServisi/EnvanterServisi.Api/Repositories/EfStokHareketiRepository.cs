using EnvanterServisi.Api.Data;
using EnvanterServisi.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnvanterServisi.Api.Repositories;

public sealed class EfStokHareketiRepository(EnvanterDbContext dbContext)
    : EfGenericRepository<StokHareketi>(dbContext), IStokHareketiRepository
{
    public override async Task<IReadOnlyCollection<StokHareketi>> ListeleAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .OrderByDescending(stokHareketi => stokHareketi.OlusturulmaTarihi)
            .ToListAsync(cancellationToken);
    }
}
