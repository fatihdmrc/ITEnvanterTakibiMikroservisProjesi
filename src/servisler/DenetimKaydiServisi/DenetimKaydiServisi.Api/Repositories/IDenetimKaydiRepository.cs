using DenetimKaydiServisi.Api.Contracts.DenetimKayitlari;
using DenetimKaydiServisi.Api.Domain.Entities;

namespace DenetimKaydiServisi.Api.Repositories;

public interface IDenetimKaydiRepository
{
    Task IndeksleriOlusturAsync(CancellationToken cancellationToken = default);
    Task<(IReadOnlyCollection<DenetimKaydi> Kayitlar, long ToplamKayit)> ListeleAsync(DenetimKaydiFiltre filtre, CancellationToken cancellationToken = default);
    Task<DenetimKaydi?> GetirAsync(string id, CancellationToken cancellationToken = default);
    Task KaydetAsync(DenetimKaydi kayit, CancellationToken cancellationToken = default);
}
