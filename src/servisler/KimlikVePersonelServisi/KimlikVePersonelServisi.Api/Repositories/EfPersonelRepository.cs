using KimlikVePersonelServisi.Api.Data;
using KimlikVePersonelServisi.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KimlikVePersonelServisi.Api.Repositories;

public sealed class EfPersonelRepository(KimlikPersonelDbContext dbContext) : IPersonelRepository
{
    public IReadOnlyCollection<Personel> Listele()
    {
        return dbContext.Personeller
            .AsNoTracking()
            .OrderBy(personel => personel.Ad)
            .ThenBy(personel => personel.Soyad)
            .ToList();
    }

    public Personel? Getir(Guid id)
    {
        return dbContext.Personeller.FirstOrDefault(personel => personel.Id == id);
    }

    public bool VarMi(Guid id)
    {
        return dbContext.Personeller.Any(personel => personel.Id == id);
    }

    public bool EmailKullaniliyorMu(string email, Guid? haricPersonelId = null)
    {
        return dbContext.Personeller.Any(personel =>
            personel.Email == email &&
            (!haricPersonelId.HasValue || personel.Id != haricPersonelId.Value));
    }

    public void Ekle(Personel personel)
    {
        dbContext.Personeller.Add(personel);
    }

    public void Kaydet()
    {
        dbContext.SaveChanges();
    }
}
