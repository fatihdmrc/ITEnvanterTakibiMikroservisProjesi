namespace EnvanterServisi.Api.Contracts.Events;

public sealed record CihazDurumuDegistiEvent(
    Guid EventId,
    Guid CihazId,
    string? AssetTag,
    string? SeriNumarasi,
    string OncekiDurum,
    string YeniDurum,
    bool AktifMi,
    bool ToplamVarligaDahilMi,
    string Neden,
    Guid OlusturanKullaniciId,
    DateTime OlusmaZamaniUtc);
