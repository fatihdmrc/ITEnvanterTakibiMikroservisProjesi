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
- Zimmet oluşturma ve iade olaylarını DotNetCore.CAP üzerinden RabbitMQ'ya yayınlar.

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
- Zimmet oluşturulduğunda `ZimmetOlusturuldu` eventi üretilir.
- Zimmet iade süreci tamamlandığında `ZimmetIadeEdildi` eventi üretilir.

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
6. Cihaz arızalı veya hasarlı teslim alındıysa zimmet iadesine "Hasarlı Teslim Alındı" notu eklenir.
7. Hasarlı teslim alma durumunda nota fotoğraf eklenebilmelidir.
8. Zimmet oluşturma işleminde cihaz fotoğrafı eklenebilecektir.
9. Zimmet iade işleminde birden fazla cihaz/hasar fotoğrafı eklenebilecektir.
10. Zimmet belgesi, dijital imza veya imzalı belge dosyası ilk kapsamda yer almayacaktır.
11. Zimmet iade sürecinde fiziki kontrolü yapan kullanıcı kaydedilecektir.
12. Hasarlı teslim alınan cihaz için ayrı bir bakım süreci izlenmeyecektir.
13. Cihaz bakımdan geldikten sonra fiziki test yapılır ve cihaz durumu manuel olarak güncellenebilir.
14. Hasar ve zimmet fotoğrafları server üzerinde dosya olarak tutulacaktır.

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
  - PersonelId
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

- ZimmetFotografi
  - Id
  - ZimmetId
  - FotografTipi
  - DosyaYolu
  - Aciklama
  - YukleyenKullaniciId
  - YuklenmeTarihi

Fotoğraf saklama kararı:

- Hasar ve zimmet fotoğrafları server üzerinde dosya olarak saklanacaktır.
- Veritabanında dosyanın kendisi değil, `DosyaYolu` bilgisi tutulacaktır.

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
- İş verisi kaydı ile event kaydı aynı transaction kapsamında yazılacaktır.
- CAP, Outbox kaydını daha sonra RabbitMQ'ya güvenilir şekilde yayınlayacaktır.

RabbitMQ exchange:

- `inventory.events`

Planlanan eventler:

- `ZimmetOlusturuldu`
- `ZimmetIadeEdildi`
- `StokAzaldi`
- `KritikStokSeviyesineDusuldu`
- `CihazDurumuDegisti`
- `CihazKontroleAlindi`
- `CihazHasarliTeslimAlindi`
- `CihazHurdayaAyrildi`
- `PersonelIstenAyrildi`

Bildirim üretme kararı:

- Sistem yalnızca kritik stok seviyesinin altına düşüldüğünde SignalR bildirimi üretecektir.
- Zimmet oluşturma, zimmet iade, personel işten ayrılma, cihaz durum değişikliği ve stok azalması eventleri audit/entegrasyon amacıyla üretilebilir; ancak bu eventler SignalR bildirimi üretmeyecektir.

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
- İlk cache kapsamı:
  - Kategori listesi
  - Lokasyon listesi
- Kategori veya lokasyon ekleme/güncelleme/pasifleştirme işlemlerinde ilgili cache temizlenmelidir.

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

- `Cihaz.AktifMi`, cihaz kaydının sistemde aktif olarak kullanılıp kullanılmadığını ifade eder. Fiziksel cihaz durumunu değil, kaydın yönetimsel aktiflik durumunu belirtir. Kayıt silmek yerine pasifleştirme yapmak için kullanılır.
- `SarfMalzeme.Birim`, sarf malzemenin hangi ölçü birimiyle takip edildiğini ifade eder. Örnek değerler: `Adet`, `Paket`, `Kutu`, `Metre`.
- `OlusturulmaTarihi`, kaydın sisteme ilk eklendiği tarihi ifade eder.
- `GuncellenmeTarihi`, kaydın son değiştirildiği tarihi ifade eder.
- `Lokasyon.UstLokasyonId`, hiyerarşik lokasyon yapısını kurmak için kullanılır. Örneğin `Genel Müdürlük > 2. Kat > Bilgi İşlem Odası` gibi bir yapıda alt lokasyonun bağlı olduğu üst lokasyonu gösterir.
- `StokHareketi`, cihaz veya sarf malzeme üzerinde gerçekleşen stok etkili işlemlerin kayıt altına alınmasını ifade eder. Manuel stok çıkışı, arıza, çalınma, kaybolma, hurda/ıskarta ve sayım düzeltmesi gibi işlemler bu kayıt üzerinden izlenir.
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

- ZimmetServisi, cihaz durumunu değiştirmek için EnvanterServisi'ne HTTP isteği mi atacak?
- EnvanterServisi cevap vermezse zimmet oluşturma işlemi iptal mi edilecek?
- Event publish edilemezse işlem başarısız mı sayılacak, yoksa tekrar deneme mekanizması mı olacak?
- CorrelationId tüm servisler arasında taşınacak mı?
- Servisler arası HTTP çağrılarında kullanıcı JWT token'ı aynen iletilecek mi?

### Audit ve Bildirim Soruları

- Audit log kayıtları kullanıcı tarafından görüntülenebilir olacak mı?
- Audit loglar silinmeyecekse arşivleme veya filtreleme ihtiyacı olacak mı?
- Kritik stok bildirimlerini hangi roller görecek?
- Kritik stok bildirimleri MongoDB veya PostgreSQL'de kalıcı olarak saklanacak mı?
- Kritik stok bildirimi her stok hareketinde tekrar üretilecek mi, yoksa eşik ilk aşıldığında bir kez mi üretilecek?

### Dosya/Fotoğraf Soruları

- Fotoğraf için dosya boyutu ve uzantı sınırı olacak mı?
- Fotoğraf silme veya değiştirme işlemi desteklenecek mi?

### Kapsam ve Teslim Soruları

- Proje tesliminde yalnızca analiz ve mimari doküman mı olacak, yoksa çalışan servis iskeleti de beklenecek mi?
- Staj defterinde UML diyagramları istenecek mi?
- ER diyagramı hazırlanacak mı?
- Servis iletişim diyagramı hazırlanacak mı?
- Docker Compose dosyası plan dokümanına dahil edilecek mi?

## 18. Güncel Uygulama Kararları - 2026-08-02

- Proje geliştirmesi şimdilik Faz 4 sınırında tutulacaktır.
- Faz 5 ve sonrası için ZimmetServisi, ApiGateway, CAP/RabbitMQ, audit log, Redis ve SignalR daha sonra ele alınacaktır.
- Yönetimsel kayıt silme işlemleri için fiziksel `DELETE` endpointleri eklenmeyecektir. Bunun yerine `AktifMi` alanı üzerinden pasifleştirme yapılacaktır.
- `AktifMi` ile pasifleştirme departman, personel, kategori, lokasyon, cihaz ve sarf malzeme kayıtlarında kullanılacaktır.
- Personelin işten ayrılması normal pasifleştirmeden ayrı bir iş kuralıdır. Bu işlem personelin durumunu `IstenAyrildi` yapar, personeli pasifleştirir ve bağlı kullanıcı hesabını da pasifleştirir.
- MVC client tarafında personel yönetimi tek sayfada satır içi düzenleme şeklinde büyütülmeyecektir. Personel listesi tabloda gösterilecek, düzenleme ve işten ayrılma onayı ayrı sayfalarda yapılacaktır.
- EnvanterServisi cihaz durum enum değerleri güncel modelle uyumlu tutulacaktır. Eski veritabanı değerleri migration ile yeni değerlere dönüştürülecektir.
