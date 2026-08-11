using DenetimKaydiServisi.Api.Domain.Enums;

namespace DenetimKaydiServisi.Api.Contracts.DenetimKayitlari;

public sealed record DenetimKaydiFiltre(
    DenetimKayitTuru? KayitTuru,
    string? EventAdi,
    string? IslemTuru,
    string? KaynakServis,
    string? VarlikTuru,
    string? VarlikId,
    Guid? KullaniciId,
    DateTime? Baslangic,
    DateTime? Bitis,
    int Sayfa,
    int SayfaBoyutu);
