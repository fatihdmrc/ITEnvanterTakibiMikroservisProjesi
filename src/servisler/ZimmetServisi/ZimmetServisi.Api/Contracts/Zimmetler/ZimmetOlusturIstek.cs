namespace ZimmetServisi.Api.Contracts.Zimmetler;

public sealed record ZimmetOlusturIstek(
    Guid CihazId,
    Guid PersonelId,
    DateOnly? ZimmetTarihi);
