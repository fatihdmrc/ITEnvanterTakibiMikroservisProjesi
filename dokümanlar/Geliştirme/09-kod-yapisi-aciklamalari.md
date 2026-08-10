# Kod Yapısı Açıklamaları

Bu doküman, kodlama sırasında kullanılan klasörlerin ve teknik yapıların ne işe yaradığını açıklamak için tutulur.

## İsimlendirme Yaklaşımı

Projede domain kavramları Türkçe tutulur; fakat teknik literatürde doğal kullanılan kelimeler gereksiz yere Türkçeleştirilmez.

Türkçe kalması uygun örnekler:

- `Personel`
- `Departman`
- `Zimmet`
- `Kullanici`
- `UygulamaKullanici`

İngilizce kalması daha doğru örnekler:

- `JWT`
- `token`
- `claim`
- `hash`
- `salt`
- `session`
- `repository`
- `seed`
- `client`

Amaç, kodu Türkçeleştirmek için komik veya yapay görünen terimler üretmek değil; projeyi okuyan kişinin kodu doğal şekilde anlamasını sağlamaktır.

## DataProtectionKeys Nedir?

`DataProtectionKeys`, ASP.NET Core'un Data Protection sistemi tarafından kullanılan anahtarların tutulduğu klasördür.

Bu anahtarlar şu işler için kullanılır:

- Session cookie bilgisini korumak
- Antiforgery token üretmek ve doğrulamak
- Cookie tabanlı framework verilerini şifrelemek veya imzalamak

Bu projede hem `KimlikVePersonelServisi.Api` hem de `EnvanterTakip.MvcClient` için DataProtection anahtarları proje içindeki yerel `DataProtectionKeys` klasörüne alınmıştır. Bunun nedeni, Windows geliştirme ortamında varsayılan kullanıcı profili anahtarlarında veya EventLog yetkilerinde hata yaşanabilmesidir.

Bu klasör kaynak koda dahil edilmez. `.gitignore` içine eklenmiştir. Silinirse uygulama yeni anahtar üretir; yalnızca eski session/cookie bilgileri geçersiz olur.

## Contracts Klasörü Nedir?

`Contracts` klasörü API'nin dış dünya ile konuşurken kullandığı request ve response modellerini tutar.

Örnekler:

- `GirisIstek`: Login endpointine gönderilen kullanıcı adı ve şifre bilgisidir.
- `GirisCevap`: Login başarılı olunca dönen JWT token ve kullanıcı bağlamıdır.
- `DepartmanOlusturIstek`: Departman oluşturma endpointinin beklediği gövdedir.
- `PersonelCevap`: Personel listeleme veya detay endpointlerinin döndürdüğü cevaptır.

`Contracts` ile `Domain/Entities` ayrımı önemlidir. Entity sınıfları veritabanı ve domain modelidir; contracts ise API sözleşmesidir. Böylece veritabanındaki her alanı istemciye açmak zorunda kalmayız ve API cevabını kontrollü tasarlarız.

## JWT Ayarları Nedir?

JWT ayarları `appsettings.json` içinde `Jwt` bölümünde tutulur ve `JwtAyarlari` sınıfına bağlanır.

Alanlar:

- `Issuer`: Token'ı üreten servisi belirtir.
- `Audience`: Token'ın hangi istemci veya sistem için üretildiğini belirtir.
- `SigningKey`: Token imzası için kullanılan gizli anahtardır.
- `GecerlilikDakikasi`: Token'ın kaç dakika geçerli olacağını belirtir.

Geliştirme ortamında `SigningKey` appsettings içinde durabilir. Canlı ortamda bu değer environment variable, secret manager veya güvenli secret store üzerinden verilmelidir.

## ASP.NET Core Identity Neden Kullanılıyor?

KimlikVePersonelServisi artık custom kullanıcı/şifre tablosu yerine ASP.NET Core Identity kullanır.

Identity şu sorumlulukları üstlenir:

- Kullanıcı kaydı
- Şifre hashleme ve doğrulama
- Rol yönetimi
- Security stamp ve lockout gibi hesap güvenliği alanları
- Kullanıcı ile rol arasındaki bağlantı tabloları

Bu projede Identity, cookie login için değil kullanıcı, rol ve şifre altyapısı için kullanılır. API tarafında oturum taşımak için JWT kullanılmaya devam eder.

Temel entity:

- `UygulamaKullanici`: `IdentityUser<Guid>` sınıfından türeyen uygulama kullanıcısıdır. Ek olarak `PersonelId` ve `AktifMi` alanlarını taşır.

Temel servisler:

- `UserManager<UygulamaKullanici>`: Kullanıcı oluşturma, kullanıcı bulma, şifre doğrulama altyapısı ve kullanıcı güncelleme işlemlerini sağlar.
- `RoleManager<IdentityRole<Guid>>`: Roller var mı, yoksa oluşturulmalı mı gibi rol yönetimi işlerini sağlar.
- `SignInManager<UygulamaKullanici>`: Login sırasında şifre doğrulama ve lockout davranışını Identity kurallarına göre yürütür.

## Services Klasörü Altındakiler Ne Yapıyor?

`Services` klasörü iş kurallarının ve teknik servislerin bulunduğu katmandır.

- `IKimlikPersonelServisi`: Endpointlerin çağırdığı ana servis sözleşmesidir.
- `KimlikPersonelServisi`: Departman, personel, kullanıcı ve giriş işlemlerindeki iş kurallarını uygular.
- `ITokenServisi` / `JwtTokenServisi`: Başarılı giriş sonrası JWT token üretir.
- `Sonuc<T>`: Servis işlemlerinde başarılı/başarısız sonucu ve hata mesajını standartlaştırır.

## Repositories Klasörü Nedir?

Repository pattern bu aşamada `Repositories` klasörüyle uygulanmıştır.

Repository sınıfları EF Core ve veritabanı sorgularını kapsar:

- `IGenericRepository<T>` / `EfGenericRepository<T>`
- `IDepartmanRepository` / `EfDepartmanRepository`
- `IPersonelRepository` / `EfPersonelRepository`

`IGenericRepository<T>` ortak CRUD operasyonlarını sağlar:

- Listeleme
- Id ile getirme
- Id var mı kontrolü
- Ekleme
- Değişiklikleri kaydetme

`IDepartmanRepository` ve `IPersonelRepository` gibi özel repository arayüzleri generic repository'den miras alır. Böylece ortak metotları tekrar yazmaz, sadece kendi domain'ine özel sorguları ekler.

Örnek:

- `IDepartmanRepository`: `AdKullaniliyorMuAsync`, `AktifVarMiAsync`
- `IPersonelRepository`: `EmailKullaniliyorMuAsync`

Not: Kullanıcı işlemleri için ayrıca repository tutulmaz. Bu alan ASP.NET Core Identity'nin `UserManager`, `RoleManager` ve `SignInManager` servisleriyle yönetilir.

Bu ayrım sayesinde:

- Endpointler doğrudan veritabanını bilmez.
- İş kuralları `KimlikPersonelServisi` içinde kalır.
- EF Core sorguları repository sınıflarında toplanır.
- İleride test veya farklı veri kaynağı ihtiyacı olursa servis kodu daha az değişir.

## MVC Client Tarafında Ne Eklendi?

`EnvanterTakip.MvcClient`, şu an KimlikVePersonelServisi'ne doğrudan HTTP ile bağlanır.

Eklenen parçalar:

- `KimlikPersonelApiClient`: MVC uygulamasının API'ye istek atmasını sağlar.
- `KimlikPersonelModelleri`: MVC ekranında kullanılan form ve liste modelleridir.
- `HomeController`: Login, çıkış, departman oluşturma, personel oluşturma, kullanıcı oluşturma ve personeli işten ayrıldı yapma işlemlerini yönetir.
- `Views/Home/Index.cshtml`: Kimlik ve personel kontrol panelidir.

ApiGateway daha sonra eklendiğinde MVC client'taki servis adresi `appsettings.json` içinden gateway adresine çevrilecektir.

## Controllers Klasörü Nedir?

`Controllers` klasörü HTTP endpointlerini tutar. Dışarıdan gelen istek önce controller action metoduna gelir, controller gerekli contract modelini alır ve iş kuralını çalıştırmak için servis katmanını çağırır.

Kimlik ve personel servisinde şu controller'lar bulunur:

- `SaglikController`: `GET /saglik` endpointini sağlar.
- `KimlikController`: `POST /api/kimlik/giris` endpointini sağlar.
- `DepartmanlarController`: departman listeleme, detay, oluşturma ve güncelleme endpointlerini sağlar.
- `PersonellerController`: personel listeleme, detay, oluşturma, güncelleme ve işten ayrıldı işlemlerini sağlar.
- `KullanicilarController`: kullanıcı listeleme ve oluşturma endpointlerini sağlar.

Genel akış:

```text
HTTP Request
    -> Controller
    -> Service
    -> Repository
    -> DbContext
    -> PostgreSQL
```

Controller içinde iş kuralı yazmamaya dikkat edilir. Controller'ın görevi HTTP isteğini almak, servisi çağırmak ve uygun HTTP cevabını döndürmektir.

## MVC Client Güncel Akışı

MVC client, Faz 4 sınırında iki ana ekran grubuna ayrılmıştır:

- `HomeController`: Kimlik, departman, personel ve kullanıcı yönetimi ekranlarını yönetir.
- `EnvanterController`: Kategori, lokasyon, cihaz, sarf malzeme ve stok ekranlarını yönetir.

Kimlik/personel client akışındaki önemli parçalar:

- `Views/Home/Index.cshtml`: Kontrol panelidir. Departmanlar, Personeller ve Kullanıcılar sekmelerini içerir.
- Personeller sekmesi artık tablo şeklindedir. Personel arama ve departmana göre filtreleme bu sekmede yapılır.
- `Views/Home/PersonelDuzenle.cshtml`: Tek bir personelin düzenlendiği ayrı sayfadır.
- `Views/Home/PersonelIstenAyrilOnay.cshtml`: Personeli işten ayrıldı yapmadan önce kullanılan onay sayfasıdır.
- Personel düzenleme veya işten ayrılma işlemi tamamlandıktan sonra kullanıcı tekrar kontrol panelindeki Personeller sekmesine yönlendirilir.

API client tarafındaki önemli parçalar:

- `ApiIslemSonucu<T>`: Tekil işlem sonucunu, varsa dönen veriyi ve hata mesajını taşır.
- `ApiListeSonucu<T>`: Listeleme işlemlerinde veri ile hata durumunu ayrı ayrı taşır.
- `KimlikPersonelApiClient`: KimlikVePersonelServisi'ne yapılan HTTP isteklerini kapsar.
- `EnvanterApiClient`: EnvanterServisi'ne yapılan HTTP isteklerini kapsar.
- `ZimmetApiClient`: ZimmetServisi'ne yapılan HTTP isteklerini kapsar.

Listeleme çağrıları başarısız olduğunda client artık boş listeyi sessizce göstermek yerine Türkçe hata mesajı üretir. Bu sayede servis kapalı, oturum süresi dolmuş, yetki yetersiz veya beklenmeyen cevap gibi durumlar kullanıcı tarafından ayırt edilebilir.

## AktifMi ile Pasifleştirme Yaklaşımı

Bu projede yönetimsel silme işlemleri için doğrudan fiziksel silme yerine `AktifMi` alanı kullanılır.

Bu yaklaşım şu entitylerde uygulanır:

- `Departman`
- `Personel`
- `Kategori`
- `Lokasyon`
- `Cihaz`
- `SarfMalzeme`

Not: Cihazlarda `AktifMi` artık doğrudan kullanıcı tarafından değiştirilen bir pasifleştirme alanı değildir. Cihazın aktifliği ve toplam varlık kapsamı cihaz durumu ile elden çıkarma tipinden servis katmanında hesaplanır.

Amaç:

- Geçmiş kayıtlarla ilişkileri korumak
- Audit ve raporlama senaryolarında veri kaybını önlemek
- Yanlışlıkla yapılan silme işlemlerinin etkisini azaltmak

Personel işten ayrıldı yapıldığında normal pasifleştirmeden daha özel bir iş kuralı çalışır. Personelin durumu `IstenAyrildi` olur, `AktifMi` değeri `false` yapılır ve bağlı kullanıcı hesabı da pasifleştirilir.

## Cihaz Durumu Enum Migration Notu

EnvanterServisi cihaz durumlarını veritabanında string olarak saklar. Bu nedenle enum adları değiştirildiğinde mevcut veritabanındaki eski string değerlerin de migration ile dönüştürülmesi gerekir.

`CihazDurumuEskiDegerleriniGuncelle` migration'ı şu eski değerleri yeni değerlere dönüştürür:

- `DepodaHazir` -> `Kullanilabilir`
- `Arizali` -> `Bakimda`
- `HurdaIskartaDepoda` -> `HurdaIskarta`
- `EldenCikarildi` -> `KullanimDisi`

Bu migration uygulanmadan eski veritabanı kayıtları okunmaya çalışılırsa EF Core enum conversion hatası verir.

## Envanter MVC Client İşlem Sayfaları

Envanter yönetimi tarafında ana `Views/Envanter/Index.cshtml` ekranı listeleme ve yeni kayıt oluşturma amacıyla sade tutulur.

- `Views/Envanter/Index.cshtml`: Stok özeti, kategori, lokasyon, cihaz ve sarf malzeme sekmelerini gösterir. Cihaz ve sarf malzeme sekmeleri tablo listeleme yapısındadır.
- `Views/Envanter/CihazIslemleri.cshtml`: Tek bir cihazın bilgi güncelleme ve cihaz durum hareketi işleme formlarını içerir.
- `Views/Envanter/SarfMalzemeIslemleri.cshtml`: Tek bir sarf malzemenin bilgi güncelleme, stok hareketi işleme ve stok hareketi geçmişi görüntüleme alanlarını içerir.

`EnvanterController` içinde `CihazIslemleri` ve `SarfMalzemeIslemleri` GET action'ları ilgili kaydı API'den tekil olarak çeker. Bu nedenle `EnvanterApiClient` içinde `CihazGetirAsync` ve `SarfMalzemeGetirAsync` metotları bulunur.

Bu ayrım sayesinde büyük listelerde ana ekran taranabilir kalır; düzenleme, cihaz durum hareketi ve sarf malzeme stok hareketi gibi detay işlemler ayrı sayfada yapılır.

## Cihaz AssetTag ve Durum Hareketi Akışı

Yeni cihaz oluşturma isteklerinde `AssetTag` boş gelirse `EnvanterYonetimServisi` sıradaki `BT-000001` formatlı değeri üretir. MVC client cihaz oluşturma formu artık asset tag alanı göstermez; cihaz düzenleme sayfasında değer salt okunur gösterilir.

`GET /api/cihazlar` endpointi kategori, lokasyon, durum, arama ve `aktifMi` filtrelerini destekler. MVC Envanter ekranındaki Cihazlar sekmesi aktiflik, kategori ve lokasyon filtrelerini bu endpoint üzerinden uygular.

Cihaz tarafında adet bazlı stok hareketi yerine “cihaz durum hareketi” kavramı kullanılır. MVC client yeni hareketler için `POST /api/cihazlar/{id}/durum-hareketleri` endpointini çağırır. Cihaz tarafındaki eski `POST /api/cihazlar/{id}/stok-hareketleri` endpointi kaldırılmıştır.

Durum hareketi geçmişi teknik olarak mevcut `StokHareketleri` tablosunda, `CihazId` dolu olacak şekilde tutulur. `GET /api/stok/hareketler` endpointi cihaz veya sarf malzeme id'siyle filtrelenebilir. `CihazIslemleri` sayfası ilgili cihazın geçmişini `Cihaz Durum Geçmişi` başlığıyla gösterir.

`AktifMi` ve `ToplamVarligaDahilMi` cihaz formunda kullanıcı tarafından değiştirilmez. `EnvanterYonetimServisi` içindeki `CihazKapsamAlanlariniDurumaGoreGuncelle` metodu cihaz durumuna ve elden çıkarma tipine göre bu alanları hesaplar.

Cihazın `Durum`, `EnvanterdenCikisTarihi`, `EldenCikarmaTipi`, `EldenCikarmaAciklamasi` ve `SatilanKisiVeyaKurum` alanları cihaz bilgi güncelleme isteğiyle yazılmaz. Bu alanlar `Cihaz Durum Hareketi` akışı üzerinden değişir ve geçmişe kaydedilir.

- `Kullanilabilir`, `Zimmetli`, `Incelemede`, `Bakimda`, `HasarliTeslimAlindi` ve kurumda duran `HurdaIskarta` cihazlar aktif ve toplam varlığa dahil kabul edilir.
- `Kayip`, `Calindi`, `KullanimDisi` ve elden çıkarılmış `HurdaIskarta` cihazlar pasif ve toplam varlık dışı kabul edilir.
- Pasif ve toplam varlık dışı cihazlarda envanterden çıkış tarihi sistem tarafından doldurulur.
- `Ariza` hareketi cihazı `Bakimda` yapar.
- `BakimdanDondu` hareketi cihazı tekrar `Kullanilabilir` yapar.
- `IncelemeyeAlindi` hareketi cihazı `Incelemede` yapar.
- `HasarliTeslimAlindi` hareketi cihazı `HasarliTeslimAlindi` yapar.
- `Zimmetlendi` hareketi cihazı `Zimmetli` yapar.
- `ZimmetIadeAlindi` hareketi cihazı `Incelemede` yapar.
- `EnvantereGiris` cihaz durum hareketi olarak kullanılmaz; yeni cihaz oluşturma akışına aittir.

Sarf malzemelerde stok hareketi kavramı aynen korunur. Çünkü sarf malzemelerde giriş, çıkış ve düzeltme gerçek miktar değişimi üretir.
Sarf malzeme stok hareketi formu ve servis doğrulaması cihaz durumuna özel nedenleri kabul etmez.
Sarf malzeme stok hareketi geçmişi de aynı `GET /api/stok/hareketler?sarfMalzemeId=...` endpointinden alınır ve `SarfMalzemeIslemleri` sayfasında `Stok Hareketi Geçmişi` olarak gösterilir.

## ZimmetServisi Kod Yapısı

`ZimmetServisi.Api`, Faz 5 kapsamında ayrı Web API projesi olarak eklenmiştir.

Temel klasörler:

- `Domain/Entities`: `Zimmet` entity'sini tutar.
- `Domain/Enums`: `ZimmetDurumu` ve `IadeKontrolDurumu` enumlarını tutar.
- `Contracts/Zimmetler`: API request/response modellerini tutar.
- `Data`: `ZimmetDbContext` ve migration dosyalarını tutar.
- `Repositories`: Zimmet sorguları ve ortak repository işlemlerini tutar.
- `Services`: Zimmet iş kurallarını tutar.
- `Services/Harici`: KimlikVePersonelServisi ve EnvanterServisi HTTP clientlarını tutar.
- `Controllers`: `SaglikController` ve `ZimmetlerController` endpointlerini tutar.

ZimmetServisi verileri PostgreSQL içinde `zimmet` şemasında saklanır. `Zimmetler` tablosunda `Aktif` ve `IadeSurecinde` durumları açık zimmet kabul edilir; bu nedenle aynı cihaz için bu durumlarda yalnızca bir kayıt bulunabilir.

Servisler arası iletişim Faz 5'te HTTP ile yapılır:

- Personel uygunluğu `GET /api/personeller/{id}` ile doğrulanır.
- Cihaz uygunluğu `GET /api/cihazlar/{id}` ile doğrulanır.
- Cihaz durumu `POST /api/cihazlar/{id}/durum-hareketleri` ile güncellenir.

ZimmetServisi cihaz durumunu doğrudan veritabanında değiştirmez. Cihaz yaşam döngüsünün tek sahibi EnvanterServisi olarak kalır.

## Zimmet MVC Client Akışı

MVC client tarafında zimmet yönetimi için şu parçalar eklenmiştir:

- `ZimmetApiClient`: ZimmetServisi endpointlerini çağırır.
- `ZimmetModelleri`: Zimmet listeleme, oluşturma, iade alma ve iade kontrolü formlarını tutar.
- `ZimmetController`: Zimmetler ekranını ve form işlemlerini yönetir.
- `Views/Zimmet/Index.cshtml`: Zimmet listesini ve zimmet oluşturma formunu gösterir.
- `Views/Zimmet/IadeAl.cshtml`: Aktif zimmetin iade alınması için onay/form ekranıdır.
- `Views/Zimmet/IadeKontrolu.cshtml`: İade sürecindeki zimmetin fiziki kontrol sonucunu kaydeder.

Admin ve ITPersoneli rolleri tüm zimmetleri yönetebilir. PersonelKullanicisi rolü yalnızca kendi zimmetlerini görebilir. Zimmet ve iade fotoğrafı için tablo, endpoint veya UI alanı bulunmaz.

## Faz 6 CAP/RabbitMQ Kod Yapısı

Faz 6 ile event üretici servislerde DotNetCore.CAP yapılandırılmıştır.

- `KimlikVePersonelServisi.Api/Contracts/Events`: `personel.isten-ayrildi` event modeli ve event adı sabitlerini tutar.
- `EnvanterServisi.Api/Contracts/Events`: `cihaz.durumu-degisti` ve `stok.kritik-seviyeye-dusuldu` event modellerini tutar.
- `ZimmetServisi.Api/Contracts/Events`: zimmet oluşturma, iade alma, kontrol ve hasarlı teslim alma event modellerini tutar.
- Her API projesindeki `Program.cs`, PostgreSQL outbox ve RabbitMQ transport ayarlarını içerir.

CAP outbox şemaları servis bazında ayrıdır:

- KimlikVePersonelServisi: `cap_kimlik`
- EnvanterServisi: `cap_envanter`
- ZimmetServisi: `cap_zimmet`

Event yayınlayan iş akışları:

- Personel işten ayrıldığında KimlikVePersonelServisi `personel.isten-ayrildi` eventini üretir.
- Cihaz durum hareketi işlendiğinde EnvanterServisi `cihaz.durumu-degisti` eventini üretir.
- Cihaz veya sarf malzeme kritik stok eşiğinin altına düştüğünde EnvanterServisi `stok.kritik-seviyeye-dusuldu` eventini üretir.
- Zimmet oluşturma ve iade akışlarında ZimmetServisi ilgili zimmet ve cihaz kontrol eventlerini üretir.

DenetimKaydiServisi ve BildirimServisi henüz eklenmediği için bu fazda event consumer sınıfı bulunmaz.
