# IT Ekipman Takip Sistemi - Mimari ve Analiz Kararları

Bu doküman, SDLC'nin ilk iki aşaması olan gereksinim analizi ve sistem tasarımı için yaşayan karar kaydı olarak tutulur. Projede yeni karar alındıkça veya mevcut karar değiştikçe bu dosya güncellenmelidir.

## 1. Proje Amacı

Sistem, IT ekipmanlarının personele zimmetlenmesi, iade edilmesi, durumlarının takip edilmesi, stok hareketlerinin izlenmesi ve kritik stok seviyelerinde bildirim üretilmesi amacıyla tasarlanacaktır.

Staj defteri konu başlığı:

`.NET Core ile Mikroservis Tabanlı Envanter ve Zimmet Takip Backend Servisi Tasarımı`

## 2. Mimari Kararlar

- Sistem mikroservis mimarisiyle tasarlanacaktır.
- Api Gateway kullanılacaktır.
- Api Gateway teknolojisi olarak YARP tercih edilmiştir.
- Client istekleri doğrudan mikroservislere değil, YARP tabanlı Api Gateway üzerinden ilgili servislere yönlendirilecektir.
- Her mikroservis kendi portunda çalışacak ve kendi Swagger arayüzüne sahip olacaktır.
- Servisler arası zorunlu doğrulamalar HTTP ile yapılacaktır.
- Asenkron olay akışları DotNetCore.CAP üzerinden RabbitMQ ile yürütülecektir.
- Event yayınlama güvenilirliği için Outbox Pattern uygulanacaktır.
- Eventler arasında gerekli görülen durumlarda kullanıcı bağlamı taşınacaktır.
- Authentication ve authorization işlemleri olacaktır.
- Güvenlik yaklaşımı JWT + rol bazlı yetkilendirme olarak planlanmıştır.
- Test yazımı bu kapsamda yapılmayacaktır.
- Öncelik çalışan koddan önce mimari plan, gereksinim analizi ve sistem tasarımıdır.
- SDLC analiz ve tasarım belgeleri Markdown formatında hazırlanacaktır.
- Projedeki görsel diyagramların Mermaid kodları Mermaid.ai üzerinde görsel tasarıma dönüştürülecektir.

## 3. Dokümantasyon ve Diyagram Araçları

- Ana dokümanlar Markdown formatında tutulacaktır.
- Doküman yazımı için VS Code kullanılacaktır.
- Diyagram kaynak kodları `.mmd` uzantılı Mermaid dosyaları olarak `dokümanlar/diyagram kodları` altındaki kategori klasörlerinde tutulacaktır.
- Görsel diyagramların oluşturulması ve düzenlenmesi için Mermaid.ai kullanılacaktır.
- Mermaid.ai üzerinde oluşturulan görseller staj raporu veya sunumda kullanılabilir.
- Markdown dokümanları karar ve açıklama kaynağı, Mermaid diyagramları ise görsel tasarım kaynağı olarak kabul edilecektir.
- Mevcut diyagram dosyaları:
  - `dokümanlar/diyagram kodları/Mimari/servis-mimari-tasarim.mmd`
  - `dokümanlar/diyagram kodları/Veri Modeli/veri-modeli-servis-bazli.mmd`
  - `dokümanlar/diyagram kodları/Kullanım Senaryoları/use-case-admin.mmd`
  - `dokümanlar/diyagram kodları/Kullanım Senaryoları/use-case-it-personeli.mmd`
  - `dokümanlar/diyagram kodları/Kullanım Senaryoları/use-case-personel-kullanicisi.mmd`
  - `dokümanlar/diyagram kodları/İş Akışları/is-akislari-genel.mmd`
  - `dokümanlar/diyagram kodları/İş Akışları/is-akisi-zimmet-olusturma.mmd`
  - `dokümanlar/diyagram kodları/İş Akışları/is-akisi-zimmet-iade.mmd`
  - `dokümanlar/diyagram kodları/İş Akışları/is-akisi-personel-isten-ayrilma.mmd`
  - `dokümanlar/diyagram kodları/İş Akışları/is-akisi-stok-cikisi-kritik-stok.mmd`
  - `dokümanlar/diyagram kodları/İş Akışları/is-akisi-hurda-iskarta.mmd`

## 4. İsimlendirme Kararları

- Mimari kararlarda, servis adlarında, entity adlarında, DTO adlarında ve kod içi domain kavramlarında Türkçe isimlendirme kullanılacaktır.
- C# değişken, sınıf, metot ve namespace isimlerinde Türkçe karakter kullanılmayacaktır; bunun yerine Türkçe kelimelerin ASCII karşılıkları tercih edilecektir.
- Kod tarafında PascalCase ve camelCase kurallarına uyulacaktır.
- Veritabanı tablo/kolon isimleri de Türkçe domain kavramlarından türetilecek, ancak teknik uyumluluk için Türkçe karakter içermeyecektir.
- Örnek isimlendirme:
  - Servis: `ZimmetServisi`, `EnvanterServisi`, `KimlikVePersonelServisi`
  - Entity: `Cihaz`, `SarfMalzeme`, `Zimmet`, `Kullanici`
  - DTO: `CihazOlusturDto`, `ZimmetIadeDto`, `StokHareketiDto`
  - Metot: `ZimmetOlustur`, `CihazDurumuGuncelle`, `KullanilabilirStokHesapla`
  - Değişken: `cihazId`, `personelId`, `zimmetTarihi`
  - Rol: `Admin`, `ITPersoneli`, `PersonelKullanicisi`

## 5. Planlanan Mikroservisler

### ApiGateway

- Client uygulamadan gelen istekler için tek giriş noktasıdır.
- YARP kullanılarak geliştirilecektir.
- Route bazlı yönlendirme yapar.
- Gerekli endpointlerde JWT doğrulama ve rol bazlı yetki politikalarını uygulayabilir.
- İstekleri KimlikVePersonelServisi, EnvanterServisi, ZimmetServisi, DenetimKaydiServisi ve BildirimServisi'ne yönlendirir.
- Servisler arası iç HTTP iletişimin yerine geçmez; yalnızca client-server trafiğini merkezileştirir.

### KimlikVePersonelServisi

- Kullanıcı girişi yapar.
- JWT token üretir.
- Kullanıcı ve rol bilgilerini yönetir.
- Diğer servislerin kullanacağı authentication/authorization yapısının merkezidir.
- Personel bilgilerini yönetir.
- Departman bilgilerini yönetir.
- PostgreSQL + EF Core kullanır.
- ZimmetServisi tarafından personel doğrulama amacıyla HTTP üzerinden çağrılır.
- Personelin aktif, pasif ve işten ayrılmış durumlarını yönetir.
- İşten ayrılan personelin kullanıcı hesabını aynı işlem kapsamında pasifleştirir.

### EnvanterServisi

- IT ekipmanlarını, cihaz kategorilerini, lokasyonları ve stok durumlarını yönetir.
- PostgreSQL + EF Core kullanır.
- Kategori ve lokasyon listelerini Redis ile cache'ler.
- Manuel stok çıkışı, arıza, çalınma-kaybolma ve hurda-iskarta işlemlerinde stok durumunu günceller.
- Kritik stok seviyesine düşüldüğünde DotNetCore.CAP üzerinden RabbitMQ eventi yayınlar.

### ZimmetServisi

- Zimmet oluşturma, zimmet iade süreci ve zimmet geçmişini yönetir.
- Bir personele birden fazla cihaz zimmetlenmesine izin verir.
- Bir cihaz aynı anda yalnızca bir personele zimmetlenebilir.
- Departmanda ortak kullanılan cihazlar departman sorumlusu adına zimmetlenir.
- KimlikVePersonelServisi ve EnvanterServisi ile işlem anı doğrulama için senkron HTTP üzerinden konuşur.
- Zimmet oluşturma ve iade olaylarını Faz 6 itibarıyla DotNetCore.CAP Outbox üzerinden RabbitMQ'ya yayınlar.

### DenetimKaydiServisi

- DotNetCore.CAP ile RabbitMQ eventlerini tüketir.
- MongoDB üzerinde JSON tabanlı audit/event log kaydı tutar.
- Zimmet geçmişi silinmeyecektir.

### BildirimServisi

- DotNetCore.CAP ile RabbitMQ üzerinden kritik stok eventlerini tüketir.
- SignalR ile yalnızca kritik stok seviyesi altına düşme bildirimlerini yayınlar.
- Kritik stok bildirim paneli ile demo edilebilir.

## 6. Ekipman ve Stok Kararları

- Proje cihaz odaklı olacaktır, ancak varlıklar iki ana tipe ayrılacaktır:
  - Seri numarası ile takip edilen tekil varlıklar
  - Sarf malzemeleri
- Her ekipman seri numarası ile takip edilmeyecektir.
- Seri numarası ile takip edilen varlıklarda seri numarası cihaz bazında benzersiz kabul edilmelidir.
- Sarf malzemeleri tekil cihaz gibi değil, adet bazlı stok olarak takip edilecektir.
- Seri numarası olmayan ancak tekil takip edilmesi gereken varlıklar için sistem içinde `AssetTag` veya demirbaş numarası kullanılabilir.
- Tekil takip edilen varlıklarda `SeriNumarasi` veya `AssetTag` alanlarından en az biri zorunlu olacaktır.
- `SeriNumarasi` olmayan tekil cihazlarda `AssetTag` zorunlu olacaktır.
- `SeriNumarasi` varsa `AssetTag` opsiyonel olabilir; ancak kurum içi barkod/QR etiketleme istenirse `AssetTag` de kullanılabilir.

### Kategoriler

Planlanan ana kategoriler:

- Bilgisayar
  - Laptop
  - Masaüstü Kasa
  - Workstation
  - Mini PC
- Mobil Cihazlar
  - Akıllı Telefonlar
  - iPad/Android Tabletler
  - El Terminalleri
  - POS Cihazları
- Görüntü ve Ses Ekipmanları
  - Harici Monitörler
  - Konferans Kameraları
  - Projektörler
  - Akıllı Tahtalar
- Ağ ve Altyapı Cihazları
  - Router
  - Switch
  - Firewall
  - Access Point
  - UPS
  - NAS Depolama Cihazları
- Çevre Birimleri
  - Port Çoğaltıcılar
  - Profesyonel Kulaklıklar
  - Klavyeler
  - Mouse'lar
- Sarf Malzemeleri
  - Kablolar
  - Yazıcı Malzemeleri
  - Piller ve Enerji Ürünleri
  - Diğer Aksesuarlar

Alt kategori ayrımı yapılacaktır.

### Seri Numaralı Varlık ve Sarf Malzeme Sınıflandırması

Seri numaralı tekil varlık olarak sınıflandırılacak ürünler:

- Bilgisayar kategorisindeki laptop, masaüstü kasa, workstation ve mini PC ürünleri
- Mobil Cihazlar kategorisindeki akıllı telefon, tablet, el terminali ve POS cihazları
- Görüntü ve Ses Ekipmanları kategorisindeki harici monitör, konferans kamerası, projektör ve akıllı tahta ürünleri
- Ağ ve Altyapı Cihazları kategorisindeki router, switch, firewall, access point, UPS ve NAS depolama cihazları
- Çevre Birimleri kategorisindeki port çoğaltıcı, profesyonel kulaklık, klavye ve mouse ürünleri

Sarf malzeme olarak sınıflandırılacak ürünler:

- Kablolar
- Yazıcı malzemeleri
- Piller ve enerji ürünleri
- Laptop kılıfı, ekran temizleme kiti, kablo düzenleyici, boş USB bellek gibi diğer aksesuarlar

Sarf malzemelerinde minimum stok seviyesi kategori bazlı tanımlanacaktır.
- Kritik stok seviyesi lokasyon-cihaz modeli ve lokasyon-kategori kırılımında takip edilecektir.

### Toplam Varlık ve Kullanılabilir Stok Ayrımı

- Toplam varlık, sisteme kayıtlı ve halen organizasyon varlığı sayılan tüm cihazları ifade eder.
- Toplam varlığa zimmetlenmiş ve zimmetlenmemiş cihazlar dahildir.
- Kullanılabilir stok, depoda bulunan ve kullanıma hazır cihazları ifade eder.
- Kullanılabilir stok hesaplaması ayrı bir sayaç tablosundan değil, anlık olarak cihaz durumlarından hesaplanacaktır.
- Kullanılabilir stoka bakımda, arızalı, kaybolmuş, çalınmış, hurda veya ıskartaya ayrılmış cihazlar dahil edilmez.
- Zimmetleme işlemi toplam varlığı azaltmaz.
- Zimmetleme işlemi fiziksel stok çıkışı olarak yorumlanmayacaktır.
- Zimmetleme sonrasında cihaz depoda kullanılabilir olmaktan çıkar ve zimmetli duruma geçer.
- Arızalı durumda olan cihaz toplam varlıktan düşmez, yalnızca kullanılabilir stoktan düşer.
- Hurda veya ıskarta durumundaki cihaz depoda bekliyorsa toplam varlık içinde gösterilir.
- Hurda veya ıskarta durumundaki cihaz depoda değilse, atıldıysa veya satıldıysa toplam varlık içinde gösterilmez.
- Sarf malzemelerinde kullanılabilir stok `EldekiMiktar` alanından takip edilir.
- Kullanılabilir stok raporu, seri numaralı varlıkları ve sarf malzemelerini aynı raporda ayrı bölümler halinde gösterebilir.

### Stok Azalma Sebepleri

Stok azalması yalnızca aşağıdaki durumlarda oluşacaktır:

- Manuel stok çıkışı
- Arıza
- Çalınma
- Kaybolma
- Hurda veya ıskartaya ayrılma

Not: Bu liste ilk tasarımda stok etkisi olan nedenleri anlatır. Güncel uygulamada cihaz tarafında bu nedenler “cihaz durum hareketi” olarak işlenir; sarf malzeme tarafında ise gerçek adet değişimi üreten “stok hareketi” olarak kalır.

Manuel stok çıkışı aşağıdaki durumlarda kullanılabilir:

- Fiziksel sayım düzeltmeleri
- Ürünün depoda hasara uğraması
- Kaza sonucu stoktan düşme
- Cihazın veya sarf malzemenin kullanım ömrünün bitmesi ancak çalışır durumda olması

### Envanter Giriş ve Çıkış Bilgileri

- Garanti bitiş tarihi takip edilmeyecektir.
- Satın alma tarihi, fatura numarası ve tedarikçi bilgisi gibi mali bilgiler tutulmayacaktır.
- Envantere giriş tarihi tutulacaktır.
- Envanterden çıkış tarihi, cihaz toplam varlıktan çıkarıldığında tutulacaktır.
- Hurda veya ıskarta cihaz depodan satıldı ya da atıldı durumuna geçerse depodan çıkarıldığı tarih ve satış/elden çıkarma bilgileri tutulabilir.

## 7. Cihaz Durum Modeli

Planlanan cihaz durumları:

- `Kullanilabilir`: Depoda hazır, kullanılabilir.
- `Zimmetli`: Personele zimmetli.
- `Incelemede`: Zimmet iadesi sonrası fiziki kontrolde.
- `Bakimda`: Bakımda veya arızalı.
- `HasarliTeslimAlindi`: Hasarlı teslim alınmış.
- `Kayip`: Kayıp.
- `Calindi`: Çalınmış.
- `HurdaIskarta`: Hurda veya ıskartaya ayrılmış.
- `KullanimDisi`: Kullanım dışı bırakılmış.

## 8. Zimmet Kuralları

- Bir personele aynı anda birden fazla cihaz zimmetlenebilir.
- Bir cihaz aynı anda birden fazla personele zimmetlenemez.
- Aktif zimmet kaydı olan cihaz tekrar zimmetlenemez.
- Departman ortak kullanımına verilen cihazlar departman sorumlusu adına zimmetlenir.
- Zimmet geçmişi silinmeyecektir.
- Faz 5'te zimmet oluşturulduğunda cihaz durumu EnvanterServisi üzerinden `Zimmetlendi` hareketiyle güncellenir.
- Faz 5'te zimmet iadesi alındığında cihaz durumu EnvanterServisi üzerinden `ZimmetIadeAlindi` hareketiyle `Incelemede` yapılır.
- `ZimmetOlusturuldu`, `ZimmetIadeAlindi`, `CihazKontroleAlindi`, `ZimmetIadeEdildi` ve gerektiğinde `CihazHasarliTeslimAlindi` eventleri Faz 6 CAP/RabbitMQ aşamasında üretilir.

## 9. Zimmet İade Süreci

Zimmet iadesi doğrudan "depoya döndü" olarak tamamlanmayacaktır. İade alınan cihaz önce fiziki kontrol sürecine girer.

İade akış kararı:

1. Personel cihazı iade eder.
2. Zimmet kaydı iade sürecine alınır.
3. Cihaz durumu `Incelemede` olur.
4. Fiziki kontrol yapılır.
5. Kontrol sonucuna göre cihaz durumu belirlenir:
   - Sağlamsa: `Kullanilabilir`
   - Arızalıysa: `Bakimda`
   - Çok ağır hasarlı veya kullanılamaz durumdaysa: `HurdaIskarta`
   - Hasarlı teslim alındıysa: `HasarliTeslimAlindi`
6. Cihaz arızalı veya hasarlı teslim alındıysa zimmet iadesine not eklenebilir.
7. Zimmet belgesi, dijital imza veya imzalı belge dosyası ilk kapsamda yer almayacaktır.
8. Zimmet iade sürecinde fiziki kontrolü yapan kullanıcı kaydedilecektir.
9. Hasarlı teslim alınan cihaz için ayrı bir bakım süreci izlenmeyecektir.
10. Cihaz bakımdan geldikten sonra fiziki test yapılır ve cihaz durumu cihaz durum hareketiyle güncellenebilir.
11. Zimmet ve iade fotoğrafları Faz 5 kapsamından çıkarılmıştır.

## 10. Lokasyon ve Departman Kararları

- Lokasyonlar şimdilik basit liste halinde tutulabilir.
- Bina, kat, oda gibi hiyerarşik lokasyon modeli tasarımda desteklenecek şekilde düşünülmelidir.
- İlk sürümde basit liste tercih edilirse ileride hiyerarşik modele geçiş için `UstLokasyonId` alanı eklenebilir.
- Departman ayrı entity olarak tutulacaktır.
- Departman sorumlusu ITPersoneli veya Admin rolündeki kullanıcı tarafından belirlenir.

## 11. Personel Yaşam Döngüsü Kararları

- Personel kayıtları fiziksel olarak silinmeyecektir.
- İşten ayrılan personel `IstenAyrildi` durumuna alınacaktır.
- İşten ayrılan personelin kullanıcı hesabı aynı servis içinde pasifleştirilecektir.
- İşten ayrılan personel sisteme giriş yapamayacaktır.
- İşten ayrılan personele yeni zimmet oluşturulamayacaktır.
- İşten ayrılan personelin geçmiş zimmet kayıtları korunacaktır.
- Personel işten ayrılmadan önce aktif zimmet kontrolü yapılmalıdır.
- Aktif zimmeti olan personelin işten ayrılması sistem tarafından engellenmeyecektir.
- Aktif zimmeti olan işten ayrılmış personel için sistem "iade bekliyor" durumu üretmelidir.
- Aktif zimmetler kapatılmadan geçmiş zimmet kayıtları değiştirilmemeli veya silinmemelidir.
- Departman sorumlusu işten ayrılırsa ilgili departmana yeni sorumlu atanmalıdır.

## 12. Veri Modeli Etkileri

### EnvanterServisi Ana Verileri

- VarlikTipi
  - `SeriNumaraliVarlik`: Seri numarası ile takip edilen tekil varlık
  - `SarfMalzeme`: Sarf malzemesi

- Cihaz
  - Id
  - SeriNumarasi
  - AssetTag
  - Ad
  - Marka
  - Model
  - KategoriId
  - LokasyonId
  - Durum
  - EnvantereGirisTarihi
  - EnvanterdenCikisTarihi
  - EldenCikarmaTipi
  - EldenCikarmaAciklamasi
  - SatilanKisiVeyaKurum
  - AktifMi
  - ToplamVarligaDahilMi
  - OlusturulmaTarihi
  - GuncellenmeTarihi

- SarfMalzeme
  - Id
  - Ad
  - KategoriId
  - LokasyonId
  - EldekiMiktar
  - KritikStokSeviyesi
  - Birim
  - AktifMi
  - OlusturulmaTarihi
  - GuncellenmeTarihi

- Kategori
  - Id
  - Ad
  - KritikStokSeviyesi
  - AktifMi

- KritikStokKuralı
  - Id
  - LokasyonId
  - KategoriId
  - CihazModeli
  - KritikStokSeviyesi
  - AktifMi

- Lokasyon
  - Id
  - Ad
  - UstLokasyonId
  - AktifMi

- StokHareketi
  - Id
  - CihazId
  - SarfMalzemeId
  - HareketTipi
  - Neden
  - Miktar
  - Aciklama
  - OlusturanKullaniciId
  - OlusturulmaTarihi

### ZimmetServisi Ana Verileri

- Zimmet
  - Id
  - CihazId
  - CihazAd
  - CihazAssetTag
  - CihazSeriNumarasi
  - PersonelId
  - PersonelAdSoyad
  - PersonelEmail
  - ZimmetTarihi
  - ZimmetleyenKullaniciId
  - IadeTarihi
  - IadeAlanKullaniciId
  - Durum
  - IadeKontrolDurumu
  - IadeKontroluYapanKullaniciId
  - IadeNotu
  - OlusturulmaTarihi
  - GuncellenmeTarihi

Fotoğraf kararı:

- Zimmet ve iade fotoğrafları Faz 5 kapsamından çıkarılmıştır.
- Bu nedenle fotoğraf tablosu, endpointi ve MVC UI alanı oluşturulmayacaktır.

### KimlikVePersonelServisi Ana Verileri

- Personel
  - Id
  - Ad
  - Soyad
  - Email
  - Departman
  - DepartmanId
  - Unvan
  - DepartmanSorumlusuMu
  - Durum
  - IseGirisTarihi
  - IstenAyrilisTarihi
  - AktifMi

- Departman
  - Id
  - Ad
  - SorumluPersonelId
  - AktifMi

- Kullanici
  - Id
  - KullaniciAdi
  - SifreHash
  - Rol
  - PersonelId
  - AktifMi

## 13. Event Tasarımı

Event bus yaklaşımı:

- Event bus için DotNetCore.CAP kullanılacaktır.
- Mesaj taşıyıcı olarak RabbitMQ kullanılacaktır.
- PostgreSQL kullanan servislerde CAP Outbox tabloları aynı veritabanı içinde tutulacaktır.
- CAP Outbox şemaları servis bazında ayrılmıştır: `cap_kimlik`, `cap_envanter`, `cap_zimmet`.
- İş verisi kaydı ile event kaydı aynı transaction kapsamında yazılacaktır.
- CAP, Outbox kaydını daha sonra RabbitMQ'ya güvenilir şekilde yayınlayacaktır.

RabbitMQ exchange:

- `inventory.events`

Planlanan eventler:

- `ZimmetOlusturuldu`
- `ZimmetIadeAlindi`
- `ZimmetIadeEdildi`
- `KritikStokSeviyesineDusuldu`
- `CihazDurumuDegisti`
- `CihazKontroleAlindi`
- `CihazHasarliTeslimAlindi`
- `PersonelIstenAyrildi`

Bildirim üretme kararı:

- Sistem yalnızca kritik stok seviyesinin altına düşüldüğünde SignalR bildirimi üretecektir.
- Zimmet oluşturma, zimmet iade, personel işten ayrılma ve cihaz durum değişikliği eventleri audit/entegrasyon amacıyla üretilebilir; ancak bu eventler SignalR bildirimi üretmeyecektir.

Tüm eventlerde asgari olarak şu bilgiler bulunmalıdır:

- EventId
- EventAdi
- IslemTipi
- OccurredAt
- KaynakServis
- CorrelationId
- KullaniciId
- PersonelId
- Rol
- Payload

Kullanıcı bağlamı kararı:

- Event payload veya metadata içinde gerekli durumlarda `KullaniciId`, `PersonelId`, `Rol` ve `CorrelationId` taşınacaktır.
- Kullanıcı bağlamı, audit loglarda işlemi yapan kişinin izlenebilmesi ve servisler arası süreç takibi için kullanılacaktır.

## 14. Cache Tasarımı

- Redis yalnızca sık okunan ve nadiren değişen referans veriler için kullanılacaktır.
- Faz 8 itibarıyla ilk uygulama kapsamı EnvanterServisi referans verileridir.
- İlk cache kapsamı:
  - Kategori listesi
  - Lokasyon listesi
- Kategori veya lokasyon ekleme/güncelleme/pasifleştirme işlemlerinde ilgili cache temizlenmelidir.
- Cache anahtarları `envanter:kategoriler:v1` ve `envanter:lokasyonlar:v1` olarak belirlenmiştir.
- Varsayılan referans veri cache süresi 30 dakikadır.
- Redis kapalıysa veya geçici hata verirse EnvanterServisi PostgreSQL üzerinden okumaya devam eder; cache hatası ana iş akışını bozmaz.

## 15. Güvenlik ve Yetki Tasarımı

Planlanan roller:

- `Admin`
- `ITPersoneli`
- `PersonelKullanicisi`

Yetki yaklaşımı:

- Admin tüm işlemleri yapabilir.
- ITPersoneli cihaz, stok ve zimmet operasyonlarını yürütebilir.
- PersonelKullanicisi yalnızca kendisine ait zimmet süreçlerini sistem üzerinden takip edebilir.
- Rol bazlı yetkilendirme şimdilik yeterli kabul edilmiştir.

Şifre kuralları:

- Şifre minimum 8, maksimum 64 karakter olmalıdır.
- Şifrede en az bir rakam bulunmalıdır.
- Şifrede en az bir büyük harf bulunmalıdır.
- Şifrede en az bir küçük harf bulunmalıdır.
- Şifrede en az bir sembol bulunmalıdır.

JWT token içinde bulunması beklenen claim'ler:

- KullaniciId
- KullaniciAdi
- Rol
- PersonelId

`PersonelId` zorunlu claim olacaktır; çünkü sisteme giriş yapan her kullanıcı bir personel kaydına bağlıdır.

## 16. Alan Açıklamaları

- `Cihaz.AktifMi`, cihaz kaydının sistemde yönetilebilir ve aktif yaşam döngüsünde olup olmadığını ifade eder. Kullanıcı tarafından elle değiştirilmez; cihaz durumu ve elden çıkarma tipine göre EnvanterServisi tarafından hesaplanır.
- `Cihaz.ToplamVarligaDahilMi`, cihazın toplam varlık sayımına girip girmediğini ifade eder. Kullanıcı tercihi değildir; `Kayip`, `Calindi`, `KullanimDisi` veya elden çıkarılmış `HurdaIskarta` durumlarında sistem tarafından `false` yapılır.
- `SarfMalzeme.Birim`, sarf malzemenin hangi ölçü birimiyle takip edildiğini ifade eder. Örnek değerler: `Adet`, `Paket`, `Kutu`, `Metre`.
- `OlusturulmaTarihi`, kaydın sisteme ilk eklendiği tarihi ifade eder.
- `GuncellenmeTarihi`, kaydın son değiştirildiği tarihi ifade eder.
- `Lokasyon.UstLokasyonId`, hiyerarşik lokasyon yapısını kurmak için kullanılır. Örneğin `Genel Müdürlük > 2. Kat > Bilgi İşlem Odası` gibi bir yapıda alt lokasyonun bağlı olduğu üst lokasyonu gösterir.
- `StokHareketi`, sarf malzemelerde miktar bazlı giriş, çıkış ve düzeltme işlemlerini kayıt altına alır. Cihaz tarafında aynı tablo geçmiş kaydı için kullanılmaya devam eder, ancak kullanıcıya “Cihaz Durum Hareketi” olarak gösterilir.
- Audit log kapsamında hem eventler hem de CRUD işlemleri kaydedilecektir.

## 17. Kalan Sorular

Aşağıdaki sorular henüz netleştirilmemiştir ve analiz/tasarım aşamasında cevaplanmalıdır.

### Lokasyon ve Departman Soruları

- İlk sürümde lokasyon modeli basit liste mi olacak, yoksa doğrudan hiyerarşik yapı mı kurulacak?

### Stok ve Durum Soruları

- Kullanılabilir stok raporu ana ekranda birleşik, detayda ayrı sayfalar olacak şekilde kesinleştirilecek mi?

### Kullanıcı ve Yetki Soruları

- Admin kullanıcıları kim oluşturacak?
- Refresh token kullanılacak mı?

### Servisler Arası İletişim Soruları

- Event publish edilemezse işlem başarısız mı sayılacak, yoksa tekrar deneme mekanizması mı olacak?
- CorrelationId tüm servisler arasında taşınacak mı?
- Faz 6 kararı: ZimmetServisi'nin HTTP ile yaptığı cihaz durum hareketi çağrıları korunacak; başarılı zimmet işlemleri ayrıca CAP Outbox eventi üretecektir. Cihaz yaşam döngüsünün yazma sahibi EnvanterServisi olarak kalır.

### Audit ve Bildirim Soruları

- Audit log kayıtları kullanıcı tarafından görüntülenebilir olacak mı?
- Audit loglar silinmeyecekse arşivleme veya filtreleme ihtiyacı olacak mı?
- Kritik stok bildirimlerini hangi roller görecek?
- Kritik stok bildirimleri MongoDB veya PostgreSQL'de kalıcı olarak saklanacak mı?
- Kritik stok bildirimi her stok hareketinde tekrar üretilecek mi, yoksa eşik ilk aşıldığında bir kez mi üretilecek?

### Kapsam ve Teslim Soruları

- Proje tesliminde yalnızca analiz ve mimari doküman mı olacak, yoksa çalışan servis iskeleti de beklenecek mi?
- Staj defterinde UML diyagramları istenecek mi?
- ER diyagramı hazırlanacak mı?
- Servis iletişim diyagramı hazırlanacak mı?
- Docker Compose dosyası plan dokümanına dahil edilecek mi?

## 18. Güncel Uygulama Kararları - 2026-08-02

- Faz 5 ZimmetServisi uygulaması başlatılmış ve ayrı API projesi olarak eklenmiştir.
- Faz 6 CAP/RabbitMQ + Outbox uygulanmıştır. Faz 7 DenetimKaydiServisi uygulanmıştır. Faz 8 Redis Cache uygulanmıştır. Faz 9 ve sonrası için SignalR, ApiGateway ve Demo/Dokümantasyon ele alınacaktır. ApiGateway, Demo ve Dokümantasyon fazından hemen önceki son teknik faz olarak planlanacaktır.
- Yönetimsel kayıt silme işlemleri için fiziksel `DELETE` endpointleri eklenmeyecektir. Bunun yerine `AktifMi` alanı üzerinden pasifleştirme yapılacaktır.
- `AktifMi` ile pasifleştirme departman, personel, kategori, lokasyon, cihaz ve sarf malzeme kayıtlarında kullanılacaktır.
- Cihazlarda `AktifMi` manuel pasifleştirme checkbox'ı olarak kullanılmayacaktır. Cihazın aktifliği ve toplam varlık kapsamı cihaz durumu ile elden çıkarma tipinden sistem tarafından hesaplanacaktır.
- Personelin işten ayrılması normal pasifleştirmeden ayrı bir iş kuralıdır. Bu işlem personelin durumunu `IstenAyrildi` yapar, personeli pasifleştirir ve bağlı kullanıcı hesabını da pasifleştirir.
- MVC client tarafında personel yönetimi tek sayfada satır içi düzenleme şeklinde büyütülmeyecektir. Personel listesi tabloda gösterilecek, düzenleme ve işten ayrılma onayı ayrı sayfalarda yapılacaktır.
- EnvanterServisi cihaz durum enum değerleri güncel modelle uyumlu tutulacaktır. Eski veritabanı değerleri migration ile yeni değerlere dönüştürülecektir.

## 19. Envanter Client Listeleme ve İşlem Sayfası Kararı - 2026-08-03

- MVC client tarafında cihaz ve sarf malzeme yönetimi tek sayfada satır içi düzenleme şeklinde büyütülmeyecektir.
- Cihazlar ve sarf malzemeler ana envanter ekranında tablo halinde listelenecektir.
- Cihazla ilgili bilgi güncelleme ve cihaz durum hareketi işlemleri `CihazIslemleri` sayfasında yapılacaktır.
- Sarf malzemeyle ilgili bilgi güncelleme, stok hareketi işleme ve stok hareketi geçmişi görüntüleme işlemleri `SarfMalzemeIslemleri` sayfasında yapılacaktır.
- Bu ayrım, kayıt sayısı arttığında ana envanter ekranının taranabilir kalması ve kritik stok hareketlerinin ayrı bir işlem ekranında yürütülmesi için tercih edilmiştir.

## 20. Cihaz AssetTag ve Durum Hareketi Kararı - 2026-08-03

- `AssetTag` kurum içi kalıcı demirbaş numarasıdır ve yeni cihaz oluşturulurken sistem tarafından otomatik üretilecektir.
- Varsayılan format `BT-000001` şeklindedir.
- MVC client üzerinde yeni cihaz oluştururken kullanıcıdan `AssetTag` istenmeyecektir.
- Cihaz düzenleme sayfasında `AssetTag` salt okunur bilgi olarak gösterilecektir.
- Cihaz listesi aktiflik, kategori ve lokasyon filtreleriyle süzülebilecektir.
- Cihazlarda kullanıcıya `Cihaz Durum Hareketi` kavramı gösterilecektir. Cihaz durum hareketi için tek yazma endpointi `POST /api/cihazlar/{id}/durum-hareketleri` olacaktır.
- Durum hareketi kayıtları mevcut `StokHareketleri` tablosunda cihaz bazında tutulacaktır ve cihaz işlem sayfasında cihaz geçmişi olarak gösterilecektir.
- Manuel stok çıkışı, kullanım ömrü bitişi, çalınma, kaybolma, kullanım dışı bırakma ve elden çıkarılmış hurda/ıskarta durumlarında cihaz pasif ve toplam varlık dışı yapılacaktır.
- Sarf malzemelerde `Stok Hareketi` kavramı korunacaktır; çünkü burada giriş, çıkış ve düzeltme gerçek miktar hareketidir.

## 21. Cihaz Kapsam Alanları Kararı - 2026-08-08

- Cihazlarda `AktifMi` ve `ToplamVarligaDahilMi` alanları tek kaynak olarak EnvanterServisi servis katmanında hesaplanacaktır.
- `Kullanilabilir`, `Zimmetli`, `Incelemede`, `Bakimda`, `HasarliTeslimAlindi` ve kurumda duran `HurdaIskarta` cihazlar aktif ve toplam varlığa dahil sayılacaktır.
- `Kayip`, `Calindi`, `KullanimDisi` ve elden çıkarılmış `HurdaIskarta` cihazlar pasif ve toplam varlık dışı sayılacaktır.
- Cihaz bilgi güncelleme ekranında bu alanlar salt okunur gösterilecek, kullanıcı tarafından değiştirilmeyecektir.
- Mevcut cihaz verileri `CihazKapsamAlanlariniDurumaGoreDuzelt` migration'ı ile bu kurala hizalanacaktır.
- Cihazın `Durum` alanı da bilgi güncelleme formundan değiştirilmeyecektir. Bütün cihaz durum değişiklikleri `Cihaz Durum Hareketi` üzerinden yapılacak ve geçmişe kaydedilecektir.
- Bakımdan dönen cihaz için `BakimdanDondu` hareketi kullanılacak ve cihaz tekrar `Kullanilabilir` durumuna alınacaktır.
- Zimmet oluşturma akışı cihazı değiştirmek için `Zimmetlendi` durum hareketini kullanacak ve cihaz `Zimmetli` durumuna alınacaktır.
- Zimmet iade alındığında `ZimmetIadeAlindi` durum hareketi kullanılacak ve cihaz fiziki kontrol için `Incelemede` durumuna alınacaktır.
- ZimmetServisi cihaz durumunu doğrudan veritabanında değiştirmeyecektir; cihaz yaşam döngüsünün ve kapsam alanlarının tek sahibi EnvanterServisi olacaktır.
- `EnvantereGiris`, cihaz durum hareketi nedeni olarak kullanılmayacaktır; yeni cihaz oluşturma akışına aittir.

## 22. Faz 5 ZimmetServisi Kararı - 2026-08-08

- ZimmetServisi ayrı API projesi olarak `http://localhost:5002` adresinde çalışır.
- Zimmet verileri PostgreSQL içinde `zimmet` şemasında tutulur.
- `Aktif` ve `IadeSurecinde` durumları açık zimmet kabul edilir; aynı cihaz için aynı anda yalnızca bir açık zimmet olabilir.
- Zimmet oluşturulacak personel `AktifMi = true` ve `Durum = Aktif` olmalıdır.
- Zimmet oluşturulacak cihaz `AktifMi = true` ve `Durum = Kullanilabilir` olmalıdır.
- Faz 5'te servisler arası iletişim senkron HTTP'dir.
- ZimmetServisi, gelen kullanıcı JWT token'ını KimlikVePersonelServisi ve EnvanterServisi çağrılarına forward eder.
- ZimmetServisi cihaz durumunu doğrudan değiştirmez; EnvanterServisi cihaz durum hareketi endpointini kullanır.
- MVC client üzerinde `Zimmetler` ekranı bulunur. Admin/IT zimmet oluşturur, iade alır ve iade kontrolünü tamamlar. Personel kullanıcısı kendi zimmetlerini görür.
- Personel kullanıcısının kendi zimmetlerini başka servis okuması olmadan görebilmesi için zimmet kaydında atama anındaki personel ve cihaz görüntü bilgileri de saklanır.
- Zimmet ve iade fotoğrafları uygulanmayacaktır; fotoğraf tablosu, endpointi ve UI alanı yoktur.
- CAP/RabbitMQ ve Outbox Faz 6'da eklenmiştir.

## 23. Faz 6 CAP/RabbitMQ + Outbox Kararı - 2026-08-10

- RabbitMQ `docker-compose.yml` içine `rabbitmq:3-management` container'ı olarak eklenmiştir.
- Ortak RabbitMQ exchange adı `inventory.events` olarak belirlenmiştir.
- KimlikVePersonelServisi, EnvanterServisi ve ZimmetServisi DotNetCore.CAP ile event üretir.
- CAP PostgreSQL outbox şemaları servis bazında ayrıdır: `cap_kimlik`, `cap_envanter`, `cap_zimmet`.
- Personel işten ayrıldığında `personel.isten-ayrildi` eventi yayınlanır.
- Cihaz durum hareketlerinde `cihaz.durumu-degisti` eventi yayınlanır.
- Kritik stok eşiğinin altına düşen cihaz veya sarf malzeme için `stok.kritik-seviyeye-dusuldu` eventi yayınlanır.
- Zimmet oluşturma ve iade akışlarında `zimmet.olusturuldu`, `zimmet.iade-alindi`, `cihaz.kontrole-alindi`, `zimmet.iade-edildi` ve hasarlı iade için `cihaz.hasarli-teslim-alindi` eventleri yayınlanır.
- DenetimKaydiServisi Faz 7'de event consumer olarak eklenmiştir; BildirimServisi event consumer uygulaması Faz 9 kapsamındadır.

## Faz 7 DenetimKaydiServisi Kararı - 2026-08-11

- DenetimKaydiServisi ayrı API olarak `http://localhost:5003` adresinde çalışır.
- MongoDB yalnızca audit/event log depolama için kullanılır; container adı `it-envanter-mongodb`, portu `27017` olarak belirlenmiştir.
- CAP/RabbitMQ üzerinden gelen audit kapsamındaki domain eventleri DenetimKaydiServisi tarafından tüketilir ve MongoDB `DenetimKayitlari` koleksiyonuna yazılır.
- Event kayıtlarında `EventId` benzersizdir; aynı event tekrar teslim edilirse duplicate audit kaydı oluşturulmaz.
- CRUD audit, kaynak servislerde global action filter üzerinden uygulanır.
- CRUD audit çağrısı best-effort HTTP çağrısıdır. DenetimKaydiServisi kapalıysa ana iş akışı başarısız sayılmaz, kaynak servis uyarı logu yazar.
- MVC client içine Denetim ekranı eklenmiştir. Admin ve ITPersoneli audit kayıtlarını filtreleyebilir, detay ekranında payload JSON içeriğini görebilir.

## Faz 8 Redis Cache Kararı - 2026-08-12

- Redis `docker-compose.yml` içine `redis:7-alpine` servisi olarak eklenmiştir.
- Redis container adı `it-envanter-redis`, portu `6379` olarak belirlenmiştir.
- Faz 8 kapsamı yalnızca EnvanterServisi kategori ve lokasyon listeleme akışıdır.
- EnvanterServisi `Microsoft.Extensions.Caching.StackExchangeRedis` ve `IDistributedCache` üzerinden Redis cache kullanır.
- Kategoriler `envanter:kategoriler:v1`, lokasyonlar `envanter:lokasyonlar:v1` anahtarıyla cache'lenir.
- Cache süresi `Cache:ReferansVeriDakika` ayarıyla yönetilir ve varsayılan değer 30 dakikadır.
- Kategori/lokasyon oluşturma veya güncelleme başarılı olunca ilgili cache temizlenir.
- Redis performans katmanıdır; veri doğruluğunun tek kaynağı PostgreSQL olarak kalır.
- Redis okunamaz, yazılamaz veya temizlenemezse ana API işlemi başarısız sayılmaz, kaynak servis uyarı logu yazar.
