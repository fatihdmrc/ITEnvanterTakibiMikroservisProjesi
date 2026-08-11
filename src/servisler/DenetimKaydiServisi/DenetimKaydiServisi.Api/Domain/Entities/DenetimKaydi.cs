using DenetimKaydiServisi.Api.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DenetimKaydiServisi.Api.Domain.Entities;

public sealed class DenetimKaydi
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonIgnoreIfNull]
    public Guid? EventId { get; set; }

    [BsonRepresentation(BsonType.String)]
    public DenetimKayitTuru KayitTuru { get; set; }

    public string KaynakServis { get; set; } = string.Empty;
    public string? EventAdi { get; set; }
    public string? IslemTuru { get; set; }
    public string? VarlikTuru { get; set; }
    public string? VarlikId { get; set; }
    public string? VarlikAdi { get; set; }
    public Guid? KullaniciId { get; set; }
    public string? Rol { get; set; }
    public string? HttpMetodu { get; set; }
    public string? Endpoint { get; set; }
    public DateTime OlusmaZamaniUtc { get; set; }
    public DateTime AlinmaZamaniUtc { get; set; }
    public string? Aciklama { get; set; }
    public string? Payload { get; set; }
}
