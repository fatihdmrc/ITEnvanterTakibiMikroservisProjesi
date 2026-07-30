using KimlikVePersonelServisi.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace KimlikVePersonelServisi.Api.Repositories;

public class EfGenericRepository<TEntity>(KimlikPersonelDbContext dbContext) : IGenericRepository<TEntity>
    where TEntity : class
{
    protected KimlikPersonelDbContext DbContext { get; } = dbContext;
    protected DbSet<TEntity> DbSet => DbContext.Set<TEntity>();

    public virtual async Task<IReadOnlyCollection<TEntity>> ListeleAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> GetirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(entity => EF.Property<Guid>(entity, "Id") == id, cancellationToken);
    }

    public virtual async Task<bool> VarMiAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(entity => EF.Property<Guid>(entity, "Id") == id, cancellationToken);
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
