using ZimmetServisi.Api.Domain.Entities;
using ZimmetServisi.Api.Domain.Enums;

namespace ZimmetServisi.Api.Repositories;

public interface IZimmetRepository : IGenericRepository<Zimmet>
{
    Task<IReadOnlyCollection<Zimmet>> FiltreleAsync(
        Guid? personelId = null,
        Guid? cihazId = null,
        ZimmetDurumu? durum = null,
        CancellationToken cancellationToken = default);

    Task<bool> AcikZimmetVarMiAsync(Guid cihazId, Guid? haricZimmetId = null, CancellationToken cancellationToken = default);
}
