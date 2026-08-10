namespace ZimmetServisi.Api.Contracts.Zimmetler;

public sealed record ZimmetIadeAlindiIstek(
    DateOnly? IadeTarihi,
    string? IadeNotu);
