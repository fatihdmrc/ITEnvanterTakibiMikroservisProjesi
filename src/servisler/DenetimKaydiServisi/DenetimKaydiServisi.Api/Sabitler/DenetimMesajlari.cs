namespace DenetimKaydiServisi.Api.Sabitler;

public static class DenetimMesajlari
{
    public const string AdminRolu = "Admin";
    public const string ITPersoneliRolu = "ITPersoneli";
    public const string JwtAyarlariYok = "JWT ayarları bulunamadı.";
    public const string MongoDbAyarlariYok = "MongoDB ayarları bulunamadı.";
    public const string DenetimKaydiBulunamadi = "Denetim kaydı bulunamadı.";
    public const string EventAlindiLogu = "EVENT | ALINDI | Servis: DenetimKaydiServisi | Event: {EventAdi} | EventId: {EventId} | TetiklenecekIslem: Audit kaydı oluşturma";
    public const string EventTetiklenenIslemTamamlandiLogu = "EVENT | TETIKLENDI | Servis: DenetimKaydiServisi | Event: {EventAdi} | EventId: {EventId} | TamamlananIslem: Audit kaydı oluşturuldu";
}
