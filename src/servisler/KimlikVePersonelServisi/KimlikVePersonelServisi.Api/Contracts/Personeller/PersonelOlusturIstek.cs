namespace KimlikVePersonelServisi.Api.Contracts.Personeller;

public sealed record PersonelOlusturIstek(
    string Ad,
    string Soyad,
    string Email,
    Guid DepartmanId,
    string Unvan,
    bool DepartmanSorumlusuMu,
    DateOnly IseGirisTarihi);
