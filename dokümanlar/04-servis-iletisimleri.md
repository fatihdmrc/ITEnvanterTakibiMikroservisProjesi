# Servis İletişimleri Dokümanı

## 1. Genel Yaklaşım

Sistem YARP tabanlı Api Gateway ile çalışacaktır. Client uygulama istekleri önce ApiGateway'e gelecek, ApiGateway isteği ilgili mikroservise yönlendirecektir. Her servis kendi portundan ve kendi Swagger arayüzünden erişilebilir olmaya devam edecektir. Servisler arası iletişim üç şekilde tasarlanır:

- Client-server yönlendirmesi için ApiGateway
- Senkron HTTP çağrıları
- Asenkron DotNetCore.CAP + RabbitMQ eventleri

Senkron HTTP çağrıları işlem anında doğrulama gerektiğinde kullanılır. CAP + RabbitMQ eventleri ise gerçekleşen olayların diğer servislere güvenilir şekilde duyurulması için kullanılır. Event publish işlemlerinde Outbox Pattern uygulanır.

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
- Fiziki kontrol sonucunda cihazı `Kullanilabilir`, `Bakimda` veya `HurdaIskarta` durumuna almak

## 4. CAP + RabbitMQ Eventleri

Event bus:

- DotNetCore.CAP

Mesaj taşıyıcı:

- RabbitMQ

Outbox kararı:

- Event üreten servislerde iş verisi ve event kaydı aynı transaction içinde yazılır.
- CAP Outbox kaydı daha sonra RabbitMQ'ya yayınlar.
- Böylece veritabanı kaydı başarılı olup event publish işleminin kaybolması riski azaltılır.

RabbitMQ exchange:

- `inventory.events`

Eventler:

| Event | Üreten Servis | Tüketen Servisler | Amaç |
| --- | --- | --- | --- |
| ZimmetOlusturuldu | ZimmetServisi | DenetimKaydiServisi | Zimmet oluşturma olayını audit için duyurmak |
| ZimmetIadeEdildi | ZimmetServisi | DenetimKaydiServisi | Zimmet iade olayını audit için duyurmak |
| StokAzaldi | EnvanterServisi | DenetimKaydiServisi | Stok azalma olayını audit için duyurmak |
| KritikStokSeviyesineDusuldu | EnvanterServisi | DenetimKaydiServisi, BildirimServisi | Kritik stok bildirimi üretmek |
| CihazDurumuDegisti | EnvanterServisi | DenetimKaydiServisi | Cihaz durum değişikliğini kaydetmek |
| CihazKontroleAlindi | ZimmetServisi veya EnvanterServisi | DenetimKaydiServisi | İade sonrası fiziki kontrol sürecini duyurmak |
| CihazHasarliTeslimAlindi | ZimmetServisi | DenetimKaydiServisi | Hasarlı iade bilgisini duyurmak |
| CihazHurdayaAyrildi | EnvanterServisi | DenetimKaydiServisi | Hurda/ıskarta bilgisini duyurmak |
| PersonelIstenAyrildi | KimlikVePersonelServisi | ZimmetServisi, DenetimKaydiServisi | Ayrılan personelin aktif zimmetlerinin görünür olması |

## 5. Standart Event İçeriği

Tüm eventlerde aşağıdaki alanlar bulunmalıdır:

| Alan | Açıklama |
| --- | --- |
| EventId | Event benzersiz kimliği |
| EventAdi | Event adı |
| OccurredAt | Event oluşma zamanı |
| KaynakServis | Eventi üreten servis |
| CorrelationId | İlgili işlem akışını takip etmek için kullanılan kimlik |
| KullaniciId | İşlemi başlatan kullanıcı |
| PersonelId | İşlemi başlatan kullanıcının bağlı olduğu personel kaydı |
| Rol | İşlemi başlatan kullanıcının sistem rolü |
| Payload | Evente özel veri |

Kullanıcı bağlamı:

- Gerekli eventlerde `KullaniciId`, `PersonelId`, `Rol` ve `CorrelationId` taşınır.
- Bu bilgiler audit log ve süreç izlenebilirliği için kullanılır.

## 6. Örnek Akış: Zimmet Oluşturma

1. Kullanıcı ApiGateway'e zimmet oluşturma isteği gönderir.
2. ApiGateway isteği ZimmetServisi'ne yönlendirir.
3. ZimmetServisi, KimlikVePersonelServisi üzerinden personeli doğrular.
4. ZimmetServisi, EnvanterServisi üzerinden cihazı doğrular.
5. Cihaz uygunsa zimmet kaydı oluşturulur.
6. ZimmetServisi, EnvanterServisi'ne cihaz durumunu `Zimmetli` yapmak için istek gönderir.
7. ZimmetServisi aynı transaction içinde Outbox kaydını oluşturur.
8. CAP, `ZimmetOlusturuldu` eventini RabbitMQ'ya yayınlar.
9. DenetimKaydiServisi eventi MongoDB'ye kaydeder.

## 7. Örnek Akış: Personel İşten Ayrılma

1. Admin veya ITPersoneli, ApiGateway üzerinden personeli işten ayrıldı durumuna alma isteği gönderir.
2. ApiGateway isteği KimlikVePersonelServisi'ne yönlendirir.
3. KimlikVePersonelServisi, personel durumunu `IstenAyrildi` yapar.
4. Aynı servis ilgili kullanıcı hesabını pasifleştirir.
5. KimlikVePersonelServisi aynı transaction içinde Outbox kaydını oluşturur.
6. CAP, `PersonelIstenAyrildi` eventini RabbitMQ'ya yayınlar.
7. ZimmetServisi personelin aktif zimmetlerini kontrol eder.
8. Aktif zimmet varsa iade bekliyor durumu üretilir.
9. DenetimKaydiServisi olayı MongoDB'ye kaydeder.
