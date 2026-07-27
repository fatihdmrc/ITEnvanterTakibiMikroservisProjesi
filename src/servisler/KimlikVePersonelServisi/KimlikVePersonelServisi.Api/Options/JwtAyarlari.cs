namespace KimlikVePersonelServisi.Api.Options;

public sealed class JwtAyarlari
{
    // Token'ı hangi servisin ürettiğini belirtir; doğrulamada aynı değer beklenir.
    public string Issuer { get; set; } = string.Empty;

    // Token'ın hangi istemci/uygulama için üretildiğini belirtir.
    public string Audience { get; set; } = string.Empty;

    // Token imzasında kullanılan gizli anahtardır; canlı ortamda appsettings yerine güvenli secret store'da tutulmalıdır.
    public string SigningKey { get; set; } = string.Empty;

    // Kullanıcının tekrar giriş yapmadan token'ı kaç dakika kullanabileceğini belirler.
    public int GecerlilikDakikasi { get; set; } = 480;
}
