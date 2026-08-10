using ZimmetServisi.Api.Data;
using ZimmetServisi.Api.Domain.Entities;
using ZimmetServisi.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ZimmetServisi.Api.Repositories;

public sealed class EfZimmetRepository(ZimmetDbContext dbContext) : EfGenericRepository<Zimmet>(dbContext), IZimmetRepository
{
    public async Task<IReadOnlyCollection<Zimmet>> FiltreleAsync(
        Guid? personelId = null,
        Guid? cihazId = null,
        ZimmetDurumu? durum = null,
        CancellationToken cancellationToken = default)
    {
        var sorgu = DbSet.AsNoTracking().AsQueryable();

        if (personelId.HasValue)
        {
            sorgu = sorgu.Where(zimmet => zimmet.PersonelId == personelId.Value);
        }

        if (cihazId.HasValue)
        {
            sorgu = sorgu.Where(zimmet => zimmet.CihazId == cihazId.Value);
        }

        if (durum.HasValue)
        {
            sorgu = sorgu.Where(zimmet => zimmet.Durum == durum.Value);
        }

        return await sorgu
            .OrderByDescending(zimmet => zimmet.OlusturulmaTarihi)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> AcikZimmetVarMiAsync(Guid cihazId, Guid? haricZimmetId = null, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(zimmet =>
            zimmet.CihazId == cihazId
            && zimmet.Id != haricZimmetId
            && (zimmet.Durum == ZimmetDurumu.Aktif || zimmet.Durum == ZimmetDurumu.IadeSurecinde),
            cancellationToken);
    }
}
