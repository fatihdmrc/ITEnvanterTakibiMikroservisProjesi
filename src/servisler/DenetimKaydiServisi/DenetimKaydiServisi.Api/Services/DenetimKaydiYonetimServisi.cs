using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using DenetimKaydiServisi.Api.Contracts.DenetimKayitlari;
using DenetimKaydiServisi.Api.Domain.Entities;
using DenetimKaydiServisi.Api.Domain.Enums;
using DenetimKaydiServisi.Api.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DenetimKaydiServisi.Api.Services;

public sealed class DenetimKaydiYonetimServisi(
    IDenetimKaydiRepository repository,
    ILogger<DenetimKaydiYonetimServisi> logger) : IDenetimKaydiServisi
{
    private static readonly JsonSerializerOptions JsonAyarlari = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<DenetimKaydiListeCevap> ListeleAsync(DenetimKaydiFiltre filtre, CancellationToken cancellationToken = default)
    {
        var sayfa = Math.Max(filtre.Sayfa, 1);
        var sayfaBoyutu = Math.Clamp(filtre.SayfaBoyutu, 1, 100);
        var normalFiltre = filtre with { Sayfa = sayfa, SayfaBoyutu = sayfaBoyutu };
        var (kayitlar, toplamKayit) = await repository.ListeleAsync(normalFiltre, cancellationToken);

        return new DenetimKaydiListeCevap(
            kayitlar.Select(CevabaDonustur).ToList(),
            toplamKayit,
            sayfa,
            sayfaBoyutu);
    }

    public async Task<DenetimKaydiCevap?> GetirAsync(string id, CancellationToken cancellationToken = default)
    {
        var kayit = await repository.GetirAsync(id, cancellationToken);
        return kayit is null ? null : CevabaDonustur(kayit);
    }

    public async Task<DenetimKaydiCevap> CrudKaydiOlusturAsync(CrudDenetimKaydiOlusturIstek istek, CancellationToken cancellationToken = default)
    {
        var kayit = new DenetimKaydi
        {
            Id = ObjectId.GenerateNewId().ToString(),
            KayitTuru = DenetimKayitTuru.Crud,
            KaynakServis = istek.KaynakServis,
            IslemTuru = istek.IslemTuru,
            VarlikTuru = istek.VarlikTuru,
            VarlikId = istek.VarlikId,
            VarlikAdi = istek.VarlikAdi,
            KullaniciId = istek.KullaniciId,
            Rol = istek.Rol,
            HttpMetodu = istek.HttpMetodu,
            Endpoint = istek.Endpoint,
            Aciklama = istek.Aciklama,
            Payload = istek.Payload,
            OlusmaZamaniUtc = (istek.OlusmaZamaniUtc ?? DateTime.UtcNow).ToUniversalTime(),
            AlinmaZamaniUtc = DateTime.UtcNow
        };

        await repository.KaydetAsync(kayit, cancellationToken);
        return CevabaDonustur(kayit);
    }

    public async Task EventKaydiOlusturAsync(EventDenetimKaydiOlusturIstek istek, CancellationToken cancellationToken = default)
    {
        var kayit = new DenetimKaydi
        {
            Id = ObjectId.GenerateNewId().ToString(),
            EventId = istek.EventId,
            KayitTuru = DenetimKayitTuru.Event,
            KaynakServis = istek.KaynakServis,
            EventAdi = istek.EventAdi,
            VarlikTuru = istek.VarlikTuru,
            VarlikId = istek.VarlikId,
            VarlikAdi = istek.VarlikAdi,
            KullaniciId = istek.KullaniciId,
            Aciklama = istek.Aciklama,
            Payload = JsonSerializer.Serialize(istek.Payload, JsonAyarlari),
            OlusmaZamaniUtc = istek.OlusmaZamaniUtc.ToUniversalTime(),
            AlinmaZamaniUtc = DateTime.UtcNow
        };

        try
        {
            await repository.KaydetAsync(kayit, cancellationToken);
        }
        catch (MongoWriteException exception) when (exception.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            logger.LogInformation("Event daha once kaydedildigi icin atlandi. EventId: {EventId}, EventAdi: {EventAdi}", istek.EventId, istek.EventAdi);
        }
    }

    private static DenetimKaydiCevap CevabaDonustur(DenetimKaydi kayit)
        => new(
            kayit.Id ?? string.Empty,
            kayit.EventId,
            kayit.KayitTuru,
            kayit.KaynakServis,
            kayit.EventAdi,
            kayit.IslemTuru,
            kayit.VarlikTuru,
            kayit.VarlikId,
            kayit.VarlikAdi,
            kayit.KullaniciId,
            kayit.Rol,
            kayit.HttpMetodu,
            kayit.Endpoint,
            kayit.OlusmaZamaniUtc,
            kayit.AlinmaZamaniUtc,
            kayit.Aciklama,
            kayit.Payload);
}
