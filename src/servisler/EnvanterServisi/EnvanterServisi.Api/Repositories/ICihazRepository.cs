using EnvanterServisi.Api.Domain.Entities;
using EnvanterServisi.Api.Domain.Enums;

namespace EnvanterServisi.Api.Repositories;

public interface ICihazRepository : IGenericRepository<Cihaz>
{
    Task<IReadOnlyCollection<Cihaz>> FiltreleAsync(Guid? kategoriId = null, Guid? lokasyonId = null, CihazDurumu? durum = null, string? arama = null, CancellationToken cancellationToken = default);
    Task<bool> SeriNumarasiVeyaAssetTagKullaniliyorMuAsync(string? seriNumarasi, string? assetTag, Guid? haricCihazId = null, CancellationToken cancellationToken = default);
    Task<int> ToplamVarlikSayisiAsync(CancellationToken cancellationToken = default);
    Task<int> KullanilabilirStokSayisiAsync(Guid? kategoriId = null, Guid? lokasyonId = null, string? model = null, CancellationToken cancellationToken = default);
}
