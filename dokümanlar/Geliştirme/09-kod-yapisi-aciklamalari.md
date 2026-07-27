# Kod Yapısı Açıklamaları

Bu doküman, kodlama sırasında kullanılan klasörlerin ve teknik yapıların ne işe yaradığını açıklamak için tutulur.

## İsimlendirme Yaklaşımı

Projede domain kavramları Türkçe tutulur; fakat teknik literatürde doğal kullanılan kelimeler gereksiz yere Türkçeleştirilmez.

Türkçe kalması uygun örnekler:

- `Personel`
- `Departman`
- `Zimmet`
- `Kullanici`
- `SifreServisi`

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

## Services Klasörü Altındakiler Ne Yapıyor?

`Services` klasörü iş kurallarının ve teknik servislerin bulunduğu katmandır.

- `IKimlikPersonelServisi`: Endpointlerin çağırdığı ana servis sözleşmesidir.
- `KimlikPersonelServisi`: Departman, personel, kullanıcı ve giriş işlemlerindeki iş kurallarını uygular.
- `ISifreServisi` / `SifreServisi`: Şifre kural kontrolü, şifre hash üretimi ve şifre doğrulama işlemlerini yapar.
- `ITokenServisi` / `JwtTokenServisi`: Başarılı giriş sonrası JWT token üretir.
- `Sonuc<T>`: Servis işlemlerinde başarılı/başarısız sonucu ve hata mesajını standartlaştırır.

## Repositories Klasörü Nedir?

Repository pattern bu aşamada `Repositories` klasörüyle uygulanmıştır.

Repository sınıfları EF Core ve veritabanı sorgularını kapsar:

- `IDepartmanRepository` / `EfDepartmanRepository`
- `IPersonelRepository` / `EfPersonelRepository`
- `IKullaniciRepository` / `EfKullaniciRepository`

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
