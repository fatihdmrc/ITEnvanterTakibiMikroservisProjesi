using BildirimServisi.Api.Contracts.Bildirimler;
using BildirimServisi.Api.Contracts.Events;
using BildirimServisi.Api.Hubs;
using DotNetCore.CAP;
using Microsoft.AspNetCore.SignalR;

namespace BildirimServisi.Api.Consumers;

public sealed class KritikStokBildirimConsumer(
    IHubContext<BildirimHub> hubContext,
    ILogger<KritikStokBildirimConsumer> logger) : ICapSubscribe
{
    [CapSubscribe(EventAdlari.KritikStokSeviyesineDusuldu)]
    public async Task KritikStokSeviyesineDusuldu(KritikStokSeviyesineDusulduEvent payload, CancellationToken cancellationToken = default)
    {
        var bildirim = new KritikStokBildirimi(
            payload.EventId,
            payload.VarlikTuru,
            payload.KategoriId,
            payload.LokasyonId,
            "Kritik stok uyarısı",
            MesajOlustur(payload),
            payload.CihazModeli,
            payload.SarfMalzemeId,
            payload.SarfMalzemeAdi,
            payload.MevcutMiktar,
            payload.KritikStokSeviyesi,
            payload.OlusmaZamaniUtc,
            DateTime.UtcNow);

        await hubContext.Clients.All.SendAsync("KritikStokBildirimiAlindi", bildirim, cancellationToken);
        logger.LogInformation("Kritik stok bildirimi SignalR ile yayinlandi. EventId: {EventId}", payload.EventId);
    }

    private static string MesajOlustur(KritikStokSeviyesineDusulduEvent payload)
    {
        var varlikAdi = payload.SarfMalzemeAdi
            ?? payload.CihazModeli
            ?? payload.VarlikTuru;

        return $"{varlikAdi} için mevcut miktar {payload.MevcutMiktar}, kritik seviye {payload.KritikStokSeviyesi}.";
    }
}
