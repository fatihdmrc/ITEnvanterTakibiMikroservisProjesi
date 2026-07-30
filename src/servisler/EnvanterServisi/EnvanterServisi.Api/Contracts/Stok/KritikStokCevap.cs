namespace EnvanterServisi.Api.Contracts.Stok;

public sealed record KritikStokCevap(
    string VarlikTuru,
    Guid KategoriId,
    Guid LokasyonId,
    string? Model,
    int MevcutMiktar,
    int KritikStokSeviyesi);
