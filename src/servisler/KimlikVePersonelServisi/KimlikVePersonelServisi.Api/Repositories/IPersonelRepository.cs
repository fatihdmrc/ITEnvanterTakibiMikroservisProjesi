using KimlikVePersonelServisi.Api.Domain.Entities;

namespace KimlikVePersonelServisi.Api.Repositories;

// Personel tablosuna ait sorgu ve kayıt işlemleri EF Core detayını servis katmanından saklar.
public interface IPersonelRepository
{
    IReadOnlyCollection<Personel> Listele();
    Personel? Getir(Guid id);
    bool VarMi(Guid id);
    bool EmailKullaniliyorMu(string email, Guid? haricPersonelId = null);
    void Ekle(Personel personel);
    void Kaydet();
}
