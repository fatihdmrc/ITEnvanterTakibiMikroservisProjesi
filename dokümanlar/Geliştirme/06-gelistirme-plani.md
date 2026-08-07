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
- Zimmet ve iade fotoğraf dosya yolu kayıtları eklenir.

### Faz 6 - ApiGateway Entegrasyonu

- YARP tabanlı ApiGateway projesi eklenir.
- Client uygulamasının servis çağrıları ApiGateway üzerinden yapılacak hale getirilir.
- Route bazlı yönlendirme yapılandırılır.
- Gerekli route'larda JWT doğrulama ve rol bazlı yetkilendirme uygulanır.

### Faz 7 - CAP + RabbitMQ + Outbox

- RabbitMQ Docker Compose'a eklenir.
- DotNetCore.CAP servislerde yapılandırılır.
- PostgreSQL kullanan servislerde CAP Outbox tabloları kullanılır.
- Zimmet, iade, stok ve personel ayrılış eventleri yayınlanır.
- Eventlerde gerekli kullanıcı bağlamı taşınır.

### Faz 8 - DenetimKaydiServisi

- MongoDB Docker Compose'a eklenir.
- DenetimKaydiServisi oluşturulur.
- CAP event consumer yapısı eklenir.
- Eventler MongoDB'ye audit log olarak yazılır.
- CRUD audit log yaklaşımı tasarlanır ve uygulanır.
- Audit sorgulama endpointleri hazırlanır.

### Faz 9 - Redis Cache

- Redis Docker Compose'a eklenir.
- Kategori ve lokasyon listeleri cache'lenir.
- Kategori veya lokasyon değiştiğinde cache invalidation uygulanır.

### Faz 10 - SignalR Bildirimleri

- BildirimServisi oluşturulur.
- Sadece `KritikStokSeviyesineDusuldu` eventi dinlenir.
- SignalR NotificationHub geliştirilir.
- MVC client üzerinde kritik stok bildirim paneli hazırlanır.

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
- Faz 3 kapsamında EnvanterServisi kategori, lokasyon, cihaz, sarf malzeme, stok hareketi ve kritik stok altyapısıyla çalışır durumdadır.
- Faz 3 cihaz durum modeli güncel enum adlarıyla hizalanmıştır: `Kullanilabilir`, `Zimmetli`, `Incelemede`, `Bakimda`, `HasarliTeslimAlindi`, `Kayip`, `Calindi`, `HurdaIskarta`, `KullanimDisi`.
- Eski veritabanı kayıtlarında kalan cihaz durum değerlerini yeni enum değerlerine dönüştüren migration eklenmiştir.
- Faz 4 kapsamında MVC client üzerinde envanter listeleme, ekleme, güncelleme, stok hareketi işleme, stok özeti ve kritik stok gösterimi çalışır durumdadır.
- Client tarafında servis kapalı, yetkisiz, rol yetersiz veya beklenmeyen cevap durumları Türkçe hata mesajlarıyla gösterilir.

### Uygulama kararı

- Kayıt silme endpointleri bu aşamada eklenmemiştir. Departman, personel, kategori, lokasyon, cihaz ve sarf malzemelerde silme yerine `AktifMi` alanı üzerinden pasifleştirme yaklaşımı kullanılacaktır.
- Faz 5 ve sonrası şu an geliştirme kapsamı dışında bırakılmıştır. ZimmetServisi, ApiGateway, event bus, audit log, cache ve bildirim fazları daha sonra ele alınacaktır.

## 6. Güncel Faz 4 Client Kararı - 2026-08-03

Envanter client tarafında cihaz ve sarf malzeme yönetimi, kayıt sayısı arttığında kullanılabilirliği korumak için listeleme ve işlem ekranı olarak ayrılmıştır.

- Cihazlar ana envanter ekranında tablo halinde listelenir.
- Cihaz düzenleme ve cihaz stok hareketi işleme işlemleri `CihazIslemleri` sayfasında yapılır.
- Sarf malzemeler ana envanter ekranında tablo halinde listelenir.
- Sarf malzeme düzenleme ve sarf malzeme stok hareketi işleme işlemleri `SarfMalzemeIslemleri` sayfasında yapılır.
- Kategori ve lokasyon yönetimi şimdilik tek sayfa üzerindeki satır içi yönetim yapısını korur.
- Yeni cihaz oluşturulurken `AssetTag` sistem tarafından `BT-000001` formatında otomatik üretilir.
- Cihazlar sekmesinde aktiflik, kategori ve lokasyon filtreleri bulunur.
- Cihaz işlem sayfasında ilgili cihazın stok hareketi geçmişi görüntülenir.
- Cihaz stok çıkışı gerçekten envanter dışına çıkarma anlamı taşıyorsa cihaz otomatik pasif ve toplam varlık dışı yapılır.

Bu karar Faz 4 sınırı içindedir. Faz 5 ve sonrası hâlâ geliştirme kapsamı dışında tutulmaktadır.
