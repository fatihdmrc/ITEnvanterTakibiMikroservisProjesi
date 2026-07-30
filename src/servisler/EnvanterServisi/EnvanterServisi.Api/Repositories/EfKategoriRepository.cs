using EnvanterServisi.Api.Data;
using EnvanterServisi.Api.Domain.Entities;
using EnvanterServisi.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnvanterServisi.Api.Repositories;

public sealed class EfKategoriRepository(EnvanterDbContext dbContext)
    : EfGenericRepository<Kategori>(dbContext), IKategoriRepository
{
    public override async Task<IReadOnlyCollection<Kategori>> ListeleAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .OrderBy(kategori => kategori.VarlikTuru)
            .ThenBy(kategori => kategori.Ad)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> AktifVarMiAsync(Guid id, VarlikTuru? varlikTuru = null, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(kategori =>
            kategori.Id == id &&
            kategori.AktifMi &&
            (!varlikTuru.HasValue || kategori.VarlikTuru == varlikTuru.Value),
            cancellationToken);
    }

    public async Task<bool> AdKullaniliyorMuAsync(string ad, Guid? ustKategoriId, Guid? haricKategoriId = null, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(kategori =>
            kategori.Ad == ad &&
            kategori.UstKategoriId == ustKategoriId &&
            (!haricKategoriId.HasValue || kategori.Id != haricKategoriId.Value),
            cancellationToken);
    }
}
