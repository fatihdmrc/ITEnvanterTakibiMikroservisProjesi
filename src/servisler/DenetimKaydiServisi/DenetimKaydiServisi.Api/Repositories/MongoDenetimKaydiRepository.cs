using DenetimKaydiServisi.Api.Contracts.DenetimKayitlari;
using DenetimKaydiServisi.Api.Domain.Entities;
using DenetimKaydiServisi.Api.Options;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DenetimKaydiServisi.Api.Repositories;

public sealed class MongoDenetimKaydiRepository : IDenetimKaydiRepository
{
    private readonly IMongoCollection<DenetimKaydi> collection;

    public MongoDenetimKaydiRepository(IMongoDatabase database, IOptions<MongoDbAyarlari> ayarlar)
    {
        collection = database.GetCollection<DenetimKaydi>(ayarlar.Value.DenetimKayitlariCollectionName);
    }

    public async Task IndeksleriOlusturAsync(CancellationToken cancellationToken = default)
    {
        var eventIdIndex = new CreateIndexModel<DenetimKaydi>(
            Builders<DenetimKaydi>.IndexKeys.Ascending(kayit => kayit.EventId),
            new CreateIndexOptions { Unique = true, Sparse = true, Name = "UX_DenetimKayitlari_EventId" });

        var filtreIndex = new CreateIndexModel<DenetimKaydi>(
            Builders<DenetimKaydi>.IndexKeys
                .Descending(kayit => kayit.AlinmaZamaniUtc)
                .Ascending(kayit => kayit.KaynakServis)
                .Ascending(kayit => kayit.KayitTuru)
                .Ascending(kayit => kayit.VarlikTuru)
                .Ascending(kayit => kayit.VarlikId),
            new CreateIndexOptions { Name = "IX_DenetimKayitlari_Filtreler" });

        await collection.Indexes.CreateManyAsync([eventIdIndex, filtreIndex], cancellationToken);
    }

    public async Task<(IReadOnlyCollection<DenetimKaydi> Kayitlar, long ToplamKayit)> ListeleAsync(
        DenetimKaydiFiltre filtre,
        CancellationToken cancellationToken = default)
    {
        var filter = FiltreOlustur(filtre);
        var sayfa = Math.Max(filtre.Sayfa, 1);
        var sayfaBoyutu = Math.Clamp(filtre.SayfaBoyutu, 1, 100);
        var atlanacakKayit = (sayfa - 1) * sayfaBoyutu;

        var toplam = await collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var kayitlar = await collection
            .Find(filter)
            .SortByDescending(kayit => kayit.AlinmaZamaniUtc)
            .Skip(atlanacakKayit)
            .Limit(sayfaBoyutu)
            .ToListAsync(cancellationToken);

        return (kayitlar, toplam);
    }

    public async Task<DenetimKaydi?> GetirAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return null;
        }

        return await collection
            .Find(kayit => kayit.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task KaydetAsync(DenetimKaydi kayit, CancellationToken cancellationToken = default)
        => collection.InsertOneAsync(kayit, cancellationToken: cancellationToken);

    private static FilterDefinition<DenetimKaydi> FiltreOlustur(DenetimKaydiFiltre filtre)
    {
        var builder = Builders<DenetimKaydi>.Filter;
        var filters = new List<FilterDefinition<DenetimKaydi>>();

        if (filtre.KayitTuru.HasValue)
        {
            filters.Add(builder.Eq(kayit => kayit.KayitTuru, filtre.KayitTuru.Value));
        }

        if (!string.IsNullOrWhiteSpace(filtre.EventAdi))
        {
            filters.Add(builder.Eq(kayit => kayit.EventAdi, filtre.EventAdi));
        }

        if (!string.IsNullOrWhiteSpace(filtre.IslemTuru))
        {
            filters.Add(builder.Eq(kayit => kayit.IslemTuru, filtre.IslemTuru));
        }

        if (!string.IsNullOrWhiteSpace(filtre.KaynakServis))
        {
            filters.Add(builder.Eq(kayit => kayit.KaynakServis, filtre.KaynakServis));
        }

        if (!string.IsNullOrWhiteSpace(filtre.VarlikTuru))
        {
            filters.Add(builder.Eq(kayit => kayit.VarlikTuru, filtre.VarlikTuru));
        }

        if (!string.IsNullOrWhiteSpace(filtre.VarlikId))
        {
            filters.Add(builder.Eq(kayit => kayit.VarlikId, filtre.VarlikId));
        }

        if (filtre.KullaniciId.HasValue)
        {
            filters.Add(builder.Eq(kayit => kayit.KullaniciId, filtre.KullaniciId.Value));
        }

        if (filtre.Baslangic.HasValue)
        {
            filters.Add(builder.Gte(kayit => kayit.OlusmaZamaniUtc, filtre.Baslangic.Value.ToUniversalTime()));
        }

        if (filtre.Bitis.HasValue)
        {
            filters.Add(builder.Lte(kayit => kayit.OlusmaZamaniUtc, filtre.Bitis.Value.ToUniversalTime()));
        }

        return filters.Count == 0 ? builder.Empty : builder.And(filters);
    }
}
