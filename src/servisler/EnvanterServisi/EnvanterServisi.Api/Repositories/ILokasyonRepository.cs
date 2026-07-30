using EnvanterServisi.Api.Domain.Entities;

namespace EnvanterServisi.Api.Repositories;

public interface ILokasyonRepository : IGenericRepository<Lokasyon>
{
    Task<bool> AktifVarMiAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> AdKullaniliyorMuAsync(string ad, Guid? ustLokasyonId, Guid? haricLokasyonId = null, CancellationToken cancellationToken = default);
}
