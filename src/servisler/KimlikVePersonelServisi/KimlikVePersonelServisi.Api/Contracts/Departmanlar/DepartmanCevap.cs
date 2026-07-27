namespace KimlikVePersonelServisi.Api.Contracts.Departmanlar;

public sealed record DepartmanCevap(
    Guid Id,
    string Ad,
    Guid? SorumluPersonelId,
    bool AktifMi);
