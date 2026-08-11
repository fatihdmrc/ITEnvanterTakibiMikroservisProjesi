using DenetimKaydiServisi.Api.Contracts.DenetimKayitlari;

namespace DenetimKaydiServisi.Api.Services;

public interface IDenetimKaydiServisi
{
    Task<DenetimKaydiListeCevap> ListeleAsync(DenetimKaydiFiltre filtre, CancellationToken cancellationToken = default);
    Task<DenetimKaydiCevap?> GetirAsync(string id, CancellationToken cancellationToken = default);
    Task<DenetimKaydiCevap> CrudKaydiOlusturAsync(CrudDenetimKaydiOlusturIstek istek, CancellationToken cancellationToken = default);
    Task EventKaydiOlusturAsync(EventDenetimKaydiOlusturIstek istek, CancellationToken cancellationToken = default);
}
