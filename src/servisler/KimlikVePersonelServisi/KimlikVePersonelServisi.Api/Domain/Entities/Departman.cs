namespace KimlikVePersonelServisi.Api.Domain.Entities;

public sealed class Departman
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Ad { get; set; } = string.Empty;
    public Guid? SorumluPersonelId { get; set; }
    public bool AktifMi { get; set; } = true;
}
