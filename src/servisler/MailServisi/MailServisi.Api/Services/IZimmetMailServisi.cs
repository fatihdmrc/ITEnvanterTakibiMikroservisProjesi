using MailServisi.Api.Contracts.Events;

namespace MailServisi.Api.Services;

public interface IZimmetMailServisi
{
    Task ZimmetOlusturulduMailiGonderAsync(ZimmetOlusturulduEvent payload, CancellationToken cancellationToken = default);
    Task ZimmetIadeAlindiMailiGonderAsync(ZimmetIadeAlindiEvent payload, CancellationToken cancellationToken = default);
    Task ZimmetIadeEdildiMailiGonderAsync(ZimmetIadeEdildiEvent payload, CancellationToken cancellationToken = default);
}
