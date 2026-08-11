# Çalıştırma Rehberi

Bu doküman, projeyi geliştirme ortamında nasıl ayağa kaldıracağını anlatır.

## 1. Gerekenler

Bilgisayarda şu araçlar bulunmalıdır:

- .NET 8 SDK
- Docker Desktop
- PowerShell veya Windows Terminal

Projede `global.json` ile .NET 8 SDK hedeflenmiştir.

## 2. Proje Klasörüne Geç

PowerShell açıp proje klasörüne geç:

```powershell
cd "C:\Users\fathd\Desktop\IT Ekipman Takip Sistemi"
```

## 3. .NET Paketlerini Geri Yükle

Paketler normal NuGet cache üzerinden geri yüklenir:

```powershell
dotnet restore ITEnvanterTakipSistemi.sln
```

## 4. Projeyi Derle

Derleme almadan önce çalışan servis terminali varsa `Ctrl + C` ile kapat. Çalışan uygulama kapanmazsa Windows `.exe` dosyasını kilitler ve build sırasında şu tarz hata alınır:

```text
The process cannot access the file ... because it is being used by another process.
```

```powershell
dotnet build ITEnvanterTakipSistemi.sln --no-restore
```

Beklenen sonuç:

```text
Oluşturma başarılı oldu.
0 Uyarı
0 Hata
```

## 5. PostgreSQL ve RabbitMQ'yu Docker ile Başlat

```powershell
docker compose up -d postgres rabbitmq mongodb
```

Çalışıyor mu kontrol etmek için:

```powershell
docker ps
```

PostgreSQL bilgileri:

```text
Host: localhost
Port: 5432
Database: it_envanter_takip
User: itenvanter
Password: itenvanter123
```

RabbitMQ bilgileri:

```text
AMQP Port: 5672
Management UI: http://localhost:15672
User: guest
Password: guest
Exchange: inventory.events
```

PostgreSQL'i durdurmak için:

```powershell
docker compose down
```

## 6. DBeaver ile Veritabanını Görüntüle

DBeaver Community Desktop kurulduktan sonra PostgreSQL veritabanına şu bilgilerle bağlanılır:

```text
Database Type: PostgreSQL
Host: localhost
Port: 5432
Database: it_envanter_takip
Username: itenvanter
Password: itenvanter123
```

İlk bağlantı adımları:

1. DBeaver'ı aç.
2. `New Database Connection` seç.
3. `PostgreSQL` seç.
4. Yukarıdaki bağlantı bilgilerini gir.
5. `Test Connection` butonuna bas.
6. Sürücü indirme uyarısı çıkarsa onayla.
7. Bağlantı başarılıysa `Finish` ile kaydet.

Kimlik ve personel tablolarını görmek için:

1. Sol menüden bağlantıyı aç.
2. `Databases > it_envanter_takip > Schemas > kimlik_personel > Tables` yoluna git.
3. `Departmanlar`, `Personeller`, `Kullanicilar`, `Roller` ve Identity bağlantı tablolarını incele.

Not: EF Core şu an tablo adlarını C# `DbSet` adlarıyla oluşturduğu için tablolar büyük harfle başlar. DBeaver'da küçük harfli tablo adı ararsan görünmüyor gibi düşünebilirsin.

Identity geçişinden sonra `Kullanicilar` tablosunda `PasswordHash`, `SecurityStamp`, `LockoutEnd` gibi standart Identity alanları da görünür. Bunlar bizim elle yazdığımız alanlar değil, ASP.NET Core Identity'nin kullanıcı güvenliği için yönettiği alanlardır.

Envanter tablolarını görmek için:

```text
Databases > it_envanter_takip > Schemas > envanter > Tables
```

Bu schema altında şu tablolar bulunur:

```text
Kategoriler
Lokasyonlar
Cihazlar
SarfMalzemeler
StokHareketleri
KritikStokKurallari
```

Zimmet tablolarını görmek için:

```text
Databases > it_envanter_takip > Schemas > zimmet > Tables
```

Bu schema altında şu tablo bulunur:

```text
Zimmetler
```

## 7. Migration Uygula

PostgreSQL çalıştıktan sonra KimlikVePersonelServisi migration'ını uygula:

```powershell
dotnet ef database update --project "src\servisler\KimlikVePersonelServisi\KimlikVePersonelServisi.Api\KimlikVePersonelServisi.Api.csproj" --startup-project "src\servisler\KimlikVePersonelServisi\KimlikVePersonelServisi.Api\KimlikVePersonelServisi.Api.csproj" --context KimlikPersonelDbContext
```

EnvanterServisi migration'ını uygulamak için:

```powershell
dotnet ef database update --project "src\servisler\EnvanterServisi\EnvanterServisi.Api\EnvanterServisi.Api.csproj" --startup-project "src\servisler\EnvanterServisi\EnvanterServisi.Api\EnvanterServisi.Api.csproj" --context EnvanterDbContext
```

ZimmetServisi migration'ını uygulamak için:

```powershell
dotnet ef database update --project "src\servisler\ZimmetServisi\ZimmetServisi.Api\ZimmetServisi.Api.csproj" --startup-project "src\servisler\ZimmetServisi\ZimmetServisi.Api\ZimmetServisi.Api.csproj" --context ZimmetDbContext
```

Not: EnvanterServisi açılışında `Database.MigrateAsync()` çalıştığı için bekleyen migration'lar servis başlatıldığında da uygulanır. `CihazKapsamAlanlariniDurumaGoreDuzelt` migration'ı mevcut cihazların `AktifMi`, `ToplamVarligaDahilMi` ve çıkış tarihi alanlarını yeni yaşam döngüsü kuralına göre düzeltir.

Not: ZimmetServisi açılışında da `Database.MigrateAsync()` çalışır. Bekleyen `IlkZimmetSemasi` migration'ı servis başlatıldığında otomatik uygulanır.

Not:

```text
EF tools version '8.0.0' is older than runtime '8.0.18'
```

uyarısı görülebilir. Migration başarılı uygulanıyorsa bu uyarı geliştirmeyi engellemez.

## 8. Servisleri Çalıştır

Her uygulamayı ayrı terminal penceresinde çalıştırman en anlaşılır yöntemdir.

### Terminal 1 - KimlikVePersonelServisi

```powershell
cd "C:\Users\fathd\Desktop\IT Ekipman Takip Sistemi"
dotnet run --project "src\servisler\KimlikVePersonelServisi\KimlikVePersonelServisi.Api\KimlikVePersonelServisi.Api.csproj" --launch-profile http
```

Kontrol adresi:

```text
http://localhost:5000/saglik
```

Swagger adresi:

```text
http://localhost:5000/swagger
```

Hazır demo kullanıcılarla JWT deneme sırası:

1. Swagger'da `POST /api/kimlik/giris` endpointini aç.
2. Aşağıdaki demo kullanıcılardan biriyle giriş yap.
3. Dönen `token` değerini kopyala.
4. Swagger'daki `Authorize` butonuna bas.
5. Açılan alana yalnızca token değerini yapıştır. `Bearer` kelimesini Swagger için ayrıca yazmana gerek yoktur.
6. Yetkili bir endpoint çağır. Örneğin `GET /api/departmanlar` endpointi admin veya IT personeli token'ı ile `200 OK` dönmelidir.

Postman veya curl ile denerken ise header şu formatta gönderilir:

```text
Authorization: Bearer JWT_TOKEN_DEGERI
```

Demo kullanıcılar:

```text
admin / Admin123! / Admin
it.personel / ItPersonel123! / ITPersoneli
personel / Personel123! / PersonelKullanicisi
```

Örnek giriş gövdesi:

```json
{
  "kullaniciAdi": "admin",
  "sifre": "Admin123!"
}
```

Başarılı cevapta beklenen temel alanlar:

```json
{
  "token": "JWT_TOKEN_DEGERI",
  "kullaniciId": "KULLANICI_ID",
  "personelId": "PERSONEL_ID",
  "rol": "Admin",
  "gecerlilikZamani": "2026-07-27T..."
}
```

Elle veri oluşturmak istersen deneme sırası:

1. `POST /api/departmanlar` ile departman oluştur.
2. Dönen `id` değerini kullanarak `POST /api/personeller` ile personel oluştur.
3. Dönen personel `id` değerini kullanarak `POST /api/kullanicilar` ile kullanıcı oluştur.
4. `POST /api/kimlik/giris` ile giriş yap.

Örnek departman oluşturma gövdesi:

```json
{
  "ad": "Bilgi İşlem",
  "sorumluPersonelId": null
}
```

Örnek personel oluşturma gövdesi:

```json
{
  "ad": "Fatih",
  "soyad": "Demir",
  "email": "fatih.demir@example.com",
  "departmanId": "DEPARTMAN_ID_BURAYA",
  "unvan": "IT Uzmanı",
  "departmanSorumlusuMu": false,
  "iseGirisTarihi": "2026-07-27"
}
```

Örnek kullanıcı oluşturma gövdesi:

```json
{
  "kullaniciAdi": "fatih.demir",
  "sifre": "Deneme123!",
  "rol": 2,
  "personelId": "PERSONEL_ID_BURAYA"
}
```

Rol değerleri:

```text
1 = Admin
2 = ITPersoneli
3 = PersonelKullanicisi
```

Örnek giriş gövdesi:

```json
{
  "kullaniciAdi": "fatih.demir",
  "sifre": "Deneme123!"
}
```

### Terminal 2 - EnvanterServisi

```powershell
cd "C:\Users\fathd\Desktop\IT Ekipman Takip Sistemi"
dotnet run --project "src\servisler\EnvanterServisi\EnvanterServisi.Api\EnvanterServisi.Api.csproj" --launch-profile http
```

Kontrol adresi:

```text
http://localhost:5001/saglik
```

Swagger adresi:

```text
http://localhost:5001/swagger
```

EnvanterServisi endpointleri JWT ile korunur. Önce KimlikVePersonelServisi üzerinden `admin` veya `it.personel` kullanıcısıyla token alıp EnvanterServisi Swagger ekranındaki `Authorize` alanına yapıştırman gerekir.

Temel endpointler:

```text
GET /api/kategoriler
GET /api/lokasyonlar
GET /api/cihazlar
GET /api/sarf-malzemeler
GET /api/stok/ozet
```

### Terminal 3 - ZimmetServisi

```powershell
cd "C:\Users\fathd\Desktop\IT Ekipman Takip Sistemi"
dotnet run --project "src\servisler\ZimmetServisi\ZimmetServisi.Api\ZimmetServisi.Api.csproj" --launch-profile http
```

Kontrol adresi:

```text
http://localhost:5002/saglik
```

Swagger adresi:

```text
http://localhost:5002/swagger
```

ZimmetServisi endpointleri JWT ile korunur. Zimmet oluşturma, iade alma ve iade kontrolü için `admin` veya `it.personel` token'ı gerekir. `personel` rolündeki kullanıcı yalnızca kendi zimmetlerini `GET /api/zimmetler/benim` endpointiyle görebilir.

Temel endpointler:

```text
GET /api/zimmetler
GET /api/zimmetler/benim
GET /api/zimmetler/{id}
POST /api/zimmetler
POST /api/zimmetler/{id}/iade-alindi
POST /api/zimmetler/{id}/iade-kontrolu
```

### Terminal 4 - DenetimKaydiServisi

```powershell
cd "C:\Users\fathd\Desktop\IT Ekipman Takip Sistemi"
dotnet run --project "src\servisler\DenetimKaydiServisi\DenetimKaydiServisi.Api\DenetimKaydiServisi.Api.csproj" --launch-profile http
```

Kontrol adresi:

```text
http://localhost:5003/swagger
http://localhost:5003/saglik
```

DenetimKaydiServisi endpointleri JWT ile korunur. Denetim kayıtlarını listeleme ve detay görüntüleme için `admin` veya `it.personel` token'ı gerekir.

Temel endpointler:

```text
GET  /api/denetim-kayitlari
GET  /api/denetim-kayitlari/{id}
POST /api/denetim-kayitlari/crud
```

### Terminal 5 - MVC Client

```powershell
cd "C:\Users\fathd\Desktop\IT Ekipman Takip Sistemi"
dotnet run --project "src\istemci\EnvanterTakip.MvcClient\EnvanterTakip.MvcClient.csproj" --launch-profile http
```

Tarayıcı adresi:

```text
http://localhost:5010
```

MVC client üzerinden şu işlemler denenebilir:

- Demo kullanıcı ile giriş yapmak
- Oturumdaki kullanıcı bilgisini görmek
- Departmanları listelemek ve yeni departman oluşturmak
- Personelleri listelemek ve yeni personel oluşturmak
- Personeli işten ayrıldı olarak işaretlemek
- Kullanıcıları listelemek ve yeni kullanıcı oluşturmak
- Envanter ekranından cihaz ve sarf malzeme işlemlerini yönetmek
- Zimmetler ekranından zimmet oluşturmak, iade almak ve iade kontrolünü tamamlamak
- Denetim ekranından event ve CRUD audit kayıtlarını filtrelemek ve detay payload'ını görüntülemek

Not: MVC client şu an doğrudan `http://localhost:5000`, `http://localhost:5001`, `http://localhost:5002` ve `http://localhost:5003` adreslerindeki servislere bağlanır. ApiGateway eklendiğinde bu adresler `appsettings.json` içinden gateway adresine çevrilecektir.

Yetki notu:

- Giriş yapılmadan departman, personel ve kullanıcı endpointleri `401 Unauthorized` döner.
- `Admin` rolü departman, personel ve kullanıcı yönetimi yapabilir.
- `ITPersoneli` rolü departman ve personel yönetimi yapabilir.
- `PersonelKullanicisi` yönetim endpointlerini kullanamaz; Zimmetler ekranında yalnızca kendi zimmet süreçlerini görebilir.

## 9. Şu Anki Durum

Şu an çalışan kapsam:

- Solution iskeleti
- KimlikVePersonelServisi API projesi
- EnvanterServisi API projesi
- ZimmetServisi API projesi
- DenetimKaydiServisi API projesi
- ASP.NET Core MVC client projesi
- PostgreSQL Docker Compose 
- RabbitMQ Docker Compose
- MongoDB Docker Compose
- KimlikVePersonelServisi EF Core migration yapısı
- KimlikVePersonelServisi PostgreSQL bağlantısı
- ASP.NET Core Identity ile kullanıcı, rol ve şifre yönetimi
- KimlikVePersonelServisi JWT token üretimi
- Demo departman, personel ve kullanıcı kayıtları
- Her iki API için `/saglik` endpointi
- DBeaver Community ile veritabanı görüntüleme
- Repository pattern ile ayrılmış veri erişim katmanı
- Controller, service ve repository şeklinde ayrılmış KimlikVePersonelServisi API mimarisi
- Kimlik ve personel işlemlerini kullanan MVC kontrol paneli
- EnvanterServisi EF Core migration yapısı
- EnvanterServisi kategori, lokasyon, cihaz ve sarf malzeme CRUD endpointleri
- EnvanterServisi kullanılabilir stok ve kritik stok özeti endpointi
- ZimmetServisi EF Core migration yapısı
- ZimmetServisi zimmet oluşturma, iade alma, iade kontrolü ve kendi zimmetlerini listeleme endpointleri
- MVC client Zimmetler ekranı
- MVC client Denetim ekranı
- DotNetCore.CAP + RabbitMQ event yayınlama altyapısı
- CAP Outbox şemaları: `cap_kimlik`, `cap_envanter`, `cap_zimmet`
- MongoDB audit log koleksiyonu: `DenetimKayitlari`

Henüz eklenmeyenler:

- ApiGateway
- Redis cache
- SignalR bildirimleri
Swagger ve EF Core paketleri proje dosyalarına eklenmiştir.

## 10. Sık Karşılaşılan Hata: Dosya Kilitli

Build sırasında şu hata görülürse:

```text
Dosya şunun tarafından kilitlendi: "EnvanterServisi.Api (21256)"
```

Bu, ilgili uygulamanın hâlâ çalıştığı anlamına gelir. Çözüm olarak:

1. Servisin çalıştığı terminale geç.
2. `Ctrl + C` ile uygulamayı durdur.
3. Build komutunu tekrar çalıştır.

Eğer terminali bulamıyorsan PowerShell'de ilgili process id ile kapatabilirsin:

```powershell
Stop-Process -Id 21256 -Force
```

Buradaki `21256` örnektir; hata mesajında hangi process id yazıyorsa onu kullanmalısın.

## 11. Güncel MVC Client Notları - 2026-08-02

MVC client üzerinden şu işlemler güncel olarak denenebilir:

- Demo kullanıcı ile giriş yapmak
- Departmanları listelemek, oluşturmak, güncellemek ve `AktifMi` ile pasifleştirmek
- Personelleri tablo halinde listelemek
- Personelleri ad, soyad veya e-posta ile aramak
- Personelleri departmana göre filtrelemek
- Personel oluşturmak
- Personeli ayrı düzenleme sayfasında güncellemek
- Personeli ayrı onay sayfası üzerinden işten ayrıldı yapmak
- Kullanıcıları listelemek ve yeni kullanıcı oluşturmak
- Envanter ekranında kategori, lokasyon ve sarf malzeme kayıtlarını listelemek, oluşturmak, güncellemek ve `AktifMi` ile pasifleştirmek
- Cihazları listelemek, oluşturmak, güncellemek ve cihaz durum hareketi işlemek
- Sarf malzeme stok hareketi işlemek
- Basit stok özetini ve kritik stok listesini görmek
- Zimmet oluşturmak
- Zimmet iadesi almak
- İade kontrol sonucunu kaydetmek
- Personel kullanıcısı ile kendi zimmetlerini görmek

Notlar:

- Client tarafındaki listeleme hataları artık boş liste gibi gösterilmez; Türkçe hata mesajı olarak ekrana yansıtılır.
- Envanter veritabanında eski cihaz durum değerleri varsa `CihazDurumuEskiDegerleriniGuncelle` migration'ı uygulanmalıdır. Uygulama açılışında `Database.MigrateAsync()` çalıştığı için servis başlatıldığında bekleyen migration'lar otomatik uygulanır.

## 12. Güncel Envanter Client Notları - 2026-08-03

Envanter ekranında cihaz ve sarf malzeme yönetimi artık listeleme ve işlem sayfası olarak ayrılmıştır:

- Cihazlar sekmesinde cihazlar tablo halinde listelenir.
- Cihaz satırındaki `İşlemler` butonu `CihazIslemleri` sayfasına gider.
- Cihaz bilgisi güncelleme ve cihaz durum hareketi işleme bu sayfada yapılır.
- Sarf Malzemeler sekmesinde sarf malzemeler tablo halinde listelenir.
- Sarf malzeme satırındaki `İşlemler` butonu `SarfMalzemeIslemleri` sayfasına gider.
- Sarf malzeme bilgisi güncelleme ve sarf malzeme stok hareketi işleme bu sayfada yapılır.
- Sarf malzeme stok hareketi işlendiğinde aynı sayfada `Stok Hareketi Geçmişi` tablosunda görüntülenir.

Kategori ve lokasyon yönetimi mevcut tek sayfa akışını korur.

Ek cihaz yönetimi notları:

- Yeni cihaz oluştururken AssetTag girilmez; sistem otomatik `BT-...` numarası üretir.
- Cihazlar sekmesinde aktif/pasif, kategori ve lokasyon filtreleriyle liste daraltılabilir.
- Cihaz işlem sayfasında durum hareketi işlendiğinde aynı sayfada cihaz durum geçmişi görülebilir.
- Cihaz işlem sayfasında `AktifMi` ve `ToplamVarligaDahilMi` elle değiştirilemez; sistem cihaz durumu ve elden çıkarma tipine göre hesaplar.
- Cihaz işlem sayfasında `Durum`, çıkış tarihi ve elden çıkarma bilgileri salt okunurdur; durum değiştirmek için `Cihaz Durum Hareketi` formu kullanılır.
- Bakımdan dönen cihazı tekrar kullanılabilir yapmak için `Cihaz Durum Hareketi` formunda `BakimdanDondu` nedeni seçilir.
- Zimmet senaryoları için `Zimmetlendi` nedeni cihazı `Zimmetli`, `ZimmetIadeAlindi` nedeni cihazı `Incelemede` durumuna alır.
- Sarf malzeme stok hareketi formunda cihaz durumuna özel nedenler gösterilmez.
- `EnvantereGiris` cihaz durum hareketi formunda gösterilmez; yeni cihaz oluşturma akışına aittir.
- Manuel stok çıkışı, kaybolma, çalınma, kullanım dışı bırakma ve elden çıkarılmış hurda/ıskarta işlemlerinden sonra cihaz pasif ve toplam varlık dışı hale gelir.
- Boş AssetTag değerlerini dolduran `AssetTagBosCihazlariDoldur` migration'ı servis başlatıldığında bekleyen migration olarak otomatik uygulanır.
- Cihaz kapsam alanlarını düzelten `CihazKapsamAlanlariniDurumaGoreDuzelt` migration'ı servis başlatıldığında bekleyen migration olarak otomatik uygulanır.

## 13. Güncel ZimmetServisi Notları - 2026-08-08

ZimmetServisi Faz 5 kapsamında ayrı servis olarak eklenmiştir:

- Servis adresi `http://localhost:5002` olarak sabitlenmiştir.
- Zimmet verileri PostgreSQL içinde `zimmet` şemasında tutulur.
- Zimmet oluşturma sırasında aktif personel ve kullanılabilir cihaz kontrolü diğer servisler üzerinden yapılır.
- Zimmet oluşturulunca cihaz EnvanterServisi üzerinden `Zimmetli` yapılır.
- İade alınınca cihaz `Incelemede` olur.
- İade kontrolünde sonuç `Saglam`, `Bakimda`, `HurdaIskarta` veya `HasarliTeslimAlindi` olarak kaydedilir ve cihaz durumu buna göre güncellenir.
- Zimmet ve iade fotoğrafları bu fazda yoktur.
- CAP/RabbitMQ Faz 6'da eklenmiştir. ZimmetServisi'nin senkron HTTP doğrulama ve cihaz durum hareketi çağrıları korunur; başarılı işlemler ayrıca CAP Outbox üzerinden RabbitMQ eventleri üretir.

## 14. Faz 6 CAP/RabbitMQ Notları - 2026-08-10

Faz 6 ile event yayınlama altyapısı eklenmiştir:

- KimlikVePersonelServisi: `personel.isten-ayrildi`
- EnvanterServisi: `cihaz.durumu-degisti`, `stok.kritik-seviyeye-dusuldu`
- ZimmetServisi: `zimmet.olusturuldu`, `zimmet.iade-alindi`, `cihaz.kontrole-alindi`, `zimmet.iade-edildi`, `cihaz.hasarli-teslim-alindi`

CAP, outbox tablolarını servis başlatıldığında kendi şemaları altında oluşturur. DenetimKaydiServisi Faz 7'de eklendiği için audit amaçlı event tüketimi MongoDB'ye yapılır. Bildirim tüketimi Faz 9 kapsamındadır.

## 15. Faz 7 DenetimKaydiServisi Notları - 2026-08-11

Faz 7 ile DenetimKaydiServisi ve MongoDB eklenmiştir:

- MongoDB container adı `it-envanter-mongodb`, portu `27017` olarak ayarlanmıştır.
- DenetimKaydiServisi `http://localhost:5003` adresinde çalışır.
- Denetim API Swagger adresi `http://localhost:5003/swagger` şeklindedir.
- Servis CAP/RabbitMQ üzerinden domain eventlerini tüketir ve MongoDB'ye yazar.
- KimlikVePersonelServisi, EnvanterServisi ve ZimmetServisi başarılı CRUD/mutasyon işlemlerini best-effort HTTP çağrısıyla DenetimKaydiServisi'ne gönderir.
- DenetimServisi kapalıysa ana iş akışları başarısız sayılmaz; kaynak servis uyarı logu üretir.
