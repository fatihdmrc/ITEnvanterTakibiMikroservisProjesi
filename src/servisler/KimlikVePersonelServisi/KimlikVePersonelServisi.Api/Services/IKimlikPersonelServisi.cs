using KimlikVePersonelServisi.Api.Contracts.Departmanlar;
using KimlikVePersonelServisi.Api.Contracts.Kimlik;
using KimlikVePersonelServisi.Api.Contracts.Kullanicilar;
using KimlikVePersonelServisi.Api.Contracts.Personeller;

namespace KimlikVePersonelServisi.Api.Services;

public interface IKimlikPersonelServisi
{
    IReadOnlyCollection<DepartmanCevap> DepartmanlariListele();
    DepartmanCevap? DepartmanGetir(Guid id);
    Sonuc<DepartmanCevap> DepartmanOlustur(DepartmanOlusturIstek istek);
    Sonuc<DepartmanCevap> DepartmanGuncelle(Guid id, DepartmanGuncelleIstek istek);
    IReadOnlyCollection<PersonelCevap> PersonelleriListele();
    PersonelCevap? PersonelGetir(Guid id);
    Sonuc<PersonelCevap> PersonelOlustur(PersonelOlusturIstek istek);
    Sonuc<PersonelCevap> PersonelGuncelle(Guid id, PersonelGuncelleIstek istek);
    Sonuc<PersonelCevap> PersoneliIstenAyrildiYap(Guid id);
    IReadOnlyCollection<KullaniciCevap> KullanicilariListele();
    Sonuc<KullaniciCevap> KullaniciOlustur(KullaniciOlusturIstek istek);
    Sonuc<GirisCevap> GirisYap(GirisIstek istek);
}
