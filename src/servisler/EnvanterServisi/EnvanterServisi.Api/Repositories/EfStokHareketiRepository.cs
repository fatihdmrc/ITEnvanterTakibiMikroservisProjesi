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

    public async Task<IReadOnlyCollection<StokHareketi>> FiltreleAsync(Guid? cihazId = null, Guid? sarfMalzemeId = null, CancellationToken cancellationToken = default)
    {
        var sorgu = DbSet.AsNoTracking();

        if (cihazId.HasValue)
        {
            sorgu = sorgu.Where(stokHareketi => stokHareketi.CihazId == cihazId.Value);
        }

        if (sarfMalzemeId.HasValue)
        {
            sorgu = sorgu.Where(stokHareketi => stokHareketi.SarfMalzemeId == sarfMalzemeId.Value);
        }

        return await sorgu
            .OrderByDescending(stokHareketi => stokHareketi.OlusturulmaTarihi)
            .ToListAsync(cancellationToken);
    }
}
