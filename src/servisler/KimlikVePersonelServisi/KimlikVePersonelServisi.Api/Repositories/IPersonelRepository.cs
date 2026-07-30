using KimlikVePersonelServisi.Api.Domain.Entities;

namespace KimlikVePersonelServisi.Api.Repositories;

// Personel tablosuna ait sorgu ve kayıt işlemleri EF Core detayını servis katmanından saklar.
public interface IPersonelRepository : IGenericRepository<Personel>
{
    Task<bool> EmailKullaniliyorMuAsync(string email, Guid? haricPersonelId = null, CancellationToken cancellationToken = default);
}
