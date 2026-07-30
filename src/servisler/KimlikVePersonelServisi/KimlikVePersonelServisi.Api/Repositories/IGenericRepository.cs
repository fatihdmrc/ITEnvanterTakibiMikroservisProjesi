namespace KimlikVePersonelServisi.Api.Repositories;

// Ortak CRUD operasyonlarını tek yerde toplar; entity'ye özel sorgular ilgili repository arayüzünde kalır.
public interface IGenericRepository<TEntity>
    where TEntity : class
{
    Task<IReadOnlyCollection<TEntity>> ListeleAsync(CancellationToken cancellationToken = default);
    Task<TEntity?> GetirAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> VarMiAsync(Guid id, CancellationToken cancellationToken = default);
    void Ekle(TEntity entity);
    Task KaydetAsync(CancellationToken cancellationToken = default);
}
