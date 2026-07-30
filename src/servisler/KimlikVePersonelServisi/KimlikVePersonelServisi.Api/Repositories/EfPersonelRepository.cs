using KimlikVePersonelServisi.Api.Data;
using KimlikVePersonelServisi.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KimlikVePersonelServisi.Api.Repositories;

public sealed class EfPersonelRepository(KimlikPersonelDbContext dbContext)
    : EfGenericRepository<Personel>(dbContext), IPersonelRepository
{
    public override async Task<IReadOnlyCollection<Personel>> ListeleAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .OrderBy(personel => personel.Ad)
            .ThenBy(personel => personel.Soyad)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> EmailKullaniliyorMuAsync(string email, Guid? haricPersonelId = null, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(personel =>
            personel.Email == email &&
            (!haricPersonelId.HasValue || personel.Id != haricPersonelId.Value),
            cancellationToken);
    }
}
