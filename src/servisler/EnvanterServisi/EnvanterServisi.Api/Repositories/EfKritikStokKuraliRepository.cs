using EnvanterServisi.Api.Data;
using EnvanterServisi.Api.Domain.Entities;

namespace EnvanterServisi.Api.Repositories;

public sealed class EfKritikStokKuraliRepository(EnvanterDbContext dbContext)
    : EfGenericRepository<KritikStokKurali>(dbContext), IKritikStokKuraliRepository;
