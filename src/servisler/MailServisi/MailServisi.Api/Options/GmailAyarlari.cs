namespace MailServisi.Api.Options;

public sealed class GmailAyarlari
{
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string KullaniciAdi { get; set; } = string.Empty;
    public string AppPassword { get; set; } = string.Empty;
    public string GonderenEmail { get; set; } = "fathdmrc01@gmail.com";
    public bool TestModu { get; set; } = true;
    public string TestAliciEmail { get; set; } = "fathdmrc01@gmail.com";
    public int DenemeSayisi { get; set; } = 3;
    public int DenemeBeklemeSaniye { get; set; } = 2;
}
