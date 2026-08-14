namespace BildirimServisi.Api.Sabitler;

public static class BildirimMesajlari
{
    public const string AdminRolu = "Admin";
    public const string ITPersoneliRolu = "ITPersoneli";
    public const string AdminVeyaITPersoneliPolicy = "AdminVeyaITPersoneli";
    public const string JwtAyarlariYok = "JWT ayarları bulunamadı.";
    public const string MongoDbAyarlariYok = "MongoDB ayarları bulunamadı.";
    public const string MvcClientCors = "MvcClientCors";
    public const string VarsayilanMvcClientAdresi = "http://localhost:5010";
    public const string BildirimHubYolu = "/hubs/bildirim";
    public const string KritikStokBildirimiAlindiMetodu = "KritikStokBildirimiAlindi";
    public const string KritikStokUyarisiBasligi = "Kritik stok uyarısı";
    public const string KritikStokYayinlandiLogu = "Kritik stok bildirimi SignalR ile yayınlandı. EventId: {EventId}";
    public const string EventAlindiLogu = "EVENT | ALINDI | Servis: BildirimServisi | Event: {EventAdi} | EventId: {EventId} | TetiklenecekIslem: SignalR bildirimi yayınlama";
    public const string EventTetiklenenIslemTamamlandiLogu = "EVENT | TETIKLENDI | Servis: BildirimServisi | Event: {EventAdi} | EventId: {EventId} | TamamlananIslem: SignalR bildirimi yayınlandı";

    public static string KritikStokMesaji(string varlikAdi, int mevcutMiktar, int kritikStokSeviyesi)
        => $"{varlikAdi} için mevcut miktar {mevcutMiktar}, kritik seviye {kritikStokSeviyesi}.";
}
