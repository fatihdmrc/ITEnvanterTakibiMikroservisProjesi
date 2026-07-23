# Gereksinim Analizi Dokümanı

## 1. Amaç

Bu sistem, IT ekipmanlarının ve sarf malzemelerinin envanter içinde takip edilmesi, seri numaralı cihazların personele zimmetlenmesi, iade süreçlerinin yönetilmesi, stok durumlarının izlenmesi ve kritik stok seviyelerinde bildirim üretilmesi amacıyla tasarlanacaktır.

Proje, SDLC döngüsünün analiz ve tasarım aşamalarına odaklanır. Öncelik kod geliştirmekten önce gereksinimleri, iş kurallarını, servis sınırlarını, veri modelini ve iletişim akışlarını netleştirmektir.

## 2. Kapsam

Sistem aşağıdaki ana yetenekleri kapsar:

- Seri numaralı IT varlıklarının envantere kaydedilmesi
- Sarf malzemelerinin adet bazlı stok olarak takip edilmesi
- Personel, departman ve kullanıcı bilgilerinin yönetilmesi
- Cihazların personele zimmetlenmesi
- Zimmet iade ve fiziki kontrol sürecinin yönetilmesi
- Cihaz durumlarının takip edilmesi
- Kullanılabilir stok ve toplam varlık bilgisinin hesaplanması
- Kritik stok durumlarında bildirim üretilmesi
- RabbitMQ ile event tabanlı iletişim kurulması
- MongoDB ile audit/event log tutulması
- Redis ile sık kullanılan referans verilerin cache'lenmesi
- SignalR ile anlık bildirim altyapısının tasarlanması

## 3. Kapsam Dışı

İlk kapsamda aşağıdaki konular yer almayacaktır:

- Api Gateway kullanımı
- Zimmet belgesi üretimi
- Dijital imza veya imzalı belge yükleme
- Garanti bitiş tarihi takibi
- Satın alma tarihi, fatura numarası ve tedarikçi gibi mali bilgiler
- Test yazımı
- CI/CD
- Kubernetes veya production deployment
- Gelişmiş OAuth/OpenID Connect altyapısı

## 4. Aktörler

### Admin

- Tüm sistem ayarlarını ve kayıtları yönetebilir.
- Kullanıcı, rol, personel, departman, cihaz, stok ve zimmet işlemlerini yapabilir.
- Departman sorumlusu belirleyebilir.

### ITPersoneli

- Cihaz, sarf malzeme, stok ve zimmet operasyonlarını yürütebilir.
- Personel ayrılışı gibi operasyonel süreçleri işleyebilir.
- Zimmet iade kontrolünü yapabilir.

### PersonelKullanicisi

- Yalnızca kendisine ait zimmet süreçlerini sistem üzerinden takip edebilir.
- Veri değiştirme yetkisi yoktur.

## 5. Fonksiyonel Gereksinimler

### Kimlik ve Personel Yönetimi

- Sistem kullanıcı girişi yapabilmelidir.
- Başarılı girişte JWT token üretilebilmelidir.
- Kullanıcılar rol bazlı yetkilendirme ile yönetilmelidir.
- Sisteme giriş yapan her kullanıcı bir personel kaydına bağlı olmalıdır.
- Şifre minimum 8, maksimum 64 karakter olmalıdır.
- Şifrede en az bir rakam, bir büyük harf, bir küçük harf ve bir sembol bulunmalıdır.
- Personel kayıtları fiziksel olarak silinmemelidir.
- İşten ayrılan personel `IstenAyrildi` durumuna alınmalıdır.
- İşten ayrılan personelin kullanıcı hesabı pasifleştirilmelidir.
- İşten ayrılan personele yeni zimmet oluşturulmamalıdır.
- Departman ayrı entity olarak tutulmalıdır.
- Departman sorumlusu Admin veya ITPersoneli tarafından belirlenebilmelidir.

### Envanter Yönetimi

- Varlıklar iki ana tipe ayrılmalıdır:
  - Seri numarası veya AssetTag ile takip edilen tekil varlıklar
  - Adet bazlı takip edilen sarf malzemeleri
- Tekil takip edilen varlıklarda `SeriNumarasi` veya `AssetTag` alanlarından en az biri zorunlu olmalıdır.
- Sarf malzemeleri `EldekiMiktar` alanı ile takip edilmelidir.
- Envantere giriş tarihi tutulmalıdır.
- Envanterden çıkış tarihi, cihaz toplam varlıktan çıkarıldığında tutulmalıdır.
- Kategori ve alt kategori ayrımı yapılmalıdır.
- Lokasyonlar başlangıçta basit liste olarak tutulabilir; ileride hiyerarşik yapıyı desteklemek için `UstLokasyonId` alanı tasarımda bulunmalıdır.

### Zimmet Yönetimi

- Bir personele aynı anda birden fazla cihaz zimmetlenebilmelidir.
- Bir cihaz aynı anda yalnızca bir personele zimmetlenebilmelidir.
- Aktif zimmet kaydı olan cihaz tekrar zimmetlenememelidir.
- Departman ortak kullanımına verilen cihazlar departman sorumlusu adına zimmetlenmelidir.
- Zimmet oluştururken teslim eden kişi zorunlu olmalıdır.
- Zimmet oluşturma işleminde cihaz fotoğrafı eklenebilmelidir.
- Zimmet geçmişi silinmemelidir.

### Zimmet İade Yönetimi

- İade edilen cihaz doğrudan kullanılabilir duruma alınmamalıdır.
- Cihaz önce fiziki kontrol sürecine girmelidir.
- İade sırasında cihaz durumu `Incelemede` olmalıdır.
- Fiziki kontrol sonucunda cihaz şu durumlardan birine alınmalıdır:
  - `Kullanilabilir`: Sağlam ve kullanıma hazır
  - `Bakimda`: Arızalı veya bakımda
  - `HurdaIskarta`: Hurda veya ıskartaya ayrılmış
- Hasarlı teslim alınan cihaz için iade notu tutulmalıdır.
- Zimmet iade sürecinde fiziki kontrolü yapan kullanıcı kaydedilmelidir.
- İade sürecinde birden fazla fotoğraf eklenebilmelidir.
- Zimmet iadesi incelemede iken cihaz tekrar zimmetlenememelidir.
- Hasarlı teslim alınan cihaz için ayrı bir bakım süreci izlenmemelidir.
- Cihaz bakımdan geldikten sonra fiziki test ile cihaz durumu manuel olarak güncellenebilmelidir.
- Hasar ve zimmet fotoğrafları server üzerinde dosya olarak saklanmalıdır.

### Stok Yönetimi

- Kullanılabilir stok, seri numaralı cihazlarda cihaz durumlarından anlık hesaplanmalıdır.
- Sarf malzemelerinde kullanılabilir stok `EldekiMiktar` alanından okunmalıdır.
- Kritik stok seviyesi lokasyon-cihaz modeli ve lokasyon-kategori kırılımında takip edilmelidir.
- Zimmetleme işlemi toplam varlığı azaltmamalıdır.
- Zimmetleme fiziksel stok çıkışı olarak yorumlanmamalıdır.
- Stok azalması yalnızca şu durumlarda oluşmalıdır:
  - Manuel stok çıkışı
  - Arıza
  - Çalınma
  - Kaybolma
  - Hurda veya ıskartaya ayrılma
- Manuel stok çıkışı fiziksel sayım düzeltmesi, depoda hasar, kaza veya kullanım ömrü biten çalışan ürünler için kullanılabilmelidir.

### Bildirim ve Audit

- Zimmet oluşturma, zimmet iade, stok azalması ve kritik stok olaylarında event üretilmelidir.
- Eventler RabbitMQ üzerinden yayınlanmalıdır.
- Event geçmişi MongoDB üzerinde JSON tabanlı olarak tutulmalıdır.
- Audit log kapsamında hem eventler hem de CRUD işlemleri kaydedilmelidir.
- SignalR bildirimi yalnızca kritik stok seviyesinin altına düşüldüğünde üretilmelidir.
- Zimmet, iade, personel ayrılışı ve cihaz durum değişikliği eventleri audit amacıyla kullanılabilir; bu eventler SignalR bildirimi üretmemelidir.

## 6. Fonksiyonel Olmayan Gereksinimler

- Sistem mikroservis mimarisiyle tasarlanmalıdır.
- Her servis kendi sorumluluk alanına sahip olmalıdır.
- Her servis kendi portunda ve kendi Swagger arayüzüyle çalışmalıdır.
- Servisler arası senkron doğrulamalar HTTP ile yapılmalıdır.
- Servisler arası asenkron iletişim RabbitMQ ile sağlanmalıdır.
- PostgreSQL ana operasyonel veriler için kullanılmalıdır.
- Redis sık kullanılan referans veriler için kullanılmalıdır.
- MongoDB audit/event log kayıtları için kullanılmalıdır.
- Sistem staj sunumunda anlaşılır şekilde açıklanabilir olmalıdır.

## 7. Ürün Sınıflandırması

Seri numaralı tekil varlıklar:

- Bilgisayar: Laptop, Masaüstü Kasa, Workstation, Mini PC
- Mobil Cihazlar: Akıllı Telefonlar, iPad/Android Tabletler, El Terminalleri, POS Cihazları
- Görüntü ve Ses Ekipmanları: Harici Monitörler, Konferans Kameraları, Projektörler, Akıllı Tahtalar
- Ağ ve Altyapı Cihazları: Router, Switch, Firewall, Access Point, UPS, NAS Depolama Cihazları
- Çevre Birimleri: Port Çoğaltıcılar, Profesyonel Kulaklıklar, Klavyeler, Mouse'lar

Sarf malzemeleri:

- Kablolar
- Yazıcı malzemeleri
- Piller ve enerji ürünleri
- Laptop kılıfı, ekran temizleme kiti, kablo düzenleyici, boş USB bellek gibi diğer aksesuarlar

## 8. Açık Sorular

- İlk sürümde lokasyon modeli basit liste mi olacak, yoksa doğrudan hiyerarşik yapı mı kurulacak?
- Admin kullanıcıları kim oluşturacak?
- Refresh token kullanılacak mı?
