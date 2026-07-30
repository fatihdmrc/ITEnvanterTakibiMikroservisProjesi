using EnvanterServisi.Api.Domain.Enums;

namespace EnvanterServisi.Api.Contracts.Cihazlar;

public sealed record CihazGuncelleIstek(
    string? SeriNumarasi,
    string? AssetTag,
    string Ad,
    string Marka,
    string Model,
    Guid KategoriId,
    Guid LokasyonId,
    CihazDurumu Durum,
    DateOnly EnvantereGirisTarihi,
    DateOnly? EnvanterdenCikisTarihi,
    EldenCikarmaTipi EldenCikarmaTipi,
    string? EldenCikarmaAciklamasi,
    string? SatilanKisiVeyaKurum,
    bool AktifMi,
    bool ToplamVarligaDahilMi);
