using ZimmetServisi.Api.Contracts.Zimmetler;
using ZimmetServisi.Api.Domain.Enums;

namespace ZimmetServisi.Api.Services;

public interface IZimmetServisi
{
    Task<IReadOnlyCollection<ZimmetCevap>> ZimmetleriListeleAsync(Guid? personelId = null, Guid? cihazId = null, ZimmetDurumu? durum = null, CancellationToken cancellationToken = default);
    Task<ZimmetCevap?> ZimmetGetirAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Sonuc<ZimmetCevap>> ZimmetOlusturAsync(ZimmetOlusturIstek istek, Guid kullaniciId, string bearerToken, CancellationToken cancellationToken = default);
    Task<Sonuc<ZimmetCevap>> IadeAlindiAsync(Guid id, ZimmetIadeAlindiIstek istek, Guid kullaniciId, string bearerToken, CancellationToken cancellationToken = default);
    Task<Sonuc<ZimmetCevap>> IadeKontroluTamamlaAsync(Guid id, ZimmetIadeKontroluIstek istek, Guid kullaniciId, string bearerToken, CancellationToken cancellationToken = default);
}
