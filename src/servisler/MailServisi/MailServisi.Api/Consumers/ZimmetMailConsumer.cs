using DotNetCore.CAP;
using MailServisi.Api.Contracts.Events;
using MailServisi.Api.Sabitler;
using MailServisi.Api.Services;

namespace MailServisi.Api.Consumers;

public sealed class ZimmetMailConsumer(
    IZimmetMailServisi zimmetMailServisi,
    ILogger<ZimmetMailConsumer> logger) : ICapSubscribe
{
    [CapSubscribe(EventAdlari.ZimmetOlusturuldu)]
    public async Task ZimmetOlusturuldu(ZimmetOlusturulduEvent payload, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(MailMesajlari.EventAlindiLogu, EventAdlari.ZimmetOlusturuldu, payload.EventId);
        await zimmetMailServisi.ZimmetOlusturulduMailiGonderAsync(payload, cancellationToken);
        logger.LogInformation(MailMesajlari.EventTetiklenenIslemTamamlandiLogu, EventAdlari.ZimmetOlusturuldu, payload.EventId);
    }

    [CapSubscribe(EventAdlari.ZimmetIadeAlindi)]
    public async Task ZimmetIadeAlindi(ZimmetIadeAlindiEvent payload, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(MailMesajlari.EventAlindiLogu, EventAdlari.ZimmetIadeAlindi, payload.EventId);
        await zimmetMailServisi.ZimmetIadeAlindiMailiGonderAsync(payload, cancellationToken);
        logger.LogInformation(MailMesajlari.EventTetiklenenIslemTamamlandiLogu, EventAdlari.ZimmetIadeAlindi, payload.EventId);
    }

    [CapSubscribe(EventAdlari.ZimmetIadeEdildi)]
    public async Task ZimmetIadeEdildi(ZimmetIadeEdildiEvent payload, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(MailMesajlari.EventAlindiLogu, EventAdlari.ZimmetIadeEdildi, payload.EventId);
        await zimmetMailServisi.ZimmetIadeEdildiMailiGonderAsync(payload, cancellationToken);
        logger.LogInformation(MailMesajlari.EventTetiklenenIslemTamamlandiLogu, EventAdlari.ZimmetIadeEdildi, payload.EventId);
    }
}
