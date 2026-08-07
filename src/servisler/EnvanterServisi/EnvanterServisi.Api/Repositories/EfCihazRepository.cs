using EnvanterServisi.Api.Data;
using EnvanterServisi.Api.Domain.Entities;
using EnvanterServisi.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnvanterServisi.Api.Repositories;

public sealed class EfCihazRepository(EnvanterDbContext dbContext)
    : EfGenericRepository<Cihaz>(dbContext), ICihazRepository
{
    public override async Task<IReadOnlyCollection<Cihaz>> ListeleAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .OrderBy(cihaz => cihaz.Ad)
            .ThenBy(cihaz => cihaz.AssetTag)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Cihaz>> FiltreleAsync(Guid? kategoriId = null, Guid? lokasyonId = null, bool? aktifMi = null, CihazDurumu? durum = null, string? arama = null, CancellationToken cancellationToken = default)
    {
        var sorgu = DbSet.AsNoTracking();

        if (kategoriId.HasValue)
        {
            sorgu = sorgu.Where(cihaz => cihaz.KategoriId == kategoriId.Value);
        }

        if (lokasyonId.HasValue)
        {
            sorgu = sorgu.Where(cihaz => cihaz.LokasyonId == lokasyonId.Value);
        }

        if (aktifMi.HasValue)
        {
            sorgu = sorgu.Where(cihaz => cihaz.AktifMi == aktifMi.Value);
        }

        if (durum.HasValue)
        {
            sorgu = sorgu.Where(cihaz => cihaz.Durum == durum.Value);
        }

        if (!string.IsNullOrWhiteSpace(arama))
        {
            var temizArama = arama.Trim();
            sorgu = sorgu.Where(cihaz =>
                cihaz.Ad.Contains(temizArama) ||
                cihaz.Marka.Contains(temizArama) ||
                cihaz.Model.Contains(temizArama) ||
                (cihaz.SeriNumarasi != null && cihaz.SeriNumarasi.Contains(temizArama)) ||
                (cihaz.AssetTag != null && cihaz.AssetTag.Contains(temizArama)));
        }

        return await sorgu
            .OrderBy(cihaz => cihaz.Ad)
            .ThenBy(cihaz => cihaz.AssetTag)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SeriNumarasiVeyaAssetTagKullaniliyorMuAsync(string? seriNumarasi, string? assetTag, Guid? haricCihazId = null, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(cihaz =>
            (!haricCihazId.HasValue || cihaz.Id != haricCihazId.Value) &&
            ((!string.IsNullOrWhiteSpace(seriNumarasi) && cihaz.SeriNumarasi == seriNumarasi) ||
             (!string.IsNullOrWhiteSpace(assetTag) && cihaz.AssetTag == assetTag)),
            cancellationToken);
    }

    public async Task<int> SonAssetTagSiraNumarasiAsync(CancellationToken cancellationToken = default)
    {
        var assetTagler = await DbSet
            .AsNoTracking()
            .Where(cihaz => cihaz.AssetTag != null && cihaz.AssetTag.StartsWith("BT-"))
            .Select(cihaz => cihaz.AssetTag!)
            .ToListAsync(cancellationToken);

        return assetTagler
            .Select(AssetTagSiraNumarasiniCoz)
            .DefaultIfEmpty(0)
            .Max();
    }

    public async Task<int> ToplamVarlikSayisiAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(cihaz => cihaz.AktifMi && cihaz.ToplamVarligaDahilMi, cancellationToken);
    }

    public async Task<int> KullanilabilirStokSayisiAsync(Guid? kategoriId = null, Guid? lokasyonId = null, string? model = null, CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(cihaz =>
            cihaz.AktifMi &&
            cihaz.Durum == CihazDurumu.Kullanilabilir &&
            (!kategoriId.HasValue || cihaz.KategoriId == kategoriId.Value) &&
            (!lokasyonId.HasValue || cihaz.LokasyonId == lokasyonId.Value) &&
            (string.IsNullOrWhiteSpace(model) || cihaz.Model == model),
            cancellationToken);
    }

    private static int AssetTagSiraNumarasiniCoz(string assetTag)
    {
        return int.TryParse(assetTag["BT-".Length..], out var siraNumarasi)
            ? siraNumarasi
            : 0;
    }
}
