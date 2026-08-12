using DotNetCore.CAP;
using MailServisi.Api.Contracts.Events;
using MailServisi.Api.Services;

namespace MailServisi.Api.Consumers;

public sealed class ZimmetMailConsumer(IZimmetMailServisi zimmetMailServisi) : ICapSubscribe
{
    [CapSubscribe(EventAdlari.ZimmetOlusturuldu)]
    public Task ZimmetOlusturuldu(ZimmetOlusturulduEvent payload, CancellationToken cancellationToken = default)
        => zimmetMailServisi.ZimmetOlusturulduMailiGonderAsync(payload, cancellationToken);

    [CapSubscribe(EventAdlari.ZimmetIadeAlindi)]
    public Task ZimmetIadeAlindi(ZimmetIadeAlindiEvent payload, CancellationToken cancellationToken = default)
        => zimmetMailServisi.ZimmetIadeAlindiMailiGonderAsync(payload, cancellationToken);

    [CapSubscribe(EventAdlari.ZimmetIadeEdildi)]
    public Task ZimmetIadeEdildi(ZimmetIadeEdildiEvent payload, CancellationToken cancellationToken = default)
        => zimmetMailServisi.ZimmetIadeEdildiMailiGonderAsync(payload, cancellationToken);
}
