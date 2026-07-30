using KimlikVePersonelServisi.Api.Domain.Entities;

namespace KimlikVePersonelServisi.Api.Repositories;

// Repository yalnızca veri erişimini soyutlar; validasyon ve iş kararları servis katmanında kalır.
public interface IDepartmanRepository : IGenericRepository<Departman>
{
    Task<bool> AktifVarMiAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> AdKullaniliyorMuAsync(string ad, Guid? haricDepartmanId = null, CancellationToken cancellationToken = default);
}
