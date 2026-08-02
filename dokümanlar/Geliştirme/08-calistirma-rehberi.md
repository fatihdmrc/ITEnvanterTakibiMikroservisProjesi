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

## 5. PostgreSQL'i Docker ile Başlat

```powershell
docker compose up -d postgres
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

## 7. Migration Uygula

PostgreSQL çalıştıktan sonra KimlikVePersonelServisi migration'ını uygula:

```powershell
dotnet ef database update --project "src\servisler\KimlikVePersonelServisi\KimlikVePersonelServisi.Api\KimlikVePersonelServisi.Api.csproj" --startup-project "src\servisler\KimlikVePersonelServisi\KimlikVePersonelServisi.Api\KimlikVePersonelServisi.Api.csproj" --context KimlikPersonelDbContext
```

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

### Terminal 3 - MVC Client

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

Not: MVC client şu an doğrudan `http://localhost:5000` adresindeki KimlikVePersonelServisi'ne bağlanır. ApiGateway eklendiğinde bu adres `appsettings.json` içinden değiştirilecektir.

Yetki notu:

- Giriş yapılmadan departman, personel ve kullanıcı endpointleri `401 Unauthorized` döner.
- `Admin` rolü departman, personel ve kullanıcı yönetimi yapabilir.
- `ITPersoneli` rolü departman ve personel yönetimi yapabilir.
- `PersonelKullanicisi` bu yönetim endpointlerini kullanamaz; ileride yalnızca kendi zimmet süreçlerini görebilecektir.

## 9. Şu Anki Durum

Şu an çalışan kapsam:

- Solution iskeleti
- KimlikVePersonelServisi API projesi
- EnvanterServisi API projesi
- ASP.NET Core MVC client projesi
- PostgreSQL Docker Compose 
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

Henüz eklenmeyenler:

- ApiGateway
- CAP + RabbitMQ
- MongoDB audit log
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
- Envanter ekranında kategori, lokasyon, cihaz ve sarf malzeme kayıtlarını listelemek, oluşturmak, güncellemek ve `AktifMi` ile pasifleştirmek
- Cihaz ve sarf malzeme stok hareketi işlemek
- Basit stok özetini ve kritik stok listesini görmek

Notlar:

- Client tarafındaki listeleme hataları artık boş liste gibi gösterilmez; Türkçe hata mesajı olarak ekrana yansıtılır.
- Envanter veritabanında eski cihaz durum değerleri varsa `CihazDurumuEskiDegerleriniGuncelle` migration'ı uygulanmalıdır. Uygulama açılışında `Database.MigrateAsync()` çalıştığı için servis başlatıldığında bekleyen migration'lar otomatik uygulanır.
