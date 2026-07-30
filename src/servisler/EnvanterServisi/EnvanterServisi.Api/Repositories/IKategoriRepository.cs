using EnvanterServisi.Api.Domain.Entities;
using EnvanterServisi.Api.Domain.Enums;

namespace EnvanterServisi.Api.Repositories;

public interface IKategoriRepository : IGenericRepository<Kategori>
{
    Task<bool> AktifVarMiAsync(Guid id, VarlikTuru? varlikTuru = null, CancellationToken cancellationToken = default);
    Task<bool> AdKullaniliyorMuAsync(string ad, Guid? ustKategoriId, Guid? haricKategoriId = null, CancellationToken cancellationToken = default);
}
