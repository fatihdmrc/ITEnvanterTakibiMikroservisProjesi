using KimlikVePersonelServisi.Api.Contracts.Departmanlar;
using KimlikVePersonelServisi.Api.Contracts.Kimlik;
using KimlikVePersonelServisi.Api.Contracts.Kullanicilar;
using KimlikVePersonelServisi.Api.Contracts.Personeller;

namespace KimlikVePersonelServisi.Api.Services;

public interface IKimlikPersonelServisi
{
    Task<IReadOnlyCollection<DepartmanCevap>> DepartmanlariListeleAsync(CancellationToken cancellationToken = default);
    Task<DepartmanCevap?> DepartmanGetirAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Sonuc<DepartmanCevap>> DepartmanOlusturAsync(DepartmanOlusturIstek istek, CancellationToken cancellationToken = default);
    Task<Sonuc<DepartmanCevap>> DepartmanGuncelleAsync(Guid id, DepartmanGuncelleIstek istek, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PersonelCevap>> PersonelleriListeleAsync(CancellationToken cancellationToken = default);
    Task<PersonelCevap?> PersonelGetirAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Sonuc<PersonelCevap>> PersonelOlusturAsync(PersonelOlusturIstek istek, CancellationToken cancellationToken = default);
    Task<Sonuc<PersonelCevap>> PersonelGuncelleAsync(Guid id, PersonelGuncelleIstek istek, CancellationToken cancellationToken = default);
    Task<Sonuc<PersonelCevap>> PersoneliIstenAyrildiYapAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<KullaniciCevap>> KullanicilariListeleAsync(CancellationToken cancellationToken = default);
    Task<Sonuc<KullaniciCevap>> KullaniciOlusturAsync(KullaniciOlusturIstek istek, CancellationToken cancellationToken = default);
    Task<Sonuc<GirisCevap>> GirisYapAsync(GirisIstek istek, CancellationToken cancellationToken = default);
}
