namespace BildirimServisi.Api.Contracts.Bildirimler;

public sealed record KritikStokBildirimi(
    Guid EventId,
    string VarlikTuru,
    Guid KategoriId,
    Guid LokasyonId,
    string Baslik,
    string Mesaj,
    string? CihazModeli,
    Guid? SarfMalzemeId,
    string? SarfMalzemeAdi,
    int MevcutMiktar,
    int KritikStokSeviyesi,
    DateTime OlusmaZamaniUtc,
    DateTime YayinlanmaZamaniUtc);
