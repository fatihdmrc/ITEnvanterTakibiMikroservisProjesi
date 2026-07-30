using EnvanterServisi.Api.Data;
using EnvanterServisi.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnvanterServisi.Api.Repositories;

public class EfGenericRepository<TEntity>(EnvanterDbContext dbContext) : IGenericRepository<TEntity>
    where TEntity : TemelEntity
{
    protected EnvanterDbContext DbContext { get; } = dbContext;
    protected DbSet<TEntity> DbSet => DbContext.Set<TEntity>();

    public virtual async Task<IReadOnlyCollection<TEntity>> ListeleAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .OrderBy(entity => entity.OlusturulmaTarihi)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> GetirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public virtual async Task<bool> VarMiAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(entity => entity.Id == id, cancellationToken);
    }

    public virtual void Ekle(TEntity entity)
    {
        DbSet.Add(entity);
    }

    public virtual async Task KaydetAsync(CancellationToken cancellationToken = default)
    {
        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
