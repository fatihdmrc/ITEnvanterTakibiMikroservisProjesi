namespace MailServisi.Api.Sabitler;

public static class MailMesajlari
{
    public const string ZimmetOlusturulduKonu = "IT ekipman zimmet bilgilendirmesi";
    public const string ZimmetIadeAlindiKonu = "IT ekipman iade alındı bilgilendirmesi";
    public const string ZimmetIadeEdildiKonu = "IT ekipman iade kontrolü tamamlandı";

    public const string GmailKullaniciAdiEksik = "Gmail kullanıcı adı tanımlı değil.";
    public const string GmailAppPasswordEksik = "Gmail app password tanımlı değil.";
    public const string GmailGonderenEksik = "Gönderen e-posta adresi tanımlı değil.";
    public const string GmailTestAliciEksik = "Test modu açıkken test alıcı e-posta adresi tanımlı olmalıdır.";
    public const string MongoDbAyarlariYok = "MongoDB ayarları bulunamadı.";
    public const string EventAlindiLogu = "EVENT | ALINDI | Servis: MailServisi | Event: {EventAdi} | EventId: {EventId} | TetiklenecekIslem: Zimmet e-postası gönderme";
    public const string EventTetiklenenIslemTamamlandiLogu = "EVENT | TETIKLENDI | Servis: MailServisi | Event: {EventAdi} | EventId: {EventId} | TamamlananIslem: Zimmet e-postası gönderildi";
    public const string MailGonderimBasladiLogu = "MAIL | BASLADI | Tur: {MailTuru} | EventId: {EventId} | GercekAlici: {GercekAlici} | GonderilecekAlici: {AliciEmail} | TestModu: {TestModu} | DenemeSayisi: {DenemeSayisi}";
    public const string MailDenemeBasladiLogu = "MAIL | DENEME | Tur: {MailTuru} | EventId: {EventId} | Deneme: {Deneme}/{DenemeSayisi}";
    public const string MailGonderildiLogu = "MAIL | BASARILI | Tur: {MailTuru} | EventId: {EventId} | Alici: {AliciEmail} | Deneme: {Deneme}/{DenemeSayisi}";
    public const string MailGonderilemediLogu = "MAIL | TEKRAR_DENEYECEK | Tur: {MailTuru} | EventId: {EventId} | Deneme: {Deneme}/{DenemeSayisi}";
    public const string MailKaliciHataLogu = "MAIL | BASARISIZ | Tur: {MailTuru} | EventId: {EventId} | Alici: {AliciEmail} | DenemeSayisi: {DenemeSayisi}";

    public static string MailGonderilemedi(string mailTuru, int denemeSayisi)
        => $"{denemeSayisi} deneme sonunda {mailTuru} maili gönderilemedi.";

    public static string TestModuHtml(string gercekAlici, string testAlici)
        => $"<p><strong>Test modu:</strong> Gerçek alıcı {gercekAlici}, test alıcısı {testAlici}.</p>";

    public static string TestModuText(string gercekAlici, string testAlici)
        => $"Test modu: Gerçek alıcı {gercekAlici}, test alıcısı {testAlici}.";
}
