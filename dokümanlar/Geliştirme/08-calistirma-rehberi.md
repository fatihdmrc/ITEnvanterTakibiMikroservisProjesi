# Çalıştırma Rehberi

Bu doküman, IT Envanter Takip Mikroservis Projesi'ni geliştirme ortamında ilk kez kurmak, servisleri ayrı ayrı çalıştırmak, demo verisini bilinçli şekilde sıfırlamak ve temel adresleri kontrol etmek için kullanılır.

## 1. Gereksinimler

Bilgisayarda şu araçlar bulunmalıdır:

- .NET 8 SDK
- Docker Desktop
- PowerShell veya Windows Terminal
- İsteğe bağlı: DBeaver Community

Projede .NET 8 hedeflenmiştir. Komutlar PowerShell için hazırlanmıştır.

## 2. Proje Kök Dizini

Önce proje kök dizinine geç:

```powershell
cd "C:\Users\fathd\Desktop\ITEnvanterTakibiMikroservisProjesi"
```

Bu rehberdeki proje yolu bu klasöre göre verilmiştir.

## 3. Paketleri Geri Yükle ve Derle

İlk kurulumda veya paket değişikliği sonrası:

```powershell
dotnet restore ITEnvanterTakipSistemi.sln
```

Derleme kontrolü:

```powershell
dotnet build ITEnvanterTakipSistemi.sln --no-restore
```

Beklenen sonuç:

```text
Oluşturma başarılı oldu.
0 Uyarı
0 Hata
```

## 4. Altyapı Servislerini Başlat

PostgreSQL, RabbitMQ, MongoDB ve Redis Docker Compose ile çalışır.

```powershell
docker compose up -d postgres rabbitmq mongodb redis
```

MongoDB CAP storage için replica set olarak çalışır. İlk kurulumdan sonra şu komutu çalıştır:

```powershell
docker exec it-envanter-mongodb mongosh --eval "try { rs.status() } catch (e) { rs.initiate({_id:'rs0', members:[{_id:0, host:'127.0.0.1:27017'}]}) }"
```

Container kontrolü:

```powershell
docker ps
```

Docker Desktop'ta beklenen compose grubu:

```text
it-envanter
```

Beklenen containerlar:

```text
it-envanter-postgres
it-envanter-rabbitmq
it-envanter-mongodb
it-envanter-redis
```

## 5. Altyapı Bağlantı Bilgileri

PostgreSQL:

```text
Host: localhost
Port: 5432
Database: it_envanter_takip
User: itenvanter
Password: itenvanter123
```

RabbitMQ:

```text
AMQP: localhost:5672
Management UI: http://localhost:15672
User: guest
Password: guest
Exchange: inventory.events
```

MongoDB:

```text
Host: localhost
Port: 27017
Replica set: rs0
Kullanım: Denetim audit/event log ve CAP consumer storage
```

Redis:

```text
Host: localhost
Port: 6379
Kullanım: EnvanterServisi kategori/lokasyon cache
```

## 6. MailServisi Gmail Ayarları

MailServisi test amaçlı Gmail SMTP kullanır. Gmail kullanıcı adı ve app password repo içine yazılmaz; `user-secrets` ile local makinede saklanır.

Proje kök dizinindeyken:

```powershell
dotnet user-secrets set "Gmail:KullaniciAdi" "fathdmrc01@gmail.com" --project "src\servisler\MailServisi\MailServisi.Api\MailServisi.Api.csproj"
dotnet user-secrets set "Gmail:AppPassword" "GMAIL_APP_PASSWORD" --project "src\servisler\MailServisi\MailServisi.Api\MailServisi.Api.csproj"
```

MailServisi klasörünün içindeyken `--project` yazmadan da çalıştırabilirsin:

```powershell
cd "C:\Users\fathd\Desktop\ITEnvanterTakibiMikroservisProjesi\src\servisler\MailServisi\MailServisi.Api"
dotnet user-secrets set "Gmail:KullaniciAdi" "fathdmrc01@gmail.com"
dotnet user-secrets set "Gmail:AppPassword" "GMAIL_APP_PASSWORD"
```

Kayıtlı değerleri görmek için:

```powershell
dotnet user-secrets list --project "src\servisler\MailServisi\MailServisi.Api\MailServisi.Api.csproj"
```

Notlar:

- `GMAIL_APP_PASSWORD` normal Gmail şifresi değildir; Gmail uygulama şifresidir.
- Bu ayarlar bir kez girildikten sonra bilgisayar kapanıp açılsa da kalır.
- Aynı key tekrar set edilirse eski değerin üzerine yazılır.
- Test modu açık olduğu için mailler gerçek personele gitmez; alıcı `fathdmrc01@gmail.com` olarak override edilir.

## 7. Veritabanı Migration

Servisler başlarken kendi migration'larını otomatik uygular. Yine de ilk kurulumda manuel uygulamak istersen PostgreSQL çalışırken şu komutları proje kökünden çalıştırabilirsin.

KimlikVePersonelServisi:

```powershell
dotnet ef database update --project "src\servisler\KimlikVePersonelServisi\KimlikVePersonelServisi.Api\KimlikVePersonelServisi.Api.csproj" --startup-project "src\servisler\KimlikVePersonelServisi\KimlikVePersonelServisi.Api\KimlikVePersonelServisi.Api.csproj" --context KimlikPersonelDbContext
```

EnvanterServisi:

```powershell
dotnet ef database update --project "src\servisler\EnvanterServisi\EnvanterServisi.Api\EnvanterServisi.Api.csproj" --startup-project "src\servisler\EnvanterServisi\EnvanterServisi.Api\EnvanterServisi.Api.csproj" --context EnvanterDbContext
```

ZimmetServisi:

```powershell
dotnet ef database update --project "src\servisler\ZimmetServisi\ZimmetServisi.Api\ZimmetServisi.Api.csproj" --startup-project "src\servisler\ZimmetServisi\ZimmetServisi.Api\ZimmetServisi.Api.csproj" --context ZimmetDbContext
```

DenetimKaydiServisi, BildirimServisi ve MailServisi PostgreSQL migration kullanmaz. Bu servisler CAP consumer state ve audit/event kayıtları için MongoDB kullanır.

## 8. Demo Verisini Sıfırlayıp Yeniden Kurma

Normal development çalıştırmasında demo reset kapalıdır:

```text
DemoVeri:Sifirla=false
```

Bu sayede servisleri başlatmak mevcut test verilerini silmez.

Demo verisini bilinçli olarak sıfırlayıp yeniden kurmak için Kimlik, Envanter ve Zimmet servislerini başlatmadan önce aynı PowerShell terminalinde şu environment override değerini ver:

```powershell
$env:DemoVeri__Sifirla="true"
```

Sonra seed uygulayacak servisleri sırayla başlat:

```powershell
dotnet run --project "src\servisler\KimlikVePersonelServisi\KimlikVePersonelServisi.Api\KimlikVePersonelServisi.Api.csproj" --launch-profile http
```

Kimlik servisi açıldıktan sonra `Ctrl + C` ile kapatıp EnvanterServisi'ni başlat:

```powershell
dotnet run --project "src\servisler\EnvanterServisi\EnvanterServisi.Api\EnvanterServisi.Api.csproj" --launch-profile http
```

Envanter servisi açıldıktan sonra `Ctrl + C` ile kapatıp ZimmetServisi'ni başlat:

```powershell
dotnet run --project "src\servisler\ZimmetServisi\ZimmetServisi.Api\ZimmetServisi.Api.csproj" --launch-profile http
```

Reset işlemi bittiğinde aynı terminalde override değerini temizle:

```powershell
Remove-Item Env:\DemoVeri__Sifirla
```

Sonra servisleri normal şekilde ayrı terminallerde çalıştır.

Önemli:

- `DemoVeri__Sifirla=true` açıkken servis başlatmak ilgili servis domain verisini sıfırlar.
- Bu değeri normal günlük testlerde açık bırakma.
- Denetim/Mongo kayıtları ve CAP teknik kayıtları domain demo seed resetinden ayrı kabul edilir.

## 9. Servisleri Ayrı Ayrı Çalıştırma

Her uygulamayı ayrı PowerShell terminalinde çalıştırmak en rahat yöntemdir. Önce her terminalde proje kök dizinine geç:

```powershell
cd "C:\Users\fathd\Desktop\ITEnvanterTakibiMikroservisProjesi"
```

### Terminal 1 - KimlikVePersonelServisi

```powershell
dotnet run --project "src\servisler\KimlikVePersonelServisi\KimlikVePersonelServisi.Api\KimlikVePersonelServisi.Api.csproj" --launch-profile http
```

Alternatif olarak proje klasörünün içindeysen:

```powershell
cd "C:\Users\fathd\Desktop\ITEnvanterTakibiMikroservisProjesi\src\servisler\KimlikVePersonelServisi\KimlikVePersonelServisi.Api"
dotnet run --launch-profile http
```

Adresler:

```text
Swagger: http://localhost:5000/swagger
Sağlık:  http://localhost:5000/saglik
```

Temel endpointler:

```text
POST /api/kimlik/giris
GET  /api/departmanlar                         Admin
POST /api/departmanlar                         Admin
GET  /api/personeller                          Admin
GET  /api/personeller/zimmet-secimi            Admin, ITPersoneli
GET  /api/personeller/{id}/zimmet-dogrulama    Admin, ITPersoneli
POST /api/personeller                          Admin
POST /api/kullanicilar                         Admin
```

### Terminal 2 - EnvanterServisi

```powershell
dotnet run --project "src\servisler\EnvanterServisi\EnvanterServisi.Api\EnvanterServisi.Api.csproj" --launch-profile http
```

Proje klasörünün içindeysen:

```powershell
cd "C:\Users\fathd\Desktop\ITEnvanterTakibiMikroservisProjesi\src\servisler\EnvanterServisi\EnvanterServisi.Api"
dotnet run --launch-profile http
```

Adresler:

```text
Swagger: http://localhost:5001/swagger
Sağlık:  http://localhost:5001/saglik
```

Temel endpointler:

```text
GET  /api/kategoriler
GET  /api/lokasyonlar
GET  /api/cihazlar
POST /api/cihazlar
POST /api/cihazlar/{id}/durum-hareketleri
GET  /api/sarf-malzemeler
POST /api/sarf-malzemeler/{id}/stok-hareketleri
GET  /api/stok/ozet
GET  /api/stok/hareketler
```

### Terminal 3 - ZimmetServisi

```powershell
dotnet run --project "src\servisler\ZimmetServisi\ZimmetServisi.Api\ZimmetServisi.Api.csproj" --launch-profile http
```

Proje klasörünün içindeysen:

```powershell
cd "C:\Users\fathd\Desktop\ITEnvanterTakibiMikroservisProjesi\src\servisler\ZimmetServisi\ZimmetServisi.Api"
dotnet run --launch-profile http
```

Adresler:

```text
Swagger: http://localhost:5002/swagger
Sağlık:  http://localhost:5002/saglik
```

Temel endpointler:

```text
GET  /api/zimmetler
GET  /api/zimmetler/benim
GET  /api/zimmetler/{id}
POST /api/zimmetler
POST /api/zimmetler/{id}/iade-alindi
POST /api/zimmetler/{id}/iade-kontrolu
```

ZimmetServisi çalışırken KimlikVePersonelServisi ve EnvanterServisi de açık olmalıdır. Çünkü personel uygunluğu ve cihaz durum değişikliği bu servisler üzerinden doğrulanır.

### Terminal 4 - DenetimKaydiServisi

```powershell
dotnet run --project "src\servisler\DenetimKaydiServisi\DenetimKaydiServisi.Api\DenetimKaydiServisi.Api.csproj" --launch-profile http
```

Proje klasörünün içindeysen:

```powershell
cd "C:\Users\fathd\Desktop\ITEnvanterTakibiMikroservisProjesi\src\servisler\DenetimKaydiServisi\DenetimKaydiServisi.Api"
dotnet run --launch-profile http
```

Adresler:

```text
Swagger: http://localhost:5003/swagger
Sağlık:  http://localhost:5003/saglik
```

Temel endpointler:

```text
GET  /api/denetim-kayitlari
GET  /api/denetim-kayitlari/{id}
POST /api/denetim-kayitlari/crud
```

### Terminal 5 - BildirimServisi

```powershell
dotnet run --project "src\servisler\BildirimServisi\BildirimServisi.Api\BildirimServisi.Api.csproj" --launch-profile http
```

Proje klasörünün içindeysen:

```powershell
cd "C:\Users\fathd\Desktop\ITEnvanterTakibiMikroservisProjesi\src\servisler\BildirimServisi\BildirimServisi.Api"
dotnet run --launch-profile http
```

Adresler:

```text
Swagger: http://localhost:5004/swagger
Sağlık:  http://localhost:5004/saglik
Hub:     http://localhost:5004/hubs/bildirim
```

BildirimServisi `stok.kritik-seviyeye-dusuldu` eventini tüketir ve Admin/IT kullanıcılarına canlı SignalR bildirimi gönderir.

### Terminal 6 - MailServisi

```powershell
dotnet run --project "src\servisler\MailServisi\MailServisi.Api\MailServisi.Api.csproj" --launch-profile http
```

Proje klasörünün içindeysen:

```powershell
cd "C:\Users\fathd\Desktop\ITEnvanterTakibiMikroservisProjesi\src\servisler\MailServisi\MailServisi.Api"
dotnet run --launch-profile http
```

Adresler:

```text
Swagger: http://localhost:5006/swagger
Sağlık:  http://localhost:5006/saglik
```

MailServisi `zimmet.olusturuldu`, `zimmet.iade-alindi` ve `zimmet.iade-edildi` eventlerini tüketir. Gmail secret değerleri girilmiş olmalıdır.

### Terminal 7 - MVC Client

```powershell
dotnet run --project "src\istemci\EnvanterTakip.MvcClient\EnvanterTakip.MvcClient.csproj" --launch-profile http
```

Proje klasörünün içindeysen:

```powershell
cd "C:\Users\fathd\Desktop\ITEnvanterTakibiMikroservisProjesi\src\istemci\EnvanterTakip.MvcClient"
dotnet run --launch-profile http
```

Tarayıcı adresi:

```text
http://localhost:5010
```

MVC client şu servislere doğrudan bağlanır:

```text
KimlikVePersonelServisi: http://localhost:5000
EnvanterServisi:        http://localhost:5001
ZimmetServisi:          http://localhost:5002
DenetimKaydiServisi:    http://localhost:5003
BildirimServisi:        http://localhost:5004
```

MailServisi MVC tarafından doğrudan çağrılmaz; CAP/RabbitMQ event consumer olarak çalışır.

## 10. dotnet watch ile Çalıştırma

Geliştirme sırasında ilgili proje klasörünün içine girip `dotnet watch` kullanabilirsin.

Örnek MailServisi:

```powershell
cd "C:\Users\fathd\Desktop\ITEnvanterTakibiMikroservisProjesi\src\servisler\MailServisi\MailServisi.Api"
dotnet watch
```

Örnek MVC Client:

```powershell
cd "C:\Users\fathd\Desktop\ITEnvanterTakibiMikroservisProjesi\src\istemci\EnvanterTakip.MvcClient"
dotnet watch
```

`dotnet watch` aynı `Development` ayarlarını ve proje için kayıtlı user-secrets değerlerini kullanır.

## 11. Demo Kullanıcılar

KimlikVePersonelServisi seed sonrası şu kullanıcılar kullanılabilir:

```text
admin / Admin123! / Admin
it.personel / ItPersonel123! / ITPersoneli
personel / Personel123! / PersonelKullanicisi
```

Yetki özeti:

```text
Admin: Departman, personel, kullanıcı, envanter, zimmet ve denetim işlemleri
ITPersoneli: Envanter, zimmet ve denetim işlemleri
PersonelKullanicisi: Kendi zimmetlerini görüntüleme
```

ITPersoneli rolü departman, personel ve kullanıcı yönetim ekranına erişemez. Zimmet oluşturma ekranında gereken personel seçimi ayrı salt-okunur endpoint üzerinden yapılır.

Swagger'da token alma:

1. `http://localhost:5000/swagger` adresine git.
2. `POST /api/kimlik/giris` endpointini aç.
3. Demo kullanıcı bilgilerinden biriyle giriş yap.
4. Dönen `token` değerini kopyala.
5. Diğer servislerin Swagger ekranında `Authorize` butonuna bas.
6. Token değerini yapıştır.

Postman veya curl için header formatı:

```text
Authorization: Bearer JWT_TOKEN_DEGERI
```

## 12. Önerilen Tam Çalıştırma Sırası

Temiz bir test için pratik sıra:

1. Docker Desktop'ı aç.
2. Proje köküne geç.
3. Altyapıyı başlat:

```powershell
docker compose up -d postgres rabbitmq mongodb redis
```

4. MongoDB replica set kontrolünü çalıştır:

```powershell
docker exec it-envanter-mongodb mongosh --eval "try { rs.status() } catch (e) { rs.initiate({_id:'rs0', members:[{_id:0, host:'127.0.0.1:27017'}]}) }"
```

5. Gerekliyse Gmail user-secrets değerlerini gir.
6. Gerekliyse demo reset için `$env:DemoVeri__Sifirla="true"` verip Kimlik, Envanter ve Zimmet servislerini sırayla birer kez başlat.
7. Reset kullandıysan `Remove-Item Env:\DemoVeri__Sifirla` ile temizle.
8. Ayrı terminallerde şu sırayla servisleri açık bırak:

```text
KimlikVePersonelServisi  -> http://localhost:5000
EnvanterServisi          -> http://localhost:5001
ZimmetServisi            -> http://localhost:5002
DenetimKaydiServisi      -> http://localhost:5003
BildirimServisi          -> http://localhost:5004
MailServisi              -> http://localhost:5006
MVC Client               -> http://localhost:5010
```

9. Tarayıcıdan MVC client'a git:

```text
http://localhost:5010
```

## 13. DBeaver ile PostgreSQL Kontrolü

DBeaver bağlantı bilgileri:

```text
Database Type: PostgreSQL
Host: localhost
Port: 5432
Database: it_envanter_takip
Username: itenvanter
Password: itenvanter123
```

Şemalar:

```text
kimlik_personel
envanter
zimmet
cap_kimlik
cap_envanter
cap_zimmet
```

Envanter tabloları:

```text
Kategoriler
Lokasyonlar
Cihazlar
SarfMalzemeler
StokHareketleri
KritikStokKurallari
```

Zimmet tabloları:

```text
Zimmetler
```

## 14. RabbitMQ Yönetim Paneli

Adres:

```text
http://localhost:15672
```

Giriş:

```text
User: guest
Password: guest
```

Kontrol edilebilecekler:

```text
Exchange: inventory.events
Consumer grupları:
- denetim-kaydi-servisi
- bildirim-servisi
- mail-servisi
```

## 15. Çalışan Kapsam

Şu an projede aktif çalışan ana parçalar:

- KimlikVePersonelServisi
- EnvanterServisi
- ZimmetServisi
- DenetimKaydiServisi
- BildirimServisi
- MailServisi
- MVC Client
- PostgreSQL
- RabbitMQ
- MongoDB
- Redis
- CAP/RabbitMQ event altyapısı
- MongoDB audit kayıtları
- Redis referans veri cache
- SignalR canlı kritik stok bildirimi
- Gmail test mail consumer akışı

Sıradaki ana faz:

```text
Faz 10 - ApiGateway Entegrasyonu
```
