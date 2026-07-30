using EnvanterServisi.Api.Domain.Entities;

namespace EnvanterServisi.Api.Repositories;

public interface ISarfMalzemeRepository : IGenericRepository<SarfMalzeme>
{
    Task<IReadOnlyCollection<SarfMalzeme>> FiltreleAsync(Guid? kategoriId = null, Guid? lokasyonId = null, string? arama = null, CancellationToken cancellationToken = default);
    Task<bool> AdKullaniliyorMuAsync(string ad, Guid kategoriId, Guid lokasyonId, Guid? haricSarfMalzemeId = null, CancellationToken cancellationToken = default);
    Task<int> ToplamMiktarAsync(CancellationToken cancellationToken = default);
}
