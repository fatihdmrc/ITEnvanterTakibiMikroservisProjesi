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

