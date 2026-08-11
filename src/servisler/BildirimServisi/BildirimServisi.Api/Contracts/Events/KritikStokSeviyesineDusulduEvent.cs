namespace BildirimServisi.Api.Contracts.Events;

public sealed record KritikStokSeviyesineDusulduEvent(
    Guid EventId,
    string VarlikTuru,
    Guid KategoriId,
    Guid LokasyonId,
    string? CihazModeli,
    Guid? SarfMalzemeId,
    string? SarfMalzemeAdi,
    int MevcutMiktar,
    int KritikStokSeviyesi,
    DateTime OlusmaZamaniUtc);
