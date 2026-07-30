using EnvanterServisi.Api.Domain.Enums;

namespace EnvanterServisi.Api.Contracts.Kategoriler;

public sealed record KategoriOlusturIstek(
    string Ad,
    Guid? UstKategoriId,
    VarlikTuru VarlikTuru,
    int? KritikStokSeviyesi);
