# Geliştirme Günlüğü

Bu doküman, kodlama aşamasında yapılan işleri ve bu işlerin neden yapıldığını takip etmek için tutulur.

## 2026-07-27

### Geliştirme planı oluşturuldu

Ne yapıldı:

- Geliştirme aşamaları fazlara ayrıldı.
- İlk çalışan hedef belirlendi.
- ApiGateway'in ilk aşamada değil, daha sonraki fazda entegre edilmesine karar verildi.
- Basit MVC client uygulamasının başlangıçtan itibaren bulunmasına karar verildi.

Neden yapıldı:

- Projeyi aynı anda çok fazla teknolojiyle karmaşıklaştırmadan başlatmak istiyoruz.
- Önce kimlik/personel ve envanter temelini kurup, sonrasında zimmet, event bus, audit log, cache ve bildirim gibi özellikleri eklemek daha kontrollü bir geliştirme sağlar.

### Solution ve ilk proje iskeleti oluşturuldu

Ne yapıldı:

- `ITEnvanterTakipSistemi.sln` solution dosyası oluşturuldu.
- `KimlikVePersonelServisi.Api` Web API projesi oluşturuldu.
- `EnvanterServisi.Api` Web API projesi oluşturuldu.
- `EnvanterTakip.MvcClient` ASP.NET Core MVC projesi oluşturuldu.
- Projeler solution dosyasına eklendi.
- API projelerindeki örnek `weatherforecast` endpointleri kaldırıldı.
- Her API projesine `/saglik` endpointi eklendi.
- NuGet erişimi onaylanmadığı için Swagger paketleri başlangıç iskeletinden geçici olarak çıkarıldı.
- MVC client ana sayfası ilk fazı anlatan basit bir kontrol paneline dönüştürüldü.
- Geliştirme portları sabitlendi:
  - KimlikVePersonelServisi: `http://localhost:5000`
  - EnvanterServisi: `http://localhost:5001`
  - MVC Client: `http://localhost:5010`
- İlk Docker Compose dosyasına PostgreSQL servisi eklendi.

Neden yapıldı:

- Solution yapısını erken kurmak servislerin aynı çatı altında yönetilmesini kolaylaştırır.
- İlk aşamada iki temel domain olan kimlik/personel ve envanter ayrı servisler olarak başlatıldı.
- `/saglik` endpointleri, her servisin çalışıp çalışmadığını hızlıca doğrulamak için eklendi.
- Dış paket indirmeden derlenebilen bir başlangıç iskeleti elde etmek için Swagger entegrasyonu sonraki adıma bırakıldı.
- PostgreSQL Docker Compose ile tanımlanarak geliştirme ortamının kişisel bilgisayara bağımlılığı azaltıldı.

### İlk derleme doğrulandı

Ne yapıldı:

- `dotnet restore` komutu yerel `.dotnet` ve `.nuget` klasörleri kullanılarak çalıştırıldı.
- `dotnet build ITEnvanterTakipSistemi.sln --no-restore` komutu çalıştırıldı.
- Build sonucu 0 hata ve 0 uyarı olarak doğrulandı.
- `.gitignore` dosyası eklendi.
- Projeyi manuel çalıştırmayı anlatan `08-calistirma-rehberi.md` dosyası eklendi.

Neden yapıldı:

- İlk iskeletin daha fazla geliştirmeye başlamadan önce derlenebilir olduğundan emin olmak istiyoruz.
- `.dotnet`, `.nuget`, `bin` ve `obj` gibi yerel/üretilmiş klasörlerin proje takibini kirletmemesi gerekir.
- Çalıştırma adımlarını dokümana yazmak, geliştirme sürecinde aynı komutları tekrar tekrar hatırlamayı kolaylaştırır.

### Swagger paketleri geri eklendi

Ne yapıldı:

- `KimlikVePersonelServisi.Api` projesine Swagger paketleri geri eklendi.
- `EnvanterServisi.Api` projesine Swagger paketleri geri eklendi.
- API projelerinde Swagger middleware'i tekrar aktif edildi.
- `/saglik` endpointleri Swagger üzerinde görünecek şekilde `WithOpenApi()` ile işaretlendi.
- API launch ayarlarında başlangıç adresi tekrar `swagger` olarak düzenlendi.

Neden yapıldı:

- Swagger, geliştirme sırasında endpointleri tarayıcı üzerinden görmek ve denemek için önemli bir kolaylık sağlar.
- İlk denemede NuGet erişimi izin/SSL hatasına takıldığı için Swagger geçici olarak çıkarılmıştı; paketler indirildiğinde tekrar kullanılacaktır.

Son durum:

- Paket referansları proje dosyalarına eklendi.
- Restore denemesinde NuGet erişimi olmadığı için paketler indirilemedi.
- Hata alınan paketler: `Microsoft.AspNetCore.OpenApi`, `Swashbuckle.AspNetCore`.
- Paketler indirilemediği sürece API projeleri build aşamasında restore bekleyecektir.

### Dosya kilidi build hatası giderildi

Ne yapıldı:

- Build çıktısında üç uygulamanın `.exe` dosyalarını kilitlediği görüldü.
- Kilit oluşturan çalışan süreçler kapatıldı:
  - `EnvanterServisi.Api`
  - `EnvanterTakip.MvcClient`
  - `KimlikVePersonelServisi.Api`
- `dotnet restore ITEnvanterTakipSistemi.sln` tekrar çalıştırıldı.
- `dotnet build ITEnvanterTakipSistemi.sln --no-restore` tekrar çalıştırıldı.
- Build sonucu 0 hata ve 0 uyarı olarak doğrulandı.
- Çalıştırma rehberine dosya kilidi hatası için çözüm notu eklendi.

Neden yapıldı:

- Windows üzerinde çalışan `.exe` dosyası build sırasında üzerine yazılamaz.
- Servisleri kapatmadan build almak dosya kilidi hatasına neden olur.
- Bu hata kod hatası değil, çalışan uygulama süreciyle ilgili geliştirme ortamı hatasıdır.

### KimlikVePersonelServisi temel endpointleri eklendi

Ne yapıldı:

- `Departman`, `Personel` ve `Kullanici` domain entityleri oluşturuldu.
- `KullaniciRolu` ve `PersonelDurumu` enumları oluşturuldu.
- Departman, personel, kullanıcı ve giriş işlemleri için DTO/contract sınıfları eklendi.
- Şifre kurallarını kontrol eden `SifreServisi` eklendi.
- Şifreler düz metin saklanmasın diye PBKDF2-SHA256 tabanlı hash üretimi eklendi.
- JWT paketleri henüz eklenmediği için geçici `GelistirmeTokenServisi` oluşturuldu.
- İlk aşamada veri kaybetmeyi kabul eden in-memory `KimlikPersonelDeposu` eklendi.
- Şu endpointler eklendi:
  - `POST /api/kimlik/giris`
  - `GET /api/departmanlar`
  - `GET /api/departmanlar/{id}`
  - `POST /api/departmanlar`
  - `PUT /api/departmanlar/{id}`
  - `GET /api/personeller`
  - `GET /api/personeller/{id}`
  - `POST /api/personeller`
  - `PUT /api/personeller/{id}`
  - `POST /api/personeller/{id}/isten-ayrildi`
  - `GET /api/kullanicilar`
  - `POST /api/kullanicilar`
- Solution build edildi ve 0 hata, 0 uyarı ile doğrulandı.

Neden yapıldı:

- EF Core/PostgreSQL paketleri için NuGet izni alınamadığı için geliştirmeyi durdurmadan domain ve endpoint davranışını önce bellekte kurduk.
- PersonelId zorunluluğu, işten ayrılan personelin hesabının pasifleştirilmesi ve şifre kuralları gibi temel iş kararları erken aşamada koda geçirildi.
- Geçici token servisi, gerçek JWT entegrasyonu gelene kadar login akışını deneyebilmek için eklendi.
- In-memory yapı kalıcı çözüm değildir; EF Core eklendiğinde repository/DbContext yapısına taşınacaktır.

### KimlikVePersonelServisi PostgreSQL'e bağlandı

Ne yapıldı:

- `KimlikPersonelDbContext` oluşturuldu.
- `Departmanlar`, `Personeller` ve `Kullanicilar` tabloları EF Core modeline eklendi.
- Varsayılan schema olarak `kimlik_personel` belirlendi.
- Enum alanları veritabanında okunabilir olması için string olarak saklanacak şekilde yapılandırıldı.
- `Departman.Ad`, `Personel.Email`, `Kullanici.KullaniciAdi` ve `Kullanici.PersonelId` için unique index tanımlandı.
- In-memory depo yerine `KimlikPersonelVeritabaniDeposu` eklendi.
- `Program.cs` içinde `KimlikPersonelDbContext` PostgreSQL bağlantısıyla kaydedildi.
- `appsettings.json` ve `appsettings.Development.json` dosyalarına bağlantı cümlesi eklendi.
- İlk migration oluşturuldu: `IlkKimlikPersonelSemasi`.
- Docker Compose ile PostgreSQL container başlatıldı.
- Migration veritabanına uygulandı.
- Solution build edildi ve 0 hata, 0 uyarı ile doğrulandı.

Neden yapıldı:

- Kimlik ve personel verilerinin servis kapanınca kaybolmaması için kalıcı veritabanı bağlantısına geçildi.
- Tek PostgreSQL container içinde mikroservis sınırını korumak için `kimlik_personel` schema'sı kullanıldı.
- Kullanıcı hesabının bir personel kaydına bağlı olması ve bir personele yalnızca bir kullanıcı hesabı açılması veri modeli seviyesinde de garanti altına alındı.
- Migration kullanımı, veritabanı şemasını kodla birlikte takip etmeyi sağlar.

### Gerçek JWT üretimi ve demo kullanıcılar eklendi

Ne yapıldı:

- `JwtTokenServisi` eklendi ve geçici geliştirme token mantığı yerine gerçek JWT üretimi kullanılmaya başlandı.
- JWT doğrulama ayarları `Program.cs` içinde aktif edildi.
- Swagger üzerinde Bearer token ile yetkili endpoint denemesi yapılabilecek güvenlik tanımı eklendi.
- Geliştirme ortamında hızlı deneme yapılabilmesi için demo departman, personel ve kullanıcı kayıtları seed edildi.
- Demo kullanıcılar:
  - `admin` / `Admin123!`
  - `it.personel` / `ItPersonel123!`
  - `personel` / `Personel123!`
- Windows geliştirme ortamında EventLog ve DataProtection yetki sorunlarına takılmamak için loglama konsola yönlendirildi, DataProtection anahtarları proje içindeki yerel klasöre alındı ve bu klasör git dışında bırakıldı.
- `dotnet build ITEnvanterTakipSistemi.sln --no-restore` komutu 0 hata ve 0 uyarı ile doğrulandı.
- `admin` kullanıcısıyla giriş yapılıp alınan JWT token ile korumalı endpoint çağrısı başarıyla denendi.

Neden yapıldı:

- Kimlik doğrulama akışının yalnızca sahte token ile değil, gerçek imzalı JWT ile çalışması gerekir.
- Demo kullanıcılar, her seferinde elle departman/personel/kullanıcı oluşturmadan Swagger üzerinden hızlı test yapmayı sağlar.
- Login cevabındaki `KullaniciId`, `PersonelId`, `Rol` ve `token` bilgileri client tarafında oturum bilgisini oluşturmak için yeterlidir.
- DataProtection anahtarları makineye ve geliştirme ortamına özel olduğu için kaynak kod takibine alınmamalıdır.

### DBeaver Community veritabanı görüntüleme aracı kuruldu

Ne yapıldı:

- PostgreSQL veritabanı yapısını masaüstünden görüntüleyebilmek için DBeaver Community Desktop kuruldu.
- Kurulum `winget` üzerinden `DBeaver.DBeaver.Community` paketiyle yapıldı.
- Çalıştırma rehberine PostgreSQL bağlantı bilgileri ve ilk bağlantı adımları eklendi.

Neden yapıldı:

- EF Core migration sonucunda oluşan schema, tablo, kolon ve kayıtları görsel olarak incelemek geliştirme ve öğrenme sürecini kolaylaştırır.
- DBeaver, PostgreSQL dışında ileride MongoDB gibi farklı veritabanlarını da görüntüleyebildiği için bu proje yapısına uygundur.

### Repository pattern ve MVC client entegrasyonu eklendi

Ne yapıldı:

- Kimlik ve personel servisindeki veri erişimi repository sınıflarına ayrıldı.
- `IDepartmanRepository`, `IPersonelRepository` ve `IKullaniciRepository` arayüzleri eklendi.
- EF Core kullanan repository implementasyonları eklendi.
- İş kuralları `KimlikPersonelServisi` içinde toplandı.
- Minimal API endpointleri artık doğrudan repository yerine `IKimlikPersonelServisi` üzerinden çalışıyor.
- MVC client tarafına `KimlikPersonelApiClient` eklendi.
- MVC ana sayfasına login, oturum bilgisi, departman listesi, personel listesi, kullanıcı listesi ve temel oluşturma formları eklendi.
- MVC client içinde session kullanılmaya başlandığı için DataProtection anahtarları proje içindeki yerel klasöre alındı.
- Kod yapısını açıklayan `09-kod-yapisi-aciklamalari.md` dokümanı oluşturuldu.

Neden yapıldı:

- Repository pattern, EF Core sorgularını servis iş kurallarından ayırır.
- MVC client, Swagger kullanmadan da temel kimlik/personel akışlarını test etmeyi sağlar.
- Session içinde JWT token tutulduğu için DataProtection ayarı geliştirme ortamında daha kararlı çalışmayı sağlar.
- Kod yapısı açıklamalarının dokümana alınması, staj defteri ve proje sunumu için daha anlaşılır bir takip sağlar.

### Endpoint authorization kuralları aktif edildi

Ne yapıldı:

- Departman endpointleri `AdminVeyaITPersoneli` policy ile korundu.
- Personel endpointleri `AdminVeyaITPersoneli` policy ile korundu.
- Kullanıcı endpointleri `SadeceAdmin` policy ile korundu.
- MVC client, session içinde tuttuğu JWT token'ı listeleme ve işlem isteklerinde API'ye gönderecek şekilde güncellendi.
- Yetki davranışı doğrulandı:
  - Tokensız departman listeleme: `401 Unauthorized`
  - Admin token ile departman listeleme: `200 OK`
  - PersonelKullanicisi token ile kullanıcı listeleme: `403 Forbidden`

Neden yapıldı:

- Authentication yalnızca token'ın doğrulanmasını sağlar; endpoint üzerinde authorization zorunlu değilse işlem yine açık kalır.
- CRUD endpointlerinin gerçekten korumalı olması için endpoint gruplarına `RequireAuthorization` eklenmelidir.

### Gereksiz kimlik endpointi ve default MVC sayfası kaldırıldı

Ne yapıldı:

- `GET /api/kimlik/ben` endpointi kaldırıldı.
- MVC client'ın oturum bilgisini almak için `/api/kimlik/ben` çağırması kaldırıldı.
- Login cevabındaki kullanıcı bağlamı MVC session içinde saklanmaya başlandı.
- Kullanılmayan `Privacy` action ve view dosyası kaldırıldı.
- `KimlikPersonelPanelModel` içindeki kullanılmayan form property'leri temizlendi.

Neden yapıldı:

- Login cevabı zaten token, kullanıcı id, personel id ve rol bilgisini döndürdüğü için ayrıca `/api/kimlik/ben` endpointine ihtiyaç kalmadı.
- Gereksiz endpoint sayısını azaltmak API yüzeyini sadeleştirir.
- Default MVC template dosyalarını kaldırmak projenin gerçek kapsamını daha net gösterir.

### KimlikVePersonelServisi controller mimarisine taşındı

Ne yapıldı:

- Minimal API route tanımları `Program.cs` içinden kaldırıldı.
- `Controllers` klasörü eklendi.
- `SaglikController`, `KimlikController`, `DepartmanlarController`, `PersonellerController` ve `KullanicilarController` oluşturuldu.
- Endpoint adresleri aynı bırakıldı:
  - `GET /saglik`
  - `POST /api/kimlik/giris`
  - `GET/POST/PUT /api/departmanlar`
  - `GET/POST/PUT /api/personeller`
  - `GET/POST /api/kullanicilar`
- `Program.cs` dependency injection, middleware, Swagger, JWT, DataProtection ve veritabanı başlangıç ayarlarına odaklanacak şekilde sadeleştirildi.
- Controller geçişi sonrasında build alındı ve temel endpoint davranışı doğrulandı:
  - `GET /saglik`: başarılı
  - `POST /api/kimlik/giris`: token üretiyor
  - Tokensız `GET /api/departmanlar`: `401 Unauthorized`
  - Admin token ile `GET /api/departmanlar`: `200 OK`

Neden yapıldı:

- Projenin hedef mimarisinde controller, service ve repository yapılarının uygulanması bekleniyor.
- Endpoint sayısı arttıkça controller dosyaları Minimal API route bloklarına göre daha okunabilir ve yönetilebilir olur.
- Controller yapısı staj dokümantasyonu ve sunumunda daha klasik, anlaşılır bir backend akışı gösterir.
