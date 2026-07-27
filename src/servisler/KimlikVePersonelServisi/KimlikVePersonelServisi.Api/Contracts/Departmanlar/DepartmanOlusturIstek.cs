namespace KimlikVePersonelServisi.Api.Contracts.Departmanlar;

public sealed record DepartmanOlusturIstek(
    string Ad,
    Guid? SorumluPersonelId);
