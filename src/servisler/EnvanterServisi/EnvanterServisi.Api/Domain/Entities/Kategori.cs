using EnvanterServisi.Api.Domain.Enums;

namespace EnvanterServisi.Api.Domain.Entities;

public sealed class Kategori : TemelEntity
{
    public string Ad { get; set; } = string.Empty;
    public Guid? UstKategoriId { get; set; }
    public VarlikTuru VarlikTuru { get; set; }
    public int? KritikStokSeviyesi { get; set; }
    public bool AktifMi { get; set; } = true;
}
