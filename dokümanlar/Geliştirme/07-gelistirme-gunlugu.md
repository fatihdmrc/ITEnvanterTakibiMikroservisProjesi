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

## 2026-07-28

### ASP.NET Core Identity geçişi yapıldı

Ne yapıldı:

- KimlikVePersonelServisi custom kullanıcı/şifre sistemi yerine ASP.NET Core Identity altyapısına taşındı.
- `Kullanici` entity'si kaldırıldı ve yerine `UygulamaKullanici : IdentityUser<Guid>` eklendi.
- `KimlikPersonelDbContext`, `IdentityDbContext<UygulamaKullanici, IdentityRole<Guid>, Guid>` sınıfından türetildi.
- Identity tabloları `kimlik_personel` schema'sı altında Türkçe tablo adlarıyla oluşturuldu:
  - `Kullanicilar`
  - `Roller`
  - `KullaniciRolleri`
  - `KullaniciClaimleri`
  - `KullaniciLoginleri`
  - `RolClaimleri`
  - `KullaniciTokenlari`
- `SifreServisi`, `IKullaniciRepository` ve `EfKullaniciRepository` kaldırıldı.
- Kullanıcı oluşturma, rol atama ve şifre doğrulama işlemleri `UserManager`, `RoleManager` ve `SignInManager` üzerinden yapılacak şekilde güncellendi.
- `JwtTokenServisi`, `UygulamaKullanici` ve Identity rol bilgisinden JWT üretecek hale getirildi.
- Personel işten ayrıldığında bağlı kullanıcı hesabının `AktifMi = false` yapılması korundu.
- Demo seed yapısı Identity üzerinden kullanıcı ve rol oluşturacak şekilde yenilendi.
- Eski migration dosyaları temizlendi ve Identity tabanlı yeni ilk migration oluşturuldu.

Neden yapıldı:

- ASP.NET Core Identity uzun vadede şifre hashleme, rol yönetimi, hesap kilitleme ve kullanıcı güvenliği gibi konularda custom çözüme göre daha güvenilir ve sürdürülebilir bir altyapı sağlar.
- JWT kullanılmaya devam ettiği için API endpoint davranışı ve MVC client akışı korunur; Identity yalnızca kullanıcı, rol ve şifre yönetimini üstlenir.
- Kullanıcı hesabının personel kaydına zorunlu bağlı olması proje kuralıyla uyumlu kalır.

Doğrulama:

- `dotnet build ITEnvanterTakipSistemi.sln --no-restore` komutu 0 hata ve 0 uyarı ile tamamlandı.

## 2026-07-30

### Faz 3 - Envanter temeli başlatıldı

Ne yapıldı:

- `EnvanterServisi.Api` projesine PostgreSQL ve EF Core altyapısı eklendi.
- `EnvanterDbContext` oluşturuldu ve varsayılan schema `envanter` olarak belirlendi.
- Envanter domain modeli oluşturuldu:
  - `Kategori`
  - `Lokasyon`
  - `Cihaz`
  - `SarfMalzeme`
  - `StokHareketi`
  - `KritikStokKurali`
- Cihaz durum modeli eklendi:
  - `DepodaHazir`
  - `Zimmetli`
  - `Incelemede`
  - `Bakimda`
  - `Arizali`
  - `Kayip`
  - `Calindi`
  - `HurdaIskartaDepoda`
  - `EldenCikarildi`
- `SeriNumarasi` veya `AssetTag` alanlarından en az birinin dolu olması hem servis katmanında hem de veritabanı check constraint ile güvence altına alındı.
- Generic repository ve özel repository yapısı EnvanterServisi içinde de uygulandı.
- `IEnvanterServisi` ve `EnvanterYonetimServisi` eklendi.
- Controller endpointleri eklendi:
  - `GET/POST/PUT /api/kategoriler`
  - `GET/POST/PUT /api/lokasyonlar`
  - `GET/POST/PUT /api/cihazlar`
  - `GET/POST/PUT /api/sarf-malzemeler`
  - `GET /api/stok/ozet`
  - `GET/POST/PUT /api/stok/kritik-kurallar`
- Envanter endpointleri JWT doğrulama ve `AdminVeyaITPersoneli` policy ile korundu.
- Demo kategori, lokasyon, cihaz, sarf malzeme ve kritik stok kuralı seed verileri eklendi.
- İlk migration oluşturuldu: `IlkEnvanterSemasi`.

Neden yapıldı:

- Faz 3 ile projenin ana iş alanı olan cihaz ve stok yönetimi görünür hale getirildi.
- Seri numaralı cihazlar ve sarf malzemeler ayrı takip edildiği için veri modeli iki varlık tipini farklı kurallarla ele alacak şekilde tasarlandı.
- Kullanılabilir stok, seri numaralı cihazlarda cihaz durumundan; sarf malzemelerde `EldekiMiktar` alanından hesaplanır.
- Kritik stok bildirimi ileride SignalR fazında yalnızca kritik stok altına düşme senaryosunda üretileceği için kritik stok raporu ve kural altyapısı erken eklendi.

Doğrulama:

- `dotnet restore ITEnvanterTakipSistemi.sln` başarıyla tamamlandı.
- `dotnet build ITEnvanterTakipSistemi.sln --no-restore` komutu 0 hata ve 0 uyarı ile tamamlandı.
- `IlkEnvanterSemasi` migration'ı PostgreSQL veritabanına başarıyla uygulandı.
- Tokensız `GET /api/kategoriler` isteği `401 Unauthorized` döndü.
- Admin token ile `GET /api/kategoriler` isteği demo kategorileri döndürdü.
- Admin token ile `GET /api/stok/ozet` isteği stok özetini ve kritik stok listesini döndürdü.
- `SeriNumarasi` ve `AssetTag` boş cihaz oluşturma isteği `400 Bad Request` döndü.
- PostgreSQL geliştirme volume'ü temiz sıfırlandı ve `IlkIdentityKimlikPersonelSemasi` migration'ı başarıyla uygulandı.
- API açılışında demo departman, personel, rol ve kullanıcı seed kayıtları Identity üzerinden oluşturuldu.
- `admin / Admin123!` ile login sonucu JWT token üretildi.
- Tokensız `GET /api/departmanlar` isteği `401 Unauthorized` döndü.
- Admin token ile `GET /api/departmanlar` isteği `200 OK` döndü.
- `PersonelKullanicisi` token ile `GET /api/kullanicilar` isteği `403 Forbidden` döndü.
- Geçici bir kullanıcıyla işten ayrılma senaryosu denendi; personel işten ayrıldıktan sonra bağlı kullanıcı login yapamadı.
- Test verileri temizlendi ve veritabanı tekrar yalnızca demo seed kayıtları kalacak şekilde sıfırlandı.

### Generic repository ve özel repository birlikte uygulandı

Ne yapıldı:

- Ortak CRUD işlemleri için `IGenericRepository<TEntity>` arayüzü eklendi.
- EF Core tabanlı ortak implementasyon için `EfGenericRepository<TEntity>` sınıfı eklendi.
- `IDepartmanRepository`, `IGenericRepository<Departman>` arayüzünden miras alacak şekilde düzenlendi.
- `IPersonelRepository`, `IGenericRepository<Personel>` arayüzünden miras alacak şekilde düzenlendi.
- `EfDepartmanRepository` ve `EfPersonelRepository`, `EfGenericRepository<T>` sınıfından türetildi.
- Departman ve personel repositorylerinde yalnızca domain'e özel sorgular bırakıldı.
- Dependency injection içinde açık generic repository kaydı eklendi.

Neden yapıldı:

- Basit CRUD metotlarının her repository'de tekrar yazılmasını engellemek istiyoruz.
- Departman ve personel gibi entity'lerde ortak işlemler generic repository üzerinden gelirken, `AdKullaniliyorMuAsync` veya `EmailKullaniliyorMuAsync` gibi özel sorgular ilgili repository içinde kalmalıdır.
- Faz 3'te eklenecek kategori, lokasyon, cihaz ve sarf malzeme yapıları için tekrar kullanılabilir bir veri erişim temeli oluşturuldu.

Doğrulama:

- `dotnet build ITEnvanterTakipSistemi.sln --no-restore` komutu 0 hata ve 0 uyarı ile tamamlandı.

### KimlikVePersonelServisi asenkron veri erişimine taşındı

Ne yapıldı:

- `IDepartmanRepository` ve `IPersonelRepository` metotları `Task` dönecek şekilde asenkron hale getirildi.
- EF Core repository implementasyonlarında `ToListAsync`, `FirstOrDefaultAsync`, `AnyAsync` ve `SaveChangesAsync` kullanılmaya başlandı.
- `IKimlikPersonelServisi` ve `KimlikPersonelServisi` metotları asenkron hale getirildi.
- Controller action metotları `async Task<ActionResult<...>>` dönecek şekilde güncellendi.
- ASP.NET Core Identity çağrılarındaki bloklayan `.GetAwaiter().GetResult()` kullanımları `await` ile değiştirildi.
- Demo seed akışı `SeedAsync` ve `KullaniciOlusturAsync` olarak düzenlendi.

Neden yapıldı:

- Web API tarafında veritabanı ve Identity gibi I/O işlemlerinde thread bloklamamak gerekir.
- Asenkron akış, özellikle eş zamanlı istek sayısı arttığında API'nin daha verimli çalışmasını sağlar.
- Faz 3 ve Faz 4'e geçmeden önce temel kimlik/personel katmanının daha doğru bir backend pratiğine taşınması hedeflendi.

Doğrulama:

- `dotnet build ITEnvanterTakipSistemi.sln --no-restore` komutu 0 hata ve 0 uyarı ile tamamlandı.

## 2026-08-02

### Faz 4 sınırı netleştirildi

Ne yapıldı:

- Geliştirme kapsamının şimdilik Faz 4'te bırakılmasına karar verildi.
- Daha önce deneme olarak eklenen Faz 5 ve Faz 6 kapsamındaki `ZimmetServisi` ve `ApiGateway` dosyaları kaldırıldı.
- Solution dosyasında yalnızca şu projelerin kalması doğrulandı:
  - `EnvanterTakip.MvcClient`
  - `KimlikVePersonelServisi.Api`
  - `EnvanterServisi.Api`

Neden yapıldı:

- Projenin mevcut aşamada kimlik/personel, envanter ve MVC client temelini sağlamlaştırması hedeflendi.
- Zimmet, ApiGateway ve event tabanlı konular daha sonra ayrı fazlarda ele alınacaktır.

Doğrulama:

- `dotnet build ITEnvanterTakipSistemi.sln --no-restore` komutu 0 hata ve 0 uyarı ile tamamlandı.

### MVC client hata yönetimi ve AktifMi pasifleştirme akışı güncellendi

Ne yapıldı:

- MVC client tarafında ortak sonuç modelleri eklendi:
  - `ApiIslemSonucu<T>`
  - `ApiListeSonucu<T>`
- Kimlik/personel ve envanter API client sınıflarında servis kapalı, yetkisiz, rol yetersiz ve beklenmeyen cevap durumları Türkçe hata mesajlarıyla yakalanacak hale getirildi.
- Listeleme çağrılarında hata olduğunda sessizce boş liste göstermek yerine kullanıcıya açık hata mesajı gösterilmesi sağlandı.
- Departman ve personel güncelleme işlemleri client tarafına eklendi.
- Departman, personel, kategori, lokasyon, cihaz ve sarf malzeme için `AktifMi` alanı üzerinden pasifleştirme yaklaşımı client tarafında desteklendi.
- Envanter ekranındaki checkbox gönderim sırası düzeltildi; işaretli checkbox değerlerinin yanlışlıkla `false` okunma riski giderildi.

Neden yapıldı:

- Kayıt silmek yerine pasifleştirme yapmak proje veri bütünlüğü açısından daha güvenli bulundu.
- Client ekranlarında servis hatalarının boş veri gibi görünmesi kullanıcıyı yanıltıyordu.
- `AktifMi` yaklaşımı backend'de bulunduğu için aynı davranışın MVC client üzerinden de yönetilebilir olması gerekiyordu.

Doğrulama:

- `dotnet build ITEnvanterTakipSistemi.sln --no-restore` komutu 0 hata ve 0 uyarı ile tamamlandı.

### Personel yönetimi ayrı düzenleme ve onay sayfalarına taşındı

Ne yapıldı:

- Kontrol panelindeki Personeller sekmesi tablo yapısına dönüştürüldü.
- Personel tablosunda şu alanlar gösterildi:
  - Ad Soyad
  - Departman
  - Sorumlu mu?
  - Aktif mi?
  - E-posta
- Personel arama eklendi. Arama ad, soyad, ad soyad ve e-posta alanlarında çalışır.
- Departmana göre personel filtreleme eklendi.
- Satır içi personel düzenleme kaldırıldı.
- `PersonelDuzenle` sayfası eklendi.
- Personel düzenleme kaydedildikten sonra kullanıcı tekrar kontrol panelindeki Personeller sekmesine yönlendirilir.
- `PersonelIstenAyrilOnay` sayfası eklendi.
- İşten ayrıldı yap işlemi artık ayrı onay sayfasından çalışır.
- Onay sonrası kullanıcı Personeller sekmesine döner ve `{Ad Soyad} {Departman} personeli işten ayrıldı yapıldı` formatında başarı mesajı görür.
- `KimlikPersonelApiClient` içine tekil personel getirme desteği eklendi.

Neden yapıldı:

- Personel sayısı arttığında tek sayfa üzerinde satır içi düzenleme yönetimi zorlaşır.
- Düzenleme ve işten ayrılma gibi kritik işlemler ayrı ekranlarda daha anlaşılır ve kontrollü yapılır.
- İşten ayrılma işlemi bağlı kullanıcı hesabını da pasifleştirdiği için açık onay adımı gerektirir.

Doğrulama:

- `dotnet build ITEnvanterTakipSistemi.sln --no-restore` komutu 0 hata ve 0 uyarı ile tamamlandı.

### Cihaz durum enum verileri için migration eklendi

Ne yapıldı:

- Cihaz durum enum adları dokümandaki güncel modele uygun hale getirildikten sonra eski veritabanı kayıtlarında eski string değerler kaldığı görüldü.
- Bu nedenle `CihazDurumuEskiDegerleriniGuncelle` migration'ı eklendi.
- Migration şu dönüşümleri yapar:
  - `DepodaHazir` -> `Kullanilabilir`
  - `Arizali` -> `Bakimda`
  - `HurdaIskartaDepoda` -> `HurdaIskarta`
  - `EldenCikarildi` -> `KullanimDisi`
- Migration mevcut PostgreSQL veritabanına uygulandı.

Neden yapıldı:

- EF Core string enum conversion kullanıldığı için veritabanındaki eski enum adları yeni enum değerlerine çevrilmeden cihaz listesi okunamıyordu.
- Veri migration'ı ile hem mevcut veriler düzeltildi hem de aynı sorunun diğer geliştirme ortamlarında tekrar etmesi engellendi.

Doğrulama:

- `dotnet ef database update --project src\servisler\EnvanterServisi\EnvanterServisi.Api\EnvanterServisi.Api.csproj --startup-project src\servisler\EnvanterServisi\EnvanterServisi.Api\EnvanterServisi.Api.csproj` komutu başarıyla tamamlandı.
- `dotnet build ITEnvanterTakipSistemi.sln --no-restore` komutu 0 hata ve 0 uyarı ile tamamlandı.

## 2026-08-03

### Envanter client cihaz ve sarf malzeme yönetimi ayrıştırıldı

Ne yapıldı:

- Envanter ekranındaki Cihazlar sekmesi satır içi düzenleme yerine tablo listeleme yapısına taşındı.
- Cihaz tablosunda cihaz adı, marka/model, seri no, asset tag, kategori, lokasyon, durum ve aktiflik bilgileri gösterilecek hale getirildi.
- Cihaz işlemleri için `CihazIslemleri` sayfası eklendi.
- Cihaz bilgisi güncelleme ve cihaz stok hareketi işleme formları bu ayrı işlem sayfasına taşındı.
- Sarf Malzemeler sekmesi de tablo listeleme yapısına taşındı.
- Sarf malzeme tablosunda ad, kategori, lokasyon, miktar, kritik stok seviyesi, birim ve aktiflik bilgileri gösterilecek hale getirildi.
- Sarf malzeme işlemleri için `SarfMalzemeIslemleri` sayfası eklendi.
- Sarf malzeme bilgisi güncelleme ve sarf malzeme stok hareketi işleme formları bu ayrı işlem sayfasına taşındı.
- `EnvanterApiClient` içine tekil cihaz ve tekil sarf malzeme getirme metotları eklendi.
- Envanter sekmeleri işlem sonrası ilgili sekmede kalacak şekilde yönlendirme desteği aldı.

Neden yapıldı:

- Cihaz ve sarf malzeme sayısı arttığında tek sayfada hem liste hem düzenleme hem stok hareketi formu kullanışsız hale geliyordu.
- Ana envanter ekranının listeleme ve hızlı tarama amacıyla sade kalması, detay işlemlerinin ayrı sayfada yapılması daha sürdürülebilir bulundu.
- Stok hareketi gibi dikkat gerektiren işlemlerin ayrı işlem ekranında yapılması kullanıcı hatası riskini azaltır.

Doğrulama:

- `dotnet build ITEnvanterTakipSistemi.sln --no-restore` komutu 0 hata ve 0 uyarı ile tamamlandı.

### Cihaz AssetTag, filtreleme ve stok hareketi geçmişi iyileştirildi

Ne yapıldı:

- Yeni cihaz oluştururken `AssetTag` alanı MVC client formundan kaldırıldı.
- EnvanterServisi, boş gelen `AssetTag` için `BT-000001` formatında sıradaki demirbaş numarasını otomatik üretir hale getirildi.
- `AssetTagBosCihazlariDoldur` migration'ı eklendi. Bu migration mevcut veritabanında `AssetTag` değeri boş olan cihazları benzersiz `BT-...` değerleriyle doldurur.
- Cihaz güncelleme ekranında `AssetTag` salt okunur bilgi olarak gösterilir ve form gönderiminde mevcut değer korunur.
- `GET /api/cihazlar` endpointine `aktifMi` filtresi eklendi.
- MVC Cihazlar sekmesine aktiflik, kategori ve lokasyon filtreleri eklendi.
- `GET /api/stok/hareketler` endpointi `cihazId` ve `sarfMalzemeId` query filtrelerini destekler hale getirildi.
- `CihazIslemleri` sayfasında ilgili cihazın stok hareketi geçmişi gösterilmeye başlandı.
- Cihaz stok çıkışında çalınma, kaybolma, elden çıkarılmış hurda/ıskarta, manuel stok çıkışı, kullanım ömrü bitişi ve fiziksel sayım düzeltmesi cihazı otomatik pasif ve toplam varlık dışı yapacak şekilde netleştirildi.

Neden yapıldı:

- Asset tag kurum içi kalıcı demirbaş numarası olduğu için kullanıcı hatasına açık elle giriş yerine sistem tarafından üretilmelidir.
- Cihaz sayısı arttığında aktif/pasif, kategori ve lokasyon filtreleri olmadan liste yönetimi zorlaşır.
- Stok hareketi işlendiğinde geçmişin görünmemesi operasyon takibini eksik bırakıyordu.

Doğrulama:

- `dotnet build ITEnvanterTakipSistemi.sln --no-restore` komutu 0 hata ve 0 uyarı ile tamamlandı.

