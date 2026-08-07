using EnvanterServisi.Api.Domain.Entities;

namespace EnvanterServisi.Api.Repositories;

public interface IStokHareketiRepository : IGenericRepository<StokHareketi>
{
    Task<IReadOnlyCollection<StokHareketi>> FiltreleAsync(Guid? cihazId = null, Guid? sarfMalzemeId = null, CancellationToken cancellationToken = default);
}
