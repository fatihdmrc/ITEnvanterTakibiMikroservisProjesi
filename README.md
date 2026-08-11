# IT Envanter Takibi Mikroservis Projesi

IT envanter, personel, cihaz, sarf malzeme ve zimmet süreçlerini yönetmek için geliştirilen .NET 8 tabanlı mikroservis projesidir. Proje; kimlik/personel yönetimi, envanter yönetimi, zimmet oluşturma, zimmet iade kontrolü ve RabbitMQ üzerinden event yayınlama altyapısını içerir.

Bu README, projeye ilk giriş ve hızlı çalıştırma rehberidir. Ayrıntılı analiz, mimari kararlar ve geliştirme notları için [Dokümantasyon](#dokümantasyon) bölümündeki dosyalara bakılmalıdır.

## Güncel Durum

Faz 8'e kadar olan temel geliştirmeler uygulanmıştır:

- Kimlik ve personel yönetimi
- Envanter yönetimi
- MVC client kontrol paneli
- Cihaz ve sarf malzeme işlem ekranları
- ZimmetServisi ve zimmet yönetimi
- DotNetCore.CAP + RabbitMQ + Outbox event yayınlama altyapısı
- DenetimKaydiServisi, MongoDB audit/event log ve MVC Denetim ekranı
- Redis ile EnvanterServisi kategori/lokasyon referans veri cache'i

Sıradaki fazlar:

- Faz 9: SignalR Bildirimleri
- Faz 10: ApiGateway Entegrasyonu
- Faz 11: Demo ve Dokümantasyon

## Kullanılan Teknolojiler

| Alan | Teknoloji |
| --- | --- |
| Backend | .NET 8, ASP.NET Core Web API |
| Client | ASP.NET Core MVC |
| Veritabanı | PostgreSQL |
| Audit log | MongoDB |
| ORM | Entity Framework Core |
| Kimlik | ASP.NET Core Identity, JWT |
| Event altyapısı | DotNetCore.CAP, RabbitMQ, Outbox Pattern |
| Cache | Redis |
| Container | Docker Compose |
| API dokümantasyonu | Swagger / OpenAPI |

## Mimari Özet

| Bileşen | Port | Sorumluluk |
| --- | --- | --- |
| KimlikVePersonelServisi | `5000` | Kullanıcı, rol, departman ve personel yönetimi |
| EnvanterServisi | `5001` | Cihaz, sarf malzeme, kategori, lokasyon ve stok işlemleri |
| ZimmetServisi | `5002` | Zimmet oluşturma, iade alma ve iade kontrolü |
| DenetimKaydiServisi | `5003` | Event ve CRUD audit kayıtları |
| MVC Client | `5010` | Yönetim arayüzü |
| PostgreSQL | `5432` | Operasyonel veri depolama |
| RabbitMQ | `5672` | Event taşıyıcı |
| RabbitMQ Management UI | `15672` | RabbitMQ yönetim paneli |
| MongoDB | `27017` | Denetim kaydı depolama |
| Redis | `6379` | Referans veri cache |

Servisler arası anlık doğrulamalar HTTP ile yapılır. Başarılı domain işlemleri DotNetCore.CAP Outbox üzerinden RabbitMQ'ya event olarak yayınlanır.

## Hızlı Başlatma

### Gereksinimler

- .NET 8 SDK
- Docker Desktop
- PowerShell veya Windows Terminal
- PostgreSQL ve RabbitMQ için Docker Compose

### 1. Proje Klasörüne Geç

```powershell
cd "C:\Users\fathd\Desktop\ITEnvanterTakibiMikroservisProjesi"
```

### 2. PostgreSQL ve RabbitMQ'yu Başlat

```powershell
docker compose up -d postgres rabbitmq mongodb redis
```

Durumu görüntülemek için:

```powershell
docker compose ps
```

### 3. Projeyi Derle

```powershell
dotnet restore ITEnvanterTakipSistemi.sln
dotnet build ITEnvanterTakipSistemi.sln --no-restore
```

### 4. Servisleri Başlat

Her komutu ayrı terminal penceresinde çalıştır.

KimlikVePersonelServisi:

```powershell
dotnet run --project "src\servisler\KimlikVePersonelServisi\KimlikVePersonelServisi.Api\KimlikVePersonelServisi.Api.csproj" --launch-profile http
```

EnvanterServisi:

```powershell
dotnet run --project "src\servisler\EnvanterServisi\EnvanterServisi.Api\EnvanterServisi.Api.csproj" --launch-profile http
```

ZimmetServisi:

```powershell
dotnet run --project "src\servisler\ZimmetServisi\ZimmetServisi.Api\ZimmetServisi.Api.csproj" --launch-profile http
```

DenetimKaydiServisi:

```powershell
dotnet run --project "src\servisler\DenetimKaydiServisi\DenetimKaydiServisi.Api\DenetimKaydiServisi.Api.csproj" --launch-profile http
```

MVC Client:

```powershell
dotnet run --project "src\istemci\EnvanterTakip.MvcClient\EnvanterTakip.MvcClient.csproj" --launch-profile http
```

## Adresler

| Uygulama | Adres |
| --- | --- |
| Kimlik API Swagger | http://localhost:5000/swagger |
| Envanter API Swagger | http://localhost:5001/swagger |
| Zimmet API Swagger | http://localhost:5002/swagger |
| Denetim API Swagger | http://localhost:5003/swagger |
| MVC Client | http://localhost:5010 |
| RabbitMQ Yönetim Paneli | http://localhost:15672 |

RabbitMQ yönetim paneli:

```text
Kullanıcı adı: guest
Şifre: guest
```

Demo kullanıcılar:

| Kullanıcı adı | Şifre | Rol |
| --- | --- | --- |
| `admin` | `Admin123!` | Admin |
| `it.personel` | `ItPersonel123!` | ITPersoneli |
| `personel` | `Personel123!` | PersonelKullanicisi |

## Ekran Görüntüleri

Ekran görüntüleri repo içinde `dokümanlar/görseller/ekranlar/` klasöründe tutulacaktır. Görseller eklendiğinde aşağıdaki yollar README içinde doğrudan çalışır.

| Ekran | Görsel |
| --- | --- |
| Giriş ekranı | `dokümanlar/görseller/ekranlar/giris-ekrani.png` |
| Personel yönetimi | `dokümanlar/görseller/ekranlar/personel-yonetimi.png` |
| Envanter cihazlar | `dokümanlar/görseller/ekranlar/envanter-cihazlar.png` |
| Cihaz işlemleri | `dokümanlar/görseller/ekranlar/cihaz-islemleri.png` |
| Sarf malzeme işlemleri | `dokümanlar/görseller/ekranlar/sarf-malzeme-islemleri.png` |
| Zimmetler | `dokümanlar/görseller/ekranlar/zimmetler.png` |
| Zimmet iade kontrolü | `dokümanlar/görseller/ekranlar/zimmet-iade-kontrolu.png` |
| RabbitMQ paneli | `dokümanlar/görseller/ekranlar/rabbitmq-paneli.png` |

## Temel Kullanım Akışları

- MVC client üzerinden demo kullanıcıyla giriş yapılır.
- Departman ve personel kayıtları yönetilir.
- Cihaz, kategori, lokasyon ve sarf malzeme kayıtları yönetilir.
- Cihazlar için durum hareketi işlenir; aktiflik ve toplam varlık kapsamı sistem tarafından hesaplanır.
- Sarf malzemelerde miktar bazlı stok hareketi işlenir.
- Uygun durumdaki cihaz aktif personele zimmetlenir.
- Zimmet iade alındığında cihaz fiziki kontrol için `Incelemede` durumuna geçer.
- İade kontrolü sonucuna göre cihaz `Kullanilabilir`, `Bakimda`, `HurdaIskarta` veya `HasarliTeslimAlindi` durumuna alınır.
- Kritik stok ve domain olayları CAP Outbox üzerinden RabbitMQ'ya yayınlanır.
- DenetimKaydiServisi RabbitMQ eventlerini ve başarılı CRUD/mutasyon işlemlerini MongoDB'ye kaydeder.
- MVC Denetim ekranında Admin/IT kullanıcıları audit kayıtlarını filtreleyip detay payload'ını görebilir.
- EnvanterServisi kategori ve lokasyon listelerini Redis ile cache'ler; kayıt değişikliklerinde ilgili cache temizlenir.

## Dokümantasyon

| Doküman | Açıklama |
| --- | --- |
| [Gereksinim Analizi](dokümanlar/01-gereksinim-analizi.md) | Projenin amaç, kapsam ve gereksinimleri |
| [Mimari Tasarım](dokümanlar/02-mimari-tasarim.md) | Mikroservis mimarisi ve teknoloji kararları |
| [Veri Modeli](dokümanlar/03-veri-modeli.md) | Entity, tablo ve veri modeli notları |
| [Servis İletişimleri](dokümanlar/04-servis-iletisimleri.md) | HTTP ve CAP/RabbitMQ iletişimleri |
| [İş Akışları](dokümanlar/05-is-akislari.md) | Zimmet, stok, personel ayrılma ve cihaz akışları |
| [Geliştirme Planı](dokümanlar/Geliştirme/06-gelistirme-plani.md) | Faz bazlı geliştirme planı |
| [Geliştirme Günlüğü](dokümanlar/Geliştirme/07-gelistirme-gunlugu.md) | Yapılan değişikliklerin tarihçesi |
| [Çalıştırma Rehberi](dokümanlar/Geliştirme/08-calistirma-rehberi.md) | Ayrıntılı kurulum ve çalıştırma adımları |
| [Kod Yapısı Açıklamaları](dokümanlar/Geliştirme/09-kod-yapisi-aciklamalari.md) | Kod organizasyonu ve teknik notlar |
| [Proje Kararları](PROJE_KARARLARI.md) | Mimari ve ürün kararlarının toplu kaydı |

## Lisans

Bu proje [MIT lisansı](LICENSE) ile lisanslanmıştır.
