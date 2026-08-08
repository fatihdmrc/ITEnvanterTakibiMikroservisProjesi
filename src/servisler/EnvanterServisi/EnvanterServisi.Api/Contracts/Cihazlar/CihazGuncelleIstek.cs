namespace EnvanterServisi.Api.Contracts.Cihazlar;

public sealed record CihazGuncelleIstek(
    string? SeriNumarasi,
    string? AssetTag,
    string Ad,
    string Marka,
    string Model,
    Guid KategoriId,
    Guid LokasyonId,
    DateOnly EnvantereGirisTarihi);
