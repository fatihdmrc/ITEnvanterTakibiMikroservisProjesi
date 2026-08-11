# Servis İletişimleri Dokümanı

## 1. Genel Yaklaşım

Sistem YARP tabanlı Api Gateway ile çalışacaktır. Client uygulama istekleri önce ApiGateway'e gelecek, ApiGateway isteği ilgili mikroservise yönlendirecektir. Her servis kendi portundan ve kendi Swagger arayüzünden erişilebilir olmaya devam edecektir. Servisler arası iletişim üç şekilde tasarlanır:

- Client-server yönlendirmesi için ApiGateway
- Senkron HTTP çağrıları
- Asenkron DotNetCore.CAP + RabbitMQ eventleri

Senkron HTTP çağrıları işlem anında doğrulama gerektiğinde kullanılır. CAP + RabbitMQ eventleri ise gerçekleşen olayların diğer servislere güvenilir şekilde duyurulması için kullanılır. Event publish işlemlerinde Outbox Pattern uygulanır.

Faz 6 güncel uygulamasında ZimmetServisi, KimlikVePersonelServisi ve EnvanterServisi ile işlem anı doğrulama için doğrudan HTTP üzerinden konuşmaya devam eder. Başarılı domain işlemleri ayrıca DotNetCore.CAP Outbox üzerinden RabbitMQ'ya event olarak yayınlanır.

## 2. Servis Portları

Önerilen portlar:

| Servis | Port | Açıklama |
| --- | --- | --- |
| ApiGateway | 5005 | YARP tabanlı merkezi client giriş noktası |
| KimlikVePersonelServisi | 5000 | Kullanıcı, personel ve departman işlemleri |
| EnvanterServisi | 5001 | Cihaz, sarf malzeme, kategori, lokasyon ve stok işlemleri |
| ZimmetServisi | 5002 | Zimmet oluşturma ve iade işlemleri |
| DenetimKaydiServisi | 5003 | Audit/event log sorgulama |
| BildirimServisi | 5004 | Kritik stok SignalR bildirim paneli |

## 3. Senkron HTTP Çağrıları

### Zimmet Oluşturma Öncesi Personel Kontrolü

Kaynak servis:

- ZimmetServisi

Hedef servis:

- KimlikVePersonelServisi

Amaç:

- Personelin var olduğunu doğrulamak
- Personelin aktif olduğunu doğrulamak
- Personelin işten ayrılmamış olduğunu doğrulamak

Beklenen sonuç:

- Personel aktifse zimmet akışı devam eder.
- Personel pasif veya işten ayrılmışsa zimmet oluşturulmaz.

### Zimmet Oluşturma Öncesi Cihaz Kontrolü

Kaynak servis:

- ZimmetServisi

Hedef servis:

- EnvanterServisi

Amaç:

- Cihazın var olduğunu doğrulamak
- Cihazın zimmetlenebilir durumda olduğunu doğrulamak
- Cihazın aktif zimmette veya iade incelemesinde olmadığını doğrulamak

Beklenen sonuç:

- Cihaz uygunsa zimmet akışı devam eder.
- Cihaz uygun değilse zimmet oluşturulmaz.

### Cihaz Durumu Güncelleme

Kaynak servis:

- ZimmetServisi

Hedef servis:

- EnvanterServisi

Amaç:

- Zimmet oluşturulduğunda cihazı `Zimmetli` durumuna almak
- İade sürecinde cihazı `Incelemede` durumuna almak
- Fiziki kontrol sonucunda cihazı `Kullanilabilir`, `Bakimda`, `HurdaIskarta` veya `HasarliTeslimAlindi` durumuna almak

Faz 5 uygulama kararı:

- ZimmetServisi cihaz tablosuna doğrudan yazmaz.
- `POST /api/cihazlar/{id}/durum-hareketleri` endpointi kullanılır.
- Zimmet oluştururken `Zimmetlendi`, iade alırken `ZimmetIadeAlindi` nedeni gönderilir.
- İade kontrolünde sonuç `BakimdanDondu`, `Ariza`, `HurdaIskarta` veya `HasarliTeslimAlindi` hareketlerinden birine çevrilir.

## 4. CAP + RabbitMQ Eventleri

Bu bölüm Faz 6 ile uygulanan asenkron iletişim modelidir. Event üretici taraf KimlikVePersonelServisi, EnvanterServisi ve ZimmetServisi içinde aktiftir. Faz 7 itibarıyla DenetimKaydiServisi event consumer olarak eklenmiştir. Faz 9 itibarıyla BildirimServisi kritik stok event consumer olarak eklenmiştir.

Event bus:

- DotNetCore.CAP

Mesaj taşıyıcı:

- RabbitMQ

Outbox kararı:

- Event üreten servislerde iş verisi ve event kaydı aynı transaction içinde yazılır.
- CAP Outbox kaydı daha sonra RabbitMQ'ya yayınlar.
- Böylece veritabanı kaydı başarılı olup event publish işleminin kaybolması riski azaltılır.
- CAP outbox şemaları servis bazında ayrıdır: `cap_kimlik`, `cap_envanter`, `cap_zimmet`.

RabbitMQ exchange:

- `inventory.events`

Eventler:

| Event Adı | Routing Key | Üreten Servis | Tüketen Servisler | Amaç |
| --- | --- | --- | --- | --- |
| ZimmetOlusturuldu | `zimmet.olusturuldu` | ZimmetServisi | DenetimKaydiServisi | Zimmet oluşturma olayını audit için duyurmak |
| ZimmetIadeAlindi | `zimmet.iade-alindi` | ZimmetServisi | DenetimKaydiServisi | Zimmetin fiziki kontrol sürecine alındığını duyurmak |
| ZimmetIadeEdildi | `zimmet.iade-edildi` | ZimmetServisi | DenetimKaydiServisi | Zimmet iade olayını audit için duyurmak |
| KritikStokSeviyesineDusuldu | `stok.kritik-seviyeye-dusuldu` | EnvanterServisi | DenetimKaydiServisi, BildirimServisi | Kritik stok bildirimi üretmek |
| CihazDurumuDegisti | `cihaz.durumu-degisti` | EnvanterServisi | DenetimKaydiServisi | Cihaz durum değişikliğini kaydetmek |
| CihazKontroleAlindi | `cihaz.kontrole-alindi` | ZimmetServisi | DenetimKaydiServisi | İade sonrası fiziki kontrol sürecini duyurmak |
| CihazHasarliTeslimAlindi | `cihaz.hasarli-teslim-alindi` | ZimmetServisi | DenetimKaydiServisi | Hasarlı iade bilgisini duyurmak |
| PersonelIstenAyrildi | `personel.isten-ayrildi` | KimlikVePersonelServisi | ZimmetServisi (gelecek), DenetimKaydiServisi | Ayrılan personelin aktif zimmetlerinin görünür olması |

## 5. Standart Event İçeriği

Faz 6 uygulamasında event adı CAP routing key üzerinden taşınır. Payload tarafında domain olayı için gerekli alanlar bulunur.

| Alan | Açıklama |
| --- | --- |
| EventId | Event benzersiz kimliği |
| OlusmaZamaniUtc | Event oluşma zamanı |
| DomainId alanları | PersonelId, CihazId, ZimmetId, SarfMalzemeId gibi evente özel referanslar |
| KullaniciId alanları | İşlemi başlatan veya kontrolü yapan kullanıcı id bilgisi, event için gerekliyse |
| Evente özel alanlar | Durum, neden, kritik stok seviyesi, iade notu gibi domain verileri |

Kullanıcı bağlamı:

- Gerekli eventlerde kullanıcı id bilgisi taşınır.
- Rol ve CorrelationId alanları DenetimKaydiServisi fazında genişletilebilir.
- Bu bilgiler audit log ve süreç izlenebilirliği için kullanılır.

## 6. Örnek Akış: Zimmet Oluşturma

1. Kullanıcı MVC client veya Swagger üzerinden ZimmetServisi'ne zimmet oluşturma isteği gönderir.
2. ZimmetServisi, KimlikVePersonelServisi üzerinden personeli doğrular.
3. ZimmetServisi, EnvanterServisi üzerinden cihazı doğrular.
4. ZimmetServisi ilgili cihaz için açık zimmet olup olmadığını kendi veritabanından kontrol eder.
5. Cihaz ve personel uygunsa EnvanterServisi'ne `Zimmetlendi` cihaz durum hareketi gönderilir.
6. EnvanterServisi cihazı `Zimmetli` durumuna alır.
7. ZimmetServisi zimmet kaydını `Aktif` durumuyla oluşturur.
8. ZimmetServisi aynı transaction içinde `zimmet.olusturuldu` Outbox kaydını oluşturur.
9. CAP, Outbox kaydını RabbitMQ `inventory.events` exchange'ine yayınlar.

## 7. Örnek Akış: Personel İşten Ayrılma

1. Admin veya ITPersoneli, ApiGateway üzerinden personeli işten ayrıldı durumuna alma isteği gönderir.
2. ApiGateway isteği KimlikVePersonelServisi'ne yönlendirir.
3. KimlikVePersonelServisi, personel durumunu `IstenAyrildi` yapar.
4. Aynı servis ilgili kullanıcı hesabını pasifleştirir.
5. KimlikVePersonelServisi aynı transaction içinde Outbox kaydını oluşturur.
6. CAP, `PersonelIstenAyrildi` eventini RabbitMQ'ya yayınlar.
7. ZimmetServisi consumer davranışı sonraki fazlarda ele alınacaktır.
8. DenetimKaydiServisi Faz 7'de olayı MongoDB'ye kaydeder.

## 8. Faz 7 CRUD Audit İletişimi

KimlikVePersonelServisi, EnvanterServisi ve ZimmetServisi başarılı `POST`, `PUT`, `PATCH` ve `DELETE` mutasyonlarından sonra DenetimKaydiServisi'ne best-effort HTTP çağrısı yapar.

```text
POST http://localhost:5003/api/denetim-kayitlari/crud
```

Bu çağrı JWT token'ı forward eder. DenetimKaydiServisi geçici olarak kapalıysa ana işlem başarısız sayılmaz; kaynak servis yalnızca uyarı logu üretir.

## 9. Faz 8 Redis Cache Kullanımı

Redis, servisler arası bir iletişim kanalı değildir; EnvanterServisi içinde sık okunan referans verileri hızlandırmak için kullanılan cache katmanıdır.

Kapsam:

- `GET /api/kategoriler` listeleme akışı
- `GET /api/lokasyonlar` listeleme akışı

Cache anahtarları:

```text
envanter:kategoriler:v1
envanter:lokasyonlar:v1
```

Davranış:

- EnvanterServisi önce Redis cache'i okumayı dener.
- Cache yoksa veya okunamazsa veri PostgreSQL'den alınır.
- PostgreSQL'den alınan liste Redis'e 30 dakikalık süreyle yazılır.
- Kategori veya lokasyon oluşturma/güncelleme başarılı olursa ilgili cache anahtarı temizlenir.
- Redis kapalıysa API sözleşmesi değişmez; ana okuma akışı PostgreSQL üzerinden devam eder.

## 10. Faz 9 SignalR Bildirim İletişimi

BildirimServisi, client-server canlı bildirim kanalıdır. Servisler arası veri sahipliği veya iş kuralı taşımaz.

CAP tüketimi:

```text
Event: stok.kritik-seviyeye-dusuldu
Consumer group: bildirim-servisi
Kaynak: EnvanterServisi
Hedef: BildirimServisi
```

SignalR hub:

```text
GET/WS http://localhost:5004/hubs/bildirim
Client metodu: KritikStokBildirimiAlindi
```

Davranış:

- MVC client, oturumdaki JWT token ile BildirimServisi hub'ına bağlanır.
- Hub yalnızca `Admin` ve `ITPersoneli` rollerini kabul eder.
- `PersonelKullanicisi` rolü canlı bildirim bağlantısı kuramaz.
- BildirimServisi yalnızca kritik stok eventini canlı bildirime dönüştürür.
- Zimmet, cihaz durumu, audit veya personel eventleri SignalR bildirimi üretmez.
- Bildirimler kalıcı saklanmaz; geçmiş için DenetimKaydiServisi kayıtları kullanılır.
