namespace EnvanterServisi.Api.Contracts.SarfMalzemeler;

public sealed record SarfMalzemeOlusturIstek(
    string Ad,
    Guid KategoriId,
    Guid LokasyonId,
    int EldekiMiktar,
    int KritikStokSeviyesi,
    string Birim);
