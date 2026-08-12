using MailServisi.Api.Contracts.Events;

namespace MailServisi.Api.Services;

public interface IZimmetMailServisi
{
    Task ZimmetOlusturulduMailiGonderAsync(ZimmetOlusturulduEvent payload, CancellationToken cancellationToken = default);
}
