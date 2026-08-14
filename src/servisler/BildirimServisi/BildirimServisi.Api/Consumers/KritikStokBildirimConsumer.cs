using BildirimServisi.Api.Contracts.Bildirimler;
using BildirimServisi.Api.Contracts.Events;
using BildirimServisi.Api.Hubs;
using BildirimServisi.Api.Sabitler;
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
        logger.LogInformation(BildirimMesajlari.EventAlindiLogu, EventAdlari.KritikStokSeviyesineDusuldu, payload.EventId);

        var bildirim = new KritikStokBildirimi(
            payload.EventId,
            payload.VarlikTuru,
            payload.KategoriId,
            payload.LokasyonId,
            BildirimMesajlari.KritikStokUyarisiBasligi,
            MesajOlustur(payload),
            payload.CihazModeli,
            payload.SarfMalzemeId,
            payload.SarfMalzemeAdi,
            payload.MevcutMiktar,
            payload.KritikStokSeviyesi,
            payload.OlusmaZamaniUtc,
            DateTime.UtcNow);

        await hubContext.Clients.All.SendAsync(BildirimMesajlari.KritikStokBildirimiAlindiMetodu, bildirim, cancellationToken);
        logger.LogInformation(BildirimMesajlari.KritikStokYayinlandiLogu, payload.EventId);
        logger.LogInformation(BildirimMesajlari.EventTetiklenenIslemTamamlandiLogu, EventAdlari.KritikStokSeviyesineDusuldu, payload.EventId);
    }

    private static string MesajOlustur(KritikStokSeviyesineDusulduEvent payload)
    {
        var varlikAdi = payload.SarfMalzemeAdi
            ?? payload.CihazModeli
            ?? payload.VarlikTuru;

        return BildirimMesajlari.KritikStokMesaji(varlikAdi, payload.MevcutMiktar, payload.KritikStokSeviyesi);
    }
}
