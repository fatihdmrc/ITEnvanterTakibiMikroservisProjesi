namespace ZimmetServisi.Api.Domain.Entities;

public abstract class TemelEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
}
