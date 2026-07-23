# Mimari Tasarım Dokümanı

## 1. Mimari Yaklaşım

Sistem mikroservis mimarisiyle tasarlanacaktır. Api Gateway kullanılmayacaktır. Her mikroservis kendi portunda çalışacak, kendi Swagger arayüzüne sahip olacak ve kendi sorumluluk alanındaki verileri yönetecektir.

Ana mimari hedef, servisleri iş alanlarına göre ayırmak ve servisler arası bağımlılığı mümkün olduğunca azaltmaktır. Senkron doğrulamalar HTTP ile yapılacak, sistem genelinde duyurulması gereken olaylar RabbitMQ eventleriyle yayınlanacaktır.

## 2. Servisler

### KimlikVePersonelServisi

Sorumlulukları:

- Kullanıcı girişi
- JWT token üretimi
- Rol bazlı yetkilendirme bilgileri
- Kullanıcı hesabı yönetimi
- Personel yönetimi
- Departman yönetimi
- Personel işten ayrılma süreci

Veritabanı:

- PostgreSQL

Önemli kararlar:

- Sisteme giriş yapan her kullanıcı bir personel kaydına bağlıdır.
- `Kullanici.PersonelId` zorunludur.
- İşten ayrılan personelin kullanıcı hesabı aynı servis içinde pasifleştirilir.
- Personel kayıtları fiziksel olarak silinmez.

### EnvanterServisi

Sorumlulukları:

- Seri numaralı cihaz yönetimi
- Sarf malzeme yönetimi
- Kategori ve alt kategori yönetimi
- Lokasyon yönetimi
- Stok hareketleri
- Kullanılabilir stok hesaplama
- Kritik stok kontrolü

Veritabanı:

- PostgreSQL

Cache:

- Redis

Önemli kararlar:

- Seri numaralı varlıklarda `SeriNumarasi` veya `AssetTag` alanlarından en az biri zorunludur.
- Sarf malzemeleri adet bazlı takip edilir.
- Kullanılabilir stok, seri numaralı cihazlarda cihaz durumlarından hesaplanır.
- Sarf malzemelerde kullanılabilir stok `EldekiMiktar` alanından okunur.

### ZimmetServisi

Sorumlulukları:

- Zimmet oluşturma
- Zimmet iade süreci
- Zimmet geçmişi
- İade fiziki kontrol süreci
- Zimmet fotoğrafları

Veritabanı:

- PostgreSQL

Senkron iletişim:

- Personel doğrulamak için KimlikVePersonelServisi
- Cihaz uygunluğunu kontrol etmek ve cihaz durumunu değiştirmek için EnvanterServisi

Önemli kararlar:

- Bir personele birden fazla cihaz zimmetlenebilir.
- Bir cihaz aynı anda yalnızca bir personele zimmetlenebilir.
- Zimmet iadesi önce `Incelemede` durumuna alınır.
- İncelemedeki cihaz tekrar zimmetlenemez.

### DenetimKaydiServisi

Sorumlulukları:

- RabbitMQ eventlerini tüketmek
- Eventleri MongoDB üzerinde audit log olarak saklamak
- Audit log sorgulama endpointleri sağlamak

Veritabanı:

- MongoDB

Önemli kararlar:

- Zimmet geçmişi silinmez.
- Event logları JSON doküman olarak saklanır.

### BildirimServisi

Sorumlulukları:

- RabbitMQ üzerinden kritik stok eventlerini tüketmek
- SignalR üzerinden kritik stok bildirimi yayınlamak
- Kritik stok bildirim paneli sağlamak

Teknoloji:

- SignalR

Önemli kararlar:

- SignalR bildirimi yalnızca kritik stok seviyesi altına düşüldüğünde üretilecektir.
- Zimmet oluşturma, zimmet iade ve personel işten ayrılma eventleri audit/entegrasyon amacıyla kullanılabilir; bildirim üretmeyecektir.

## 3. Servisler Arası İletişim

### Senkron HTTP İletişimi

Senkron HTTP çağrıları, işlem sırasında hemen doğrulama gereken durumlarda kullanılacaktır.

Örnekler:

- ZimmetServisi, personelin aktif olup olmadığını KimlikVePersonelServisi üzerinden kontrol eder.
- ZimmetServisi, cihazın zimmetlenebilir durumda olup olmadığını EnvanterServisi üzerinden kontrol eder.
- ZimmetServisi, zimmet oluşturma veya iade sürecinde cihaz durumunu EnvanterServisi üzerinden günceller.

### Asenkron RabbitMQ İletişimi

RabbitMQ, sistemde gerçekleşen olayların diğer servislere duyurulması için kullanılacaktır.

Örnekler:

- Zimmet oluşturuldu
- Zimmet iade edildi
- Cihaz durumu değişti
- Stok azaldı
- Kritik stok seviyesine düşüldü
- Personel işten ayrıldı

## 4. Veri Saklama Stratejisi

PostgreSQL:

- KimlikVePersonelServisi operasyonel verileri
- EnvanterServisi operasyonel verileri
- ZimmetServisi operasyonel verileri

MongoDB:

- Audit log
- Event log

Redis:

- Kategori listesi cache
- Lokasyon listesi cache

RabbitMQ:

- Servisler arası event dağıtımı

## 5. Güvenlik Tasarımı

- Authentication JWT ile yapılacaktır.
- Authorization rol bazlı olacaktır.
- Roller: `Admin`, `ITPersoneli`, `PersonelKullanicisi`
- JWT içinde `KullaniciId`, `KullaniciAdi`, `Rol`, `PersonelId` claim'leri bulunacaktır.
- `PersonelId` zorunlu claim olacaktır.
- İşten ayrılan personelin kullanıcı hesabı pasifleştirileceği için token alma hakkı kalkacaktır.
- Şifre minimum 8, maksimum 64 karakter olmalı; en az bir rakam, bir büyük harf, bir küçük harf ve bir sembol içermelidir.

## 6. Ücretsiz Araçlarla Görselleştirme

Bu dokümandaki mimari tasarım şu araçlarla görselleştirilebilir:

- Sistem mimarisi: diagrams.net/draw.io
- Servis iletişimleri: diagrams.net/draw.io veya PlantUML sequence diagram
- Veri modeli: dbdiagram.io veya diagrams.net
- İş akışları: draw.io activity diagram veya PlantUML activity diagram

## 7. Görsel Tasarıma Dönüştürülecek Diyagramlar

- Mikroservis genel mimari diyagramı
- Servisler arası HTTP ve RabbitMQ iletişim diyagramı
- Zimmet oluşturma sequence diagram
- Zimmet iade activity diagram
- Personel işten ayrılma activity diagram
- Kritik stok bildirim akışı
- ER diyagramı
