using ZimmetServisi.Api.Domain.Entities;

namespace ZimmetServisi.Api.Repositories;

public interface IGenericRepository<TEntity>
    where TEntity : TemelEntity
{
    Task<IReadOnlyCollection<TEntity>> ListeleAsync(CancellationToken cancellationToken = default);
    Task<TEntity?> GetirAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> VarMiAsync(Guid id, CancellationToken cancellationToken = default);
    void Ekle(TEntity entity);
    Task KaydetAsync(CancellationToken cancellationToken = default);
}
