using EnvanterServisi.Api.Domain.Enums;

namespace EnvanterServisi.Api.Contracts.Kategoriler;

public sealed record KategoriGuncelleIstek(
    string Ad,
    Guid? UstKategoriId,
    VarlikTuru VarlikTuru,
    int? KritikStokSeviyesi,
    bool AktifMi);
