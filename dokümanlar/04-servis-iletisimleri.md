# Servis İletişimleri Dokümanı

## 1. Genel Yaklaşım

Sistem Api Gateway olmadan çalışacaktır. Her servis kendi portundan erişilebilir olacaktır. Servisler arası iletişim iki şekilde tasarlanır:

- Senkron HTTP çağrıları
- Asenkron RabbitMQ eventleri

Senkron HTTP çağrıları işlem anında doğrulama gerektiğinde kullanılır. RabbitMQ eventleri ise gerçekleşen olayların diğer servislere duyurulması için kullanılır.

## 2. Servis Portları

Önerilen portlar:

| Servis | Port | Açıklama |
| --- | --- | --- |
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

## 4. RabbitMQ Eventleri

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
| Payload | Evente özel veri |

## 6. Örnek Akış: Zimmet Oluşturma

1. Kullanıcı ZimmetServisi'ne zimmet oluşturma isteği gönderir.
2. ZimmetServisi, KimlikVePersonelServisi üzerinden personeli doğrular.
3. ZimmetServisi, EnvanterServisi üzerinden cihazı doğrular.
4. Cihaz uygunsa zimmet kaydı oluşturulur.
5. ZimmetServisi, EnvanterServisi'ne cihaz durumunu `Zimmetli` yapmak için istek gönderir.
6. ZimmetServisi `ZimmetOlusturuldu` eventini RabbitMQ'ya yayınlar.
7. DenetimKaydiServisi eventi MongoDB'ye kaydeder.

## 7. Örnek Akış: Personel İşten Ayrılma

1. Admin veya ITPersoneli, KimlikVePersonelServisi üzerinden personeli işten ayrıldı durumuna alır.
2. KimlikVePersonelServisi, personel durumunu `IstenAyrildi` yapar.
3. Aynı servis ilgili kullanıcı hesabını pasifleştirir.
4. KimlikVePersonelServisi `PersonelIstenAyrildi` eventini yayınlar.
5. ZimmetServisi personelin aktif zimmetlerini kontrol eder.
6. Aktif zimmet varsa iade bekliyor durumu üretilir.
7. DenetimKaydiServisi olayı MongoDB'ye kaydeder.
