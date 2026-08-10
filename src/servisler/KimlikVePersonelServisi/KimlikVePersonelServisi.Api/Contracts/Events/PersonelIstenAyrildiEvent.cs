namespace KimlikVePersonelServisi.Api.Contracts.Events;

public sealed record PersonelIstenAyrildiEvent(
    Guid EventId,
    Guid PersonelId,
    string AdSoyad,
    string Email,
    Guid DepartmanId,
    DateOnly IstenAyrilisTarihi,
    DateTime OlusmaZamaniUtc);
