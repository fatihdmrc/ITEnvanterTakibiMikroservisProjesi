using KimlikVePersonelServisi.Api.Domain.Entities;

namespace KimlikVePersonelServisi.Api.Repositories;

// Kullanıcı hesabı işlemleri kimlik doğrulama akışının veri tarafını yönetir.
public interface IKullaniciRepository
{
    IReadOnlyCollection<Kullanici> Listele();
    Kullanici? KullaniciAdiIleGetir(string kullaniciAdi);
    bool KullaniciAdiKullaniliyorMu(string kullaniciAdi);
    bool PersonelIcinHesapVarMi(Guid personelId);
    void Ekle(Kullanici kullanici);
    void PersonelHesaplariniPasiflestir(Guid personelId);
    void Kaydet();
}
