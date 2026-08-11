# Mimari Tasarım Dokümanı

## 1. Mimari Yaklaşım

Sistem mikroservis mimarisiyle tasarlanacaktır. Client istekleri YARP tabanlı Api Gateway üzerinden ilgili mikroservislere yönlendirilecektir. Her mikroservis kendi portunda çalışacak, kendi Swagger arayüzüne sahip olacak ve kendi sorumluluk alanındaki verileri yönetecektir.

Ana mimari hedef, servisleri iş alanlarına göre ayırmak ve servisler arası bağımlılığı mümkün olduğunca azaltmaktır. Senkron doğrulamalar HTTP ile yapılacak, sistem genelinde duyurulması gereken olaylar DotNetCore.CAP üzerinden RabbitMQ eventleriyle yayınlanacaktır. Event yayınlama güvenilirliği için Outbox Pattern uygulanacaktır.

## 2. Servisler

### ApiGateway

Sorumlulukları:

- Client uygulamadan gelen istekler için tek giriş noktası olmak
- YARP ile route bazlı yönlendirme yapmak
- Gerekli endpointlerde JWT doğrulama ve rol bazlı yetki politikalarını uygulamak
- İstekleri ilgili mikroservise iletmek
- Client tarafındaki servis adresi karmaşıklığını azaltmak

Teknoloji:

- YARP

Önemli kararlar:

- ApiGateway iş kuralı veya veri sahipliği taşımaz.
- Servisler arası iç HTTP çağrıları ApiGateway üzerinden yapılmak zorunda değildir.
- Her servis kendi Swagger arayüzünü korur; ApiGateway ise client-server trafiği için merkezi giriş noktasıdır.

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
- Zimmet ve iade fotoğrafları Faz 5 kapsamında uygulanmaz.

### DenetimKaydiServisi

Sorumlulukları:

- DotNetCore.CAP ile RabbitMQ eventlerini tüketmek
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

### Asenkron CAP + RabbitMQ İletişimi

DotNetCore.CAP, uygulama tarafındaki event bus katmanı olarak kullanılacaktır. RabbitMQ mesaj taşıyıcı olarak görev yapacaktır. PostgreSQL kullanan servislerde CAP Outbox tabloları aynı servis veritabanı içinde yer alacak ve iş verisi ile event kaydı aynı transaction kapsamında yazılacaktır. Faz 6 itibarıyla event üretici taraf KimlikVePersonelServisi, EnvanterServisi ve ZimmetServisi içinde uygulanmıştır. Faz 7 itibarıyla DenetimKaydiServisi audit consumer olarak eklenmiştir; bildirim consumer tarafı Faz 9 kapsamındadır.

### Faz 7 Denetim Mimarisi

- DenetimKaydiServisi `5003` portunda ayrı API olarak çalışır.
- MongoDB audit/event log depolama için kullanılır.
- CAP/RabbitMQ eventleri DenetimKaydiServisi consumer sınıfları tarafından tüketilir.
- CRUD audit kayıtları kaynak servislerden DenetimKaydiServisi'ne best-effort HTTP çağrısıyla gönderilir.
- MVC client Denetim ekranı DenetimKaydiServisi API'sini doğrudan çağırır; ApiGateway Faz 10'a kadar devrede değildir.

CAP Outbox şemaları:

- `cap_kimlik`
- `cap_envanter`
- `cap_zimmet`

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

DotNetCore.CAP:

- Event publish/subscribe yönetimi
- Outbox Pattern
- Event yayınlama tekrar denemeleri
- RabbitMQ entegrasyonu

## 5. Güvenlik Tasarımı

- Kullanıcı, rol ve şifre yönetimi ASP.NET Core Identity ile yapılacaktır.
- Servisler arası ve client-server authentication JWT ile yapılacaktır.
- Authorization rol bazlı olacaktır.
- ApiGateway üzerinde route bazlı authorization policy uygulanabilecektir.
- Mikroservisler de kendilerine gelen JWT token'ı doğrulayabilecek şekilde tasarlanmalıdır.
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
- Api Gateway, servisler arası HTTP ve CAP/RabbitMQ iletişim diyagramı
- Zimmet oluşturma sequence diagram
- Zimmet iade activity diagram
- Personel işten ayrılma activity diagram
- Kritik stok bildirim akışı
- ER diyagramı
