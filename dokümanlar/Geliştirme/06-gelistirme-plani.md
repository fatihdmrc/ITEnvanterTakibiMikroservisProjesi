# Geliştirme Planı

Bu doküman, IT Ekipman Takip Sistemi'nin kodlama aşamasında hangi sırayla geliştirileceğini belirtir. Plan, temelden başlayıp sistemin ana özelliklerini aşamalı şekilde ekleme yaklaşımına göre hazırlanmıştır.

## 1. Temel Kararlar

- Proje kodlanacaktır.
- Servisler aynı solution içinde, ayrı proje klasörleri olarak tutulacaktır.
- İlk aşamada basit bir client uygulaması olacaktır.
- Client uygulaması ASP.NET Core MVC ve saf Bootstrap ile geliştirilecektir.
- İlk aşamada ApiGateway entegre edilmeyecektir.
- ApiGateway daha sonraki fazda YARP ile eklenecektir.
- Veritabanları Docker Compose ile ayağa kaldırılacaktır.
- Test yazımı şimdilik kapsam dışıdır.
- Kod içinde gerekli yerlerde Türkçe yorum satırları kullanılacaktır.
- Yapılan teknik işler ve nedenleri `07-gelistirme-gunlugu.md` dosyasında takip edilecektir.

## 2. İlk Çalışan Hedef

İlk çalışan hedef:

```text
KimlikVePersonelServisi + EnvanterServisi + PostgreSQL + Basit ASP.NET MVC Client
```

Bu hedefin seçilme nedeni:

- Kimlik ve personel yapısı sistemin güvenlik temelini oluşturur.
- Envanter yapısı projenin ana iş alanını erken görünür hale getirir.
- Zimmet, event bus, audit log, cache ve bildirim gibi daha karmaşık konular sağlam bir temel üzerine eklenir.
- Basit MVC client sayesinde sistem Swagger dışında da gözlemlenebilir hale gelir.

## 3. Fazlar

### Faz 0 - Çözüm ve Ortam İskeleti

- Solution dosyası oluşturulur.
- Servis, client ve ortak kod klasörleri oluşturulur.
- KimlikVePersonelServisi API projesi oluşturulur.
- EnvanterServisi API projesi oluşturulur.
- ASP.NET Core MVC client projesi oluşturulur.
- Docker Compose altyapısı için hazırlık yapılır.

### Faz 1 - Kimlik ve Personel Temeli

- Kullanıcı, rol, personel ve departman entityleri oluşturulur.
- PostgreSQL bağlantısı yapılandırılır.
- EF Core DbContext oluşturulur.
- Migration yapısı hazırlanır.
- Login endpointi geliştirilir.
- JWT token üretimi eklenir.
- Şifre kuralları uygulanır.
- Personel ve departman CRUD endpointleri geliştirilir.
- Personel işten ayrıldı durumu eklenir.

### Faz 2 - Basit MVC Client

- Login ekranı hazırlanır.
- Token saklama için basit session/cookie yaklaşımı uygulanır.
- Personel listeleme, ekleme ve güncelleme ekranları hazırlanır.
- Departman listeleme, ekleme ve güncelleme ekranları hazırlanır.

### Faz 3 - Envanter Temeli

- Kategori ve alt kategori entityleri oluşturulur.
- Lokasyon entitysi oluşturulur.
- Cihaz ve sarf malzeme entityleri oluşturulur.
- `SeriNumarasi` veya `AssetTag` alanlarından en az birinin dolu olması kuralı uygulanır.
- Cihaz durum modeli eklenir.
- Kullanılabilir stok hesaplama endpointi hazırlanır.
- Kategori, lokasyon, cihaz ve sarf malzeme CRUD endpointleri geliştirilir.

### Faz 4 - MVC Client Envanter Ekranları

- Kategori ve lokasyon ekranları hazırlanır.
- Cihaz listeleme, ekleme ve güncelleme ekranları hazırlanır.
- Sarf malzeme listeleme, ekleme ve güncelleme ekranları hazırlanır.
- Basit stok raporu ekranı hazırlanır.

### Faz 5 - ZimmetServisi

- Zimmet entityleri oluşturulur.
- Zimmet oluşturma endpointi geliştirilir.
- Personel aktiflik kontrolü KimlikVePersonelServisi üzerinden yapılır.
- Cihaz uygunluk kontrolü EnvanterServisi üzerinden yapılır.
- Zimmet oluşturulduğunda cihaz durumu `Zimmetli` yapılır.
- Zimmet iade süreci geliştirilir.
- İade kontrolünde cihaz durumu `Incelemede`, `Kullanilabilir`, `Bakimda` veya `HurdaIskarta` olarak güncellenir.
- Zimmet ve iade fotoğraf dosya yolu kayıtları bu fazdan çıkarılmıştır.

### Faz 6 - CAP + RabbitMQ + Outbox

- RabbitMQ Docker Compose'a eklenmiştir.
- DotNetCore.CAP KimlikVePersonelServisi, EnvanterServisi ve ZimmetServisi üzerinde yapılandırılmıştır.
- PostgreSQL kullanan event üretici servislerde CAP Outbox tabloları kullanılır.
- CAP outbox şemaları servis bazında ayrılmıştır: `cap_kimlik`, `cap_envanter`, `cap_zimmet`.
- Zimmet, iade, cihaz durum değişikliği, kritik stok ve personel ayrılış eventleri yayınlanır.
- Event üretimi iş verisiyle aynı EF transaction kapsamına alınmıştır.
- Event consumer tarafı Faz 7 DenetimKaydiServisi ve Faz 9 SignalR Bildirimleri kapsamında ele alınacaktır.

### Faz 7 - DenetimKaydiServisi

- Tamamlandı.
- MongoDB Docker Compose'a eklendi.
- DenetimKaydiServisi `http://localhost:5003` adresinde ayrı API olarak oluşturuldu.
- CAP event consumer yapısı eklendi.
- Eventler MongoDB `DenetimKayitlari` koleksiyonuna audit log olarak yazılır.
- CRUD audit log yaklaşımı best-effort HTTP kayıt endpointiyle uygulanır.
- Audit sorgulama endpointleri ve MVC Denetim ekranı hazırlandı.

### Faz 8 - Redis Cache

- Tamamlandı.
- Redis Docker Compose'a eklendi.
- EnvanterServisi kategori ve lokasyon listeleri Redis ile cache'lenir.
- Kategori veya lokasyon değiştiğinde ilgili cache invalidation uygulanır.
- Redis geçici olarak kapalıysa okuma akışı PostgreSQL üzerinden devam eder.

### Faz 9 - SignalR Bildirimleri

- BildirimServisi oluşturulur.
- Sadece `KritikStokSeviyesineDusuldu` eventi dinlenir.
- SignalR NotificationHub geliştirilir.
- MVC client üzerinde kritik stok bildirim paneli hazırlanır.

### Faz 10 - ApiGateway Entegrasyonu

- YARP tabanlı ApiGateway projesi eklenir.
- Client uygulamasının servis çağrıları ApiGateway üzerinden yapılacak hale getirilir.
- Route bazlı yönlendirme yapılandırılır.
- Gerekli route'larda JWT doğrulama ve rol bazlı yetkilendirme uygulanır.
- ApiGateway, servis yüzeyi ve bildirim/audit akışları netleştikten sonra Demo ve Dokümantasyon fazından hemen önce eklenecektir.

### Faz 11 - Demo ve Dokümantasyon

- Demo veri senaryoları hazırlanır.
- Çalıştırma adımları yazılır.
- Servis portları ve örnek kullanıcılar dokümante edilir.
- Staj defterine uygun teknik açıklamalar toparlanır.

## 4. Öncelikli Geliştirme Sırası

Başlangıçta uygulanacak sıra:

1. Solution ve proje iskeleti
2. Docker Compose ile PostgreSQL
3. KimlikVePersonelServisi temel yapısı
4. EnvanterServisi temel yapısı
5. Basit MVC client
6. JWT login
7. Personel, departman, kategori, lokasyon ve cihaz temel ekranları

## 5. Güncel Faz Durumu - 2026-08-02

Bu bölüm, kod tarafında yapılan son değişikliklerden sonra planın güncel durumunu göstermek için eklenmiştir.

### Tamamlanan veya Faz 4 sınırında bırakılan işler

- Faz 1 kapsamında KimlikVePersonelServisi, ASP.NET Core Identity, JWT, PostgreSQL, EF Core migration, rol bazlı yetkilendirme, departman/personel/kullanıcı temel endpointleri ve işten ayrılma akışıyla çalışır durumdadır.
- Faz 2 kapsamında MVC client login, session içinde token saklama, departman listeleme/oluşturma/güncelleme, personel listeleme/oluşturma/güncelleme ve kullanıcı listeleme/oluşturma işlemlerini destekler.
- Faz 2 personel yönetimi tek satır düzenleme yerine ayrı sayfa akışına taşınmıştır. Personeller artık tabloda listelenir, arama ve departman filtresiyle süzülebilir, düzenleme ayrı sayfada yapılır.
- Personeli işten ayrıldı yapma işlemi artık doğrudan listeden çalışmaz; ayrı bir onay sayfası üzerinden yapılır.
- Faz 3 kapsamında EnvanterServisi kategori, lokasyon, cihaz, sarf malzeme, cihaz durum hareketi, sarf malzeme stok hareketi ve kritik stok altyapısıyla çalışır durumdadır.
- Faz 3 cihaz durum modeli güncel enum adlarıyla hizalanmıştır: `Kullanilabilir`, `Zimmetli`, `Incelemede`, `Bakimda`, `HasarliTeslimAlindi`, `Kayip`, `Calindi`, `HurdaIskarta`, `KullanimDisi`.
- Eski veritabanı kayıtlarında kalan cihaz durum değerlerini yeni enum değerlerine dönüştüren migration eklenmiştir.
- Faz 4 kapsamında MVC client üzerinde envanter listeleme, ekleme, güncelleme, cihaz durum hareketi, sarf malzeme stok hareketi, stok özeti ve kritik stok gösterimi çalışır durumdadır.
- Client tarafında servis kapalı, yetkisiz, rol yetersiz veya beklenmeyen cevap durumları Türkçe hata mesajlarıyla gösterilir.

### Uygulama kararı

- Kayıt silme endpointleri bu aşamada eklenmemiştir. Departman, personel, kategori, lokasyon, cihaz ve sarf malzemelerde silme yerine `AktifMi` alanı üzerinden pasifleştirme yaklaşımı kullanılacaktır.
- Faz 5 ZimmetServisi geliştirmesi tamamlanmıştır. Faz 6 CAP/RabbitMQ + Outbox entegrasyonu uygulanmıştır. Faz 7 DenetimKaydiServisi tamamlanmıştır. Faz 8 Redis Cache tamamlanmıştır. Faz 9 ve sonrası sırasıyla bildirim, ApiGateway ve Demo/Dokümantasyon olarak ele alınacaktır.

## 6. Güncel Faz 4 Client Kararı - 2026-08-03

Envanter client tarafında cihaz ve sarf malzeme yönetimi, kayıt sayısı arttığında kullanılabilirliği korumak için listeleme ve işlem ekranı olarak ayrılmıştır.

- Cihazlar ana envanter ekranında tablo halinde listelenir.
- Cihaz düzenleme ve cihaz durum hareketi işleme işlemleri `CihazIslemleri` sayfasında yapılır.
- Sarf malzemeler ana envanter ekranında tablo halinde listelenir.
- Sarf malzeme düzenleme ve sarf malzeme stok hareketi işleme işlemleri `SarfMalzemeIslemleri` sayfasında yapılır.
- Sarf malzeme işlem sayfasında ilgili sarf malzemenin stok hareketi geçmişi görüntülenir.
- Kategori ve lokasyon yönetimi şimdilik tek sayfa üzerindeki satır içi yönetim yapısını korur.
- Yeni cihaz oluşturulurken `AssetTag` sistem tarafından `BT-000001` formatında otomatik üretilir.
- Cihazlar sekmesinde aktiflik, kategori ve lokasyon filtreleri bulunur.
- Cihaz işlem sayfasında ilgili cihazın durum geçmişi görüntülenir.
- Cihaz durum hareketi gerçekten envanter dışına çıkarma anlamı taşıyorsa cihaz otomatik pasif ve toplam varlık dışı yapılır.

Bu karar Faz 4 sınırı içinde tamamlanmıştır. Faz 5 ZimmetServisi ayrı servis olarak ele alınacaktır.

## 7. Cihaz Durum Hareketi ve Kapsam Kararı - 2026-08-08

Faz 3 ve Faz 4 kapsamında cihaz yaşam döngüsü yönetimi netleştirilmiştir.

- Cihazlarda `AktifMi` artık manuel yönetim alanı değildir; cihazın durumuna ve elden çıkarma tipine göre sistem tarafından hesaplanır.
- `ToplamVarligaDahilMi` kullanıcı tercihi değil, raporlama ve sayım kapsamı sonucudur.
- Cihaz güncelleme ekranında `AktifMi` ve `ToplamVarligaDahilMi` checkbox'ları bulunmaz; bu alanlar salt okunur bilgi olarak gösterilir.
- Cihazlarda “stok hareketi” kavramı yerine “cihaz durum hareketi” kavramı kullanılacaktır.
- Eski cihaz stok hareketi endpointi kaldırılmıştır; cihaz durum hareketi için tek yazma endpointi `POST /api/cihazlar/{id}/durum-hareketleri` olarak kullanılır.
- Sarf malzemelerde “stok hareketi” kavramı aynı kalır; çünkü sarf malzemeler adet bazlı giriş, çıkış ve düzeltme hareketleriyle yönetilir.
- Mevcut cihaz kayıtlarını yeni kurala hizalamak için `CihazKapsamAlanlariniDurumaGoreDuzelt` migration'ı eklenmiştir.
- Cihaz bilgi güncelleme ekranından cihaz durumu ve elden çıkarma alanları kaldırılmıştır; bu alanlar salt okunur gösterilir.
- Cihazın bütün durum değişiklikleri `Cihaz Durum Hareketi` formu üzerinden yapılır ve geçmişe kaydedilir.
- Bakımdan dönen cihaz için `BakimdanDondu` hareketi kullanılır; bu hareket cihazı tekrar `Kullanilabilir` durumuna alır.
- Zimmet akışları için `Zimmetlendi` hareketi cihazı `Zimmetli`, `ZimmetIadeAlindi` hareketi cihazı `Incelemede` durumuna alır.
- Bu iki hareket Faz 5 ZimmetServisi için hazırlık niteliğindedir; cihaz durumunun nihai sahibi EnvanterServisi olarak kalır.
- `EnvantereGiris` cihaz durum hareketi seçeneği değildir; yeni cihaz oluşturma akışının parçasıdır.

## 8. Faz 5 ZimmetServisi Durumu - 2026-08-08

Faz 5 kapsamında `ZimmetServisi.Api` ayrı API projesi olarak eklenmiştir.

- Zimmet verileri PostgreSQL içinde `zimmet` şemasında tutulur.
- `zimmet.Zimmetler` tablosu için `IlkZimmetSemasi` migration'ı oluşturulmuştur.
- Bir cihaz için aynı anda yalnızca bir açık zimmet bulunabilir. Açık zimmet durumları `Aktif` ve `IadeSurecinde` olarak kabul edilir.
- Zimmet oluşturma sırasında personel uygunluğu KimlikVePersonelServisi üzerinden, cihaz uygunluğu EnvanterServisi üzerinden HTTP ile doğrulanır.
- Zimmet oluşturulunca ZimmetServisi, EnvanterServisi cihaz durum hareketi endpointine `Zimmetlendi` nedeni gönderir ve cihaz `Zimmetli` olur.
- Zimmet iade alınınca `ZimmetIadeAlindi` nedeni kullanılır ve cihaz fiziki kontrol için `Incelemede` olur.
- İade kontrolü `Saglam`, `Bakimda`, `HurdaIskarta` veya `HasarliTeslimAlindi` sonuçlarından biriyle tamamlanır.
- MVC client üzerinde `Zimmetler` bölümü eklenmiştir. Admin/IT tüm zimmetleri yönetebilir, personel kullanıcısı kendi zimmetlerini görebilir.
- Zimmet ve iade fotoğrafları bu fazda uygulanmamıştır; fotoğraf tablosu, endpointi ve UI alanı yoktur.
- CAP/RabbitMQ ve Outbox Faz 6'da eklenmiştir; Faz 5'in senkron HTTP doğrulama ve cihaz durumu güncelleme akışı korunmuştur.

## 9. Faz 6 CAP/RabbitMQ + Outbox Durumu - 2026-08-10

Faz 6 kapsamında event üreten servislerde DotNetCore.CAP ve RabbitMQ entegrasyonu uygulanmıştır.

- Docker Compose'a RabbitMQ management container'ı eklenmiştir.
- Üç API projesine `DotNetCore.CAP.PostgreSql` ve `DotNetCore.CAP.RabbitMQ` paketleri eklenmiştir.
- KimlikVePersonelServisi, EnvanterServisi ve ZimmetServisi kendi PostgreSQL bağlantıları üzerinden CAP outbox kullanır.
- CAP şemaları servis bazında ayrıdır: `cap_kimlik`, `cap_envanter`, `cap_zimmet`.
- `inventory.events` RabbitMQ exchange'i ortak event exchange'i olarak kullanılır.
- KimlikVePersonelServisi `personel.isten-ayrildi` eventini üretir.
- EnvanterServisi `cihaz.durumu-degisti` ve `stok.kritik-seviyeye-dusuldu` eventlerini üretir.
- ZimmetServisi `zimmet.olusturuldu`, `zimmet.iade-alindi`, `cihaz.kontrole-alindi`, `zimmet.iade-edildi` ve hasarlı iade durumunda `cihaz.hasarli-teslim-alindi` eventlerini üretir.
- Event tüketicilerinden DenetimKaydiServisi Faz 7'de uygulanmıştır; bildirim tüketimi Faz 9'da uygulanacaktır.

## 15. Faz 7 DenetimKaydiServisi Durumu - 2026-08-11

Faz 7 ile DenetimKaydiServisi ayrı API olarak eklenmiştir.

- Servis portu `5003` olarak sabitlenmiştir.
- MongoDB `docker-compose.yml` içine `mongodb` servisi olarak eklenmiştir.
- CAP/RabbitMQ event consumer yapısı `denetim-kaydi-servisi` consumer grubu ile çalışır.
- `personel.isten-ayrildi`, `cihaz.durumu-degisti`, `stok.kritik-seviyeye-dusuldu`, `zimmet.olusturuldu`, `zimmet.iade-alindi`, `zimmet.iade-edildi`, `cihaz.kontrole-alindi` ve `cihaz.hasarli-teslim-alindi` eventleri MongoDB'ye yazılır.
- KimlikVePersonelServisi, EnvanterServisi ve ZimmetServisi başarılı mutasyonları DenetimKaydiServisi `POST /api/denetim-kayitlari/crud` endpointine best-effort olarak gönderir.
- MVC client içinde Denetim ekranı eklenmiştir; Admin/IT kullanıcıları kayıtları filtreleyebilir ve payload detayını görebilir.

## 16. Faz 8 Redis Cache Durumu - 2026-08-12

Faz 8 ile Redis cache altyapısı eklenmiştir.

- Redis `docker-compose.yml` içine `redis` servisi olarak eklenmiştir.
- EnvanterServisi `Microsoft.Extensions.Caching.StackExchangeRedis` paketiyle Redis'e bağlanır.
- `KategorileriListeleAsync` ve `LokasyonlariListeleAsync` metotları cache-aside yaklaşımıyla çalışır.
- Cache anahtarları `envanter:kategoriler:v1` ve `envanter:lokasyonlar:v1` olarak belirlenmiştir.
- Kategori veya lokasyon oluşturma/güncelleme başarılı olunca ilgili cache temizlenir.
- Redis okunamaz, yazılamaz veya temizlenemezse ana API akışı PostgreSQL üzerinden devam eder ve uyarı logu yazılır.
