namespace EnvanterServisi.Api.Contracts.Stok;

public sealed record StokOzetCevap(
    int ToplamVarlik,
    int KullanilabilirCihazStoku,
    int SarfMalzemeToplamMiktari,
    IReadOnlyCollection<KritikStokCevap> KritikStoklar);
