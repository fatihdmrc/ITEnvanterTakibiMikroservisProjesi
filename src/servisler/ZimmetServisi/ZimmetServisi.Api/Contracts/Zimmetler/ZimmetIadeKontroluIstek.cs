using ZimmetServisi.Api.Domain.Enums;

namespace ZimmetServisi.Api.Contracts.Zimmetler;

public sealed record ZimmetIadeKontroluIstek(
    IadeKontrolDurumu IadeKontrolDurumu,
    string? IadeNotu);
