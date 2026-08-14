using MailKit.Net.Smtp;
using MailKit.Security;
using MailServisi.Api.Contracts.Events;
using MailServisi.Api.Options;
using MailServisi.Api.Sabitler;
using Microsoft.Extensions.Options;
using MimeKit;

namespace MailServisi.Api.Services;

public sealed class GmailZimmetMailServisi(
    IOptions<GmailAyarlari> gmailAyarlari,
    ILogger<GmailZimmetMailServisi> logger) : IZimmetMailServisi
{
    public Task ZimmetOlusturulduMailiGonderAsync(ZimmetOlusturulduEvent payload, CancellationToken cancellationToken = default)
    {
        var assetTag = AssetTagMetni(payload.CihazAssetTag);
        var icerik = new MailIcerigi(
            "zimmet oluşturuldu",
            payload.EventId,
            payload.PersonelEmail,
            MailMesajlari.ZimmetOlusturulduKonu,
            $"""
            <p>Merhaba,</p>
            <p>Aşağıdaki ekipman zimmet kaydı oluşturuldu.</p>
            <ul>
                <li><strong>Personel:</strong> {payload.PersonelAdSoyad}</li>
                <li><strong>Personel e-posta:</strong> {payload.PersonelEmail}</li>
                <li><strong>Cihaz:</strong> {payload.CihazAd}</li>
                <li><strong>Asset tag:</strong> {assetTag}</li>
                <li><strong>Zimmet tarihi:</strong> {payload.ZimmetTarihi:dd.MM.yyyy}</li>
            </ul>
            """,
            $"""
            Merhaba,

            Aşağıdaki ekipman zimmet kaydı oluşturuldu.

            Personel: {payload.PersonelAdSoyad}
            Personel e-posta: {payload.PersonelEmail}
            Cihaz: {payload.CihazAd}
            Asset tag: {assetTag}
            Zimmet tarihi: {payload.ZimmetTarihi:dd.MM.yyyy}
            """);

        return ZimmetMailiGonderAsync(icerik, cancellationToken);
    }

    public Task ZimmetIadeAlindiMailiGonderAsync(ZimmetIadeAlindiEvent payload, CancellationToken cancellationToken = default)
    {
        var assetTag = AssetTagMetni(payload.CihazAssetTag);
        var not = NotMetni(payload.IadeNotu);
        var icerik = new MailIcerigi(
            "zimmet iade alındı",
            payload.EventId,
            payload.PersonelEmail,
            MailMesajlari.ZimmetIadeAlindiKonu,
            $"""
            <p>Merhaba,</p>
            <p>Aşağıdaki ekipmanın zimmet iadesi alındı ve cihaz inceleme sürecine taşındı.</p>
            <ul>
                <li><strong>Personel:</strong> {payload.PersonelAdSoyad}</li>
                <li><strong>Personel e-posta:</strong> {payload.PersonelEmail}</li>
                <li><strong>Cihaz:</strong> {payload.CihazAd}</li>
                <li><strong>Asset tag:</strong> {assetTag}</li>
                <li><strong>İade tarihi:</strong> {payload.IadeTarihi:dd.MM.yyyy}</li>
                <li><strong>İade notu:</strong> {not}</li>
            </ul>
            """,
            $"""
            Merhaba,

            Aşağıdaki ekipmanın zimmet iadesi alındı ve cihaz inceleme sürecine taşındı.

            Personel: {payload.PersonelAdSoyad}
            Personel e-posta: {payload.PersonelEmail}
            Cihaz: {payload.CihazAd}
            Asset tag: {assetTag}
            İade tarihi: {payload.IadeTarihi:dd.MM.yyyy}
            İade notu: {not}
            """);

        return ZimmetMailiGonderAsync(icerik, cancellationToken);
    }

    public Task ZimmetIadeEdildiMailiGonderAsync(ZimmetIadeEdildiEvent payload, CancellationToken cancellationToken = default)
    {
        var assetTag = AssetTagMetni(payload.CihazAssetTag);
        var not = NotMetni(payload.IadeNotu);
        var icerik = new MailIcerigi(
            "zimmet iade edildi",
            payload.EventId,
            payload.PersonelEmail,
            MailMesajlari.ZimmetIadeEdildiKonu,
            $"""
            <p>Merhaba,</p>
            <p>Aşağıdaki ekipmanın iade kontrolü tamamlandı.</p>
            <ul>
                <li><strong>Personel:</strong> {payload.PersonelAdSoyad}</li>
                <li><strong>Personel e-posta:</strong> {payload.PersonelEmail}</li>
                <li><strong>Cihaz:</strong> {payload.CihazAd}</li>
                <li><strong>Asset tag:</strong> {assetTag}</li>
                <li><strong>Kontrol sonucu:</strong> {payload.IadeKontrolDurumu}</li>
                <li><strong>Kontrol notu:</strong> {not}</li>
            </ul>
            """,
            $"""
            Merhaba,

            Aşağıdaki ekipmanın iade kontrolü tamamlandı.

            Personel: {payload.PersonelAdSoyad}
            Personel e-posta: {payload.PersonelEmail}
            Cihaz: {payload.CihazAd}
            Asset tag: {assetTag}
            Kontrol sonucu: {payload.IadeKontrolDurumu}
            Kontrol notu: {not}
            """);

        return ZimmetMailiGonderAsync(icerik, cancellationToken);
    }

    private async Task ZimmetMailiGonderAsync(MailIcerigi icerik, CancellationToken cancellationToken)
    {
        var ayarlar = gmailAyarlari.Value;
        AyarlariDogrula(ayarlar);

        var denemeSayisi = Math.Max(ayarlar.DenemeSayisi, 1);
        var aliciEmail = AliciEmailGetir(icerik, ayarlar);
        var sonHata = default(Exception);

        logger.LogInformation(
            MailMesajlari.MailGonderimBasladiLogu,
            icerik.MailTuru,
            icerik.EventId,
            icerik.GercekAliciEmail,
            aliciEmail,
            ayarlar.TestModu,
            denemeSayisi);

        for (var deneme = 1; deneme <= denemeSayisi; deneme++)
        {
            try
            {
                logger.LogInformation(MailMesajlari.MailDenemeBasladiLogu, icerik.MailTuru, icerik.EventId, deneme, denemeSayisi);
                await MailGonderAsync(icerik, ayarlar, cancellationToken);
                logger.LogInformation(MailMesajlari.MailGonderildiLogu, icerik.MailTuru, icerik.EventId, aliciEmail, deneme, denemeSayisi);
                return;
            }
            catch (Exception exception) when (deneme < denemeSayisi && !cancellationToken.IsCancellationRequested)
            {
                sonHata = exception;
                logger.LogWarning(exception, MailMesajlari.MailGonderilemediLogu, icerik.MailTuru, icerik.EventId, deneme, denemeSayisi);
                await Task.Delay(TimeSpan.FromSeconds(Math.Max(ayarlar.DenemeBeklemeSaniye, 1)), cancellationToken);
            }
            catch (Exception exception)
            {
                sonHata = exception;
                break;
            }
        }

        logger.LogError(sonHata, MailMesajlari.MailKaliciHataLogu, icerik.MailTuru, icerik.EventId, aliciEmail, denemeSayisi);
        throw new InvalidOperationException(MailMesajlari.MailGonderilemedi(icerik.MailTuru, denemeSayisi), sonHata);
    }

    private static async Task MailGonderAsync(MailIcerigi icerik, GmailAyarlari ayarlar, CancellationToken cancellationToken)
    {
        var aliciEmail = AliciEmailGetir(icerik, ayarlar);
        var testModuHtml = ayarlar.TestModu
            ? MailMesajlari.TestModuHtml(icerik.GercekAliciEmail, ayarlar.TestAliciEmail)
            : string.Empty;
        var testModuText = ayarlar.TestModu
            ? $"{Environment.NewLine}{MailMesajlari.TestModuText(icerik.GercekAliciEmail, ayarlar.TestAliciEmail)}"
            : string.Empty;

        var mesaj = new MimeMessage();
        mesaj.From.Add(MailboxAddress.Parse(ayarlar.GonderenEmail));
        mesaj.To.Add(MailboxAddress.Parse(aliciEmail));
        mesaj.Subject = icerik.Konu;
        mesaj.Body = new BodyBuilder
        {
            HtmlBody = $"""
                {icerik.HtmlGovde}
                {testModuHtml}
                <p>Bu e-posta IT Envanter Takip Sistemi test mail akışı tarafından gönderilmiştir.</p>
                """,
            TextBody = $"""
                {icerik.TextGovde}
                {testModuText}

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
            throw new InvalidOperationException(MailMesajlari.GmailKullaniciAdiEksik);
        }

        if (string.IsNullOrWhiteSpace(ayarlar.AppPassword))
        {
            throw new InvalidOperationException(MailMesajlari.GmailAppPasswordEksik);
        }

        if (string.IsNullOrWhiteSpace(ayarlar.GonderenEmail))
        {
            throw new InvalidOperationException(MailMesajlari.GmailGonderenEksik);
        }

        if (ayarlar.TestModu && string.IsNullOrWhiteSpace(ayarlar.TestAliciEmail))
        {
            throw new InvalidOperationException(MailMesajlari.GmailTestAliciEksik);
        }
    }

    private static string AssetTagMetni(string? assetTag)
        => string.IsNullOrWhiteSpace(assetTag) ? "-" : assetTag;

    private static string NotMetni(string? not)
        => string.IsNullOrWhiteSpace(not) ? "-" : not;

    private static string AliciEmailGetir(MailIcerigi icerik, GmailAyarlari ayarlar)
        => ayarlar.TestModu ? ayarlar.TestAliciEmail : icerik.GercekAliciEmail;

    private sealed record MailIcerigi(
        string MailTuru,
        Guid EventId,
        string GercekAliciEmail,
        string Konu,
        string HtmlGovde,
        string TextGovde);
}
