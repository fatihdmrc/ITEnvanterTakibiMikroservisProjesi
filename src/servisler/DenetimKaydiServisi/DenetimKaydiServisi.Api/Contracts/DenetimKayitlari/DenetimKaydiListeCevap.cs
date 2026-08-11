namespace DenetimKaydiServisi.Api.Contracts.DenetimKayitlari;

public sealed record DenetimKaydiListeCevap(
    IReadOnlyCollection<DenetimKaydiCevap> Kayitlar,
    long ToplamKayit,
    int Sayfa,
    int SayfaBoyutu);
