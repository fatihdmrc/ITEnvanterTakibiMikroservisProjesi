using KimlikVePersonelServisi.Api.Domain.Entities;

namespace KimlikVePersonelServisi.Api.Repositories;

// Repository yalnızca veri erişimini soyutlar; validasyon ve iş kararları servis katmanında kalır.
public interface IDepartmanRepository
{
    IReadOnlyCollection<Departman> Listele();
    Departman? Getir(Guid id);
    bool VarMi(Guid id);
    bool AktifVarMi(Guid id);
    bool AdKullaniliyorMu(string ad, Guid? haricDepartmanId = null);
    void Ekle(Departman departman);
    void Kaydet();
}
