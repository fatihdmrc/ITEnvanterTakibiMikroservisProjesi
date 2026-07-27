using KimlikVePersonelServisi.Api.Data;
using KimlikVePersonelServisi.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KimlikVePersonelServisi.Api.Repositories;

public sealed class EfDepartmanRepository(KimlikPersonelDbContext dbContext) : IDepartmanRepository
{
    public IReadOnlyCollection<Departman> Listele()
    {
        return dbContext.Departmanlar
            .AsNoTracking()
            .OrderBy(departman => departman.Ad)
            .ToList();
    }

    public Departman? Getir(Guid id)
    {
        return dbContext.Departmanlar.FirstOrDefault(departman => departman.Id == id);
    }

    public bool VarMi(Guid id)
    {
        return dbContext.Departmanlar.Any(departman => departman.Id == id);
    }

    public bool AktifVarMi(Guid id)
    {
        return dbContext.Departmanlar.Any(departman => departman.Id == id && departman.AktifMi);
    }

    public bool AdKullaniliyorMu(string ad, Guid? haricDepartmanId = null)
    {
        return dbContext.Departmanlar.Any(departman =>
            departman.Ad == ad &&
            (!haricDepartmanId.HasValue || departman.Id != haricDepartmanId.Value));
    }

    public void Ekle(Departman departman)
    {
        dbContext.Departmanlar.Add(departman);
    }

    public void Kaydet()
    {
        dbContext.SaveChanges();
    }
}
