namespace KimlikVePersonelServisi.Api.Contracts.Departmanlar;

public sealed record DepartmanGuncelleIstek(
    string Ad,
    Guid? SorumluPersonelId,
    bool AktifMi);
