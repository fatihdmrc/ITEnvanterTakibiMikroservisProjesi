using MailKit.Net.Smtp;
using MailKit.Security;
using MailServisi.Api.Contracts.Events;
using MailServisi.Api.Options;
using Microsoft.Extensions.Options;
using MimeKit;

namespace MailServisi.Api.Services;

public sealed class GmailZimmetMailServisi(
    IOptions<GmailAyarlari> gmailAyarlari,
    ILogger<GmailZimmetMailServisi> logger) : IZimmetMailServisi
{
    public async Task ZimmetOlusturulduMailiGonderAsync(ZimmetOlusturulduEvent payload, CancellationToken cancellationToken = default)
    {
        var ayarlar = gmailAyarlari.Value;
        AyarlariDogrula(ayarlar);

        var denemeSayisi = Math.Max(ayarlar.DenemeSayisi, 1);
        var sonHata = default(Exception);

        for (var deneme = 1; deneme <= denemeSayisi; deneme++)
        {
            try
            {
                await MailGonderAsync(payload, ayarlar, cancellationToken);
                logger.LogInformation("Zimmet maili gonderildi. EventId: {EventId}, Deneme: {Deneme}", payload.EventId, deneme);
                return;
            }
            catch (Exception exception) when (deneme < denemeSayisi && !cancellationToken.IsCancellationRequested)
            {
                sonHata = exception;
                logger.LogWarning(exception, "Zimmet maili gonderilemedi. EventId: {EventId}, Deneme: {Deneme}/{DenemeSayisi}", payload.EventId, deneme, denemeSayisi);
                await Task.Delay(TimeSpan.FromSeconds(Math.Max(ayarlar.DenemeBeklemeSaniye, 1)), cancellationToken);
            }
            catch (Exception exception)
            {
                sonHata = exception;
                break;
            }
        }

        throw new InvalidOperationException($"{denemeSayisi} deneme sonunda zimmet maili gonderilemedi.", sonHata);
    }

    private static async Task MailGonderAsync(ZimmetOlusturulduEvent payload, GmailAyarlari ayarlar, CancellationToken cancellationToken)
    {
        var aliciEmail = ayarlar.TestModu ? ayarlar.TestAliciEmail : payload.PersonelEmail;

        var mesaj = new MimeMessage();
        mesaj.From.Add(MailboxAddress.Parse(ayarlar.GonderenEmail));
        mesaj.To.Add(MailboxAddress.Parse(aliciEmail));
        mesaj.Subject = "IT ekipman zimmet bilgilendirmesi";

        var assetTag = string.IsNullOrWhiteSpace(payload.CihazAssetTag) ? "-" : payload.CihazAssetTag;
        var testModuNotu = ayarlar.TestModu
            ? $"<p><strong>Test modu:</strong> Gerçek alıcı {payload.PersonelEmail}, test alıcısı {ayarlar.TestAliciEmail}.</p>"
            : string.Empty;

        mesaj.Body = new BodyBuilder
        {
            HtmlBody = $"""
                <p>Merhaba,</p>
                <p>Aşağıdaki ekipman zimmet kaydı oluşturuldu.</p>
                <ul>
                    <li><strong>Personel:</strong> {payload.PersonelAdSoyad}</li>
                    <li><strong>Personel e-posta:</strong> {payload.PersonelEmail}</li>
                    <li><strong>Cihaz:</strong> {payload.CihazAd}</li>
                    <li><strong>Asset tag:</strong> {assetTag}</li>
                    <li><strong>Zimmet tarihi:</strong> {payload.ZimmetTarihi:dd.MM.yyyy}</li>
                </ul>
                {testModuNotu}
                <p>Bu e-posta IT Envanter Takip Sistemi test mail akışı tarafından gönderilmiştir.</p>
                """,
            TextBody = $"""
                Merhaba,

                Aşağıdaki ekipman zimmet kaydı oluşturuldu.

                Personel: {payload.PersonelAdSoyad}
                Personel e-posta: {payload.PersonelEmail}
                Cihaz: {payload.CihazAd}
                Asset tag: {assetTag}
                Zimmet tarihi: {payload.ZimmetTarihi:dd.MM.yyyy}

                {(ayarlar.TestModu ? $"Test modu: Gerçek alıcı {payload.PersonelEmail}, test alıcısı {ayarlar.TestAliciEmail}." : string.Empty)}

                Bu e-posta IT Envanter Takip Sistemi test mail akışı tarafından gönderilmiştir.
                """
        }.ToMessageBody();

        using var smtpClient = new SmtpClient();
        await smtpClient.ConnectAsync(ayarlar.SmtpHost, ayarlar.SmtpPort, SecureSocketOptions.StartTls, cancellationToken);
        await smtpClient.AuthenticateAsync(ayarlar.KullaniciAdi, ayarlar.AppPassword, cancellationToken);
        await smtpClient.SendAsync(mesaj, cancellationToken);
        await smtpClient.DisconnectAsync(true, cancellationToken);
    }

    private static void AyarlariDogrula(GmailAyarlari ayarlar)
    {
        if (string.IsNullOrWhiteSpace(ayarlar.KullaniciAdi))
        {
            throw new InvalidOperationException("Gmail kullanıcı adı tanımlı değil.");
        }

        if (string.IsNullOrWhiteSpace(ayarlar.AppPassword))
        {
            throw new InvalidOperationException("Gmail app password tanımlı değil.");
        }

        if (string.IsNullOrWhiteSpace(ayarlar.GonderenEmail))
        {
            throw new InvalidOperationException("Gönderen e-posta adresi tanımlı değil.");
        }

        if (ayarlar.TestModu && string.IsNullOrWhiteSpace(ayarlar.TestAliciEmail))
        {
            throw new InvalidOperationException("Test modu açıkken test alıcı e-posta adresi tanımlı olmalıdır.");
        }
    }
}
