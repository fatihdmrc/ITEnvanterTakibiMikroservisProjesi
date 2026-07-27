using KimlikVePersonelServisi.Api.Data;
using KimlikVePersonelServisi.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KimlikVePersonelServisi.Api.Repositories;

public sealed class EfKullaniciRepository(KimlikPersonelDbContext dbContext) : IKullaniciRepository
{
    public IReadOnlyCollection<Kullanici> Listele()
    {
        return dbContext.Kullanicilar
            .AsNoTracking()
            .OrderBy(kullanici => kullanici.KullaniciAdi)
            .ToList();
    }

    public Kullanici? KullaniciAdiIleGetir(string kullaniciAdi)
    {
        return dbContext.Kullanicilar.FirstOrDefault(kullanici => kullanici.KullaniciAdi == kullaniciAdi);
    }

    public bool KullaniciAdiKullaniliyorMu(string kullaniciAdi)
    {
        return dbContext.Kullanicilar.Any(kullanici => kullanici.KullaniciAdi == kullaniciAdi);
    }

    public bool PersonelIcinHesapVarMi(Guid personelId)
    {
        return dbContext.Kullanicilar.Any(kullanici => kullanici.PersonelId == personelId);
    }

    public void Ekle(Kullanici kullanici)
    {
        dbContext.Kullanicilar.Add(kullanici);
    }

    public void PersonelHesaplariniPasiflestir(Guid personelId)
    {
        foreach (var kullanici in dbContext.Kullanicilar.Where(kullanici => kullanici.PersonelId == personelId))
        {
            kullanici.AktifMi = false;
        }
    }

    public void Kaydet()
    {
        dbContext.SaveChanges();
    }
}
