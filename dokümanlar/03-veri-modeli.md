# Veri Modeli Dokümanı

## 1. Genel Yaklaşım

Sistem mikroservis mimarisiyle tasarlandığı için her ana servis kendi veri modelinden sorumlu olacaktır. Operasyonel veriler PostgreSQL üzerinde tutulacak, audit/event geçmişi MongoDB üzerinde saklanacaktır.

Veritabanı ve kod isimlendirmelerinde Türkçe domain kavramları kullanılacaktır. C# ve veritabanı uyumluluğu için teknik adlarda Türkçe karakter kullanılmayacaktır.

## 2. KimlikVePersonelServisi Veri Modeli

### UygulamaKullanici

| Alan | Açıklama |
| --- | --- |
| Id | Identity kullanıcı benzersiz kimliği |
| UserName | Sisteme girişte kullanılan kullanıcı adı |
| PersonelId | Bağlı olduğu personel kaydı |
| AktifMi | Kullanıcı hesabının aktiflik durumu |

Kurallar:

- Kullanıcı, ASP.NET Core Identity `IdentityUser<Guid>` altyapısı üzerinden yönetilir.
- Şifre hashleme, doğrulama, security stamp, lockout ve rol bağlantıları Identity tabloları üzerinden tutulur.
- Roller Identity role sistemiyle `Admin`, `ITPersoneli` ve `PersonelKullanicisi` olarak yönetilir.
- `PersonelId` zorunludur.
- Personel kaydı olmayan kullanıcı oluşturulmaz.
- Bir personele yalnızca bir kullanıcı hesabı açılabilir.
- İşten ayrılan personelin kullanıcı hesabı pasifleştirilir.

### Personel

| Alan | Açıklama |
| --- | --- |
| Id | Personel benzersiz kimliği |
| Ad | Personel adı |
| Soyad | Personel soyadı |
| Email | Personel e-posta adresi |
| DepartmanId | Bağlı olduğu departman |
| Unvan | Personel ünvanı |
| DepartmanSorumlusuMu | Departman sorumlusu olup olmadığı |
| Durum | Aktif, pasif veya işten ayrılmış |
| IseGirisTarihi | İşe giriş tarihi |
| IstenAyrilisTarihi | İşten ayrılış tarihi |
| AktifMi | Cihaz kaydının sistemde aktif olarak kullanılıp kullanılmadığı |

Kurallar:

- Personel fiziksel olarak silinmez.
- İşten ayrılan personele yeni zimmet oluşturulamaz.
- Geçmiş zimmet kayıtları korunur.

### Departman

| Alan | Açıklama |
| --- | --- |
| Id | Departman benzersiz kimliği |
| Ad | Departman adı |
| SorumluPersonelId | Departman sorumlusu |
| AktifMi | Departman aktiflik durumu |

Kurallar:

- Departman sorumlusu Admin veya ITPersoneli tarafından belirlenir.
- Departman sorumlusu işten ayrılırsa yeni sorumlu atanmalıdır.

## 3. EnvanterServisi Veri Modeli

### Cihaz

| Alan | Açıklama |
| --- | --- |
| Id | Cihaz benzersiz kimliği |
| SeriNumarasi | Üretici seri numarası |
| AssetTag | Kurum içi demirbaş/takip numarası |
| Ad | Cihaz adı |
| Marka | Cihaz markası |
| Model | Cihaz modeli |
| KategoriId | Kategori ilişkisi |
| LokasyonId | Lokasyon ilişkisi |
| Durum | Cihaz durumu |
| EnvantereGirisTarihi | Envantere giriş tarihi |
| EnvanterdenCikisTarihi | Envanterden çıkış tarihi |
| EldenCikarmaTipi | Atıldı, satıldı vb. |
| EldenCikarmaAciklamasi | Elden çıkarma açıklaması |
| SatilanKisiVeyaKurum | Satıldıysa alıcı bilgisi |
| AktifMi | Kayıt aktiflik durumu |
| ToplamVarligaDahilMi | Toplam varlık hesaplamasına dahil olup olmadığı |
| OlusturulmaTarihi | Kaydın sisteme ilk eklendiği tarih |
| GuncellenmeTarihi | Kaydın son değiştirildiği tarih |

Kurallar:

- `SeriNumarasi` veya `AssetTag` alanlarından en az biri zorunludur.
- Aktif zimmeti olan cihaz tekrar zimmetlenemez.
- Arızalı cihaz toplam varlıktan düşmez, yalnızca kullanılabilir stoktan düşer.
- Hurda/ıskarta cihaz depoda bekliyorsa toplam varlık içinde kalır.
- Hurda/ıskarta cihaz atıldı veya satıldıysa toplam varlık içinde gösterilmez.

### SarfMalzeme

| Alan | Açıklama |
| --- | --- |
| Id | Sarf malzeme benzersiz kimliği |
| Ad | Sarf malzeme adı |
| KategoriId | Kategori ilişkisi |
| LokasyonId | Lokasyon ilişkisi |
| EldekiMiktar | Kullanılabilir miktar |
| KritikStokSeviyesi | Minimum stok seviyesi |
| Birim | Sarf malzemenin takip edildiği ölçü birimi; örnek: Adet, Paket, Kutu, Metre |
| AktifMi | Kayıt aktiflik durumu |
| OlusturulmaTarihi | Kaydın sisteme ilk eklendiği tarih |
| GuncellenmeTarihi | Kaydın son değiştirildiği tarih |

Kurallar:

- Sarf malzemeleri adet bazlı takip edilir.
- Kullanılabilir stok `EldekiMiktar` alanından okunur.
- Minimum stok seviyesi kategori bazlı tanımlanır.

### Kategori

| Alan | Açıklama |
| --- | --- |
| Id | Kategori benzersiz kimliği |
| Ad | Kategori adı |
| UstKategoriId | Alt kategori desteği |
| KritikStokSeviyesi | Varsayılan kategori bazlı kritik stok seviyesi |
| AktifMi | Kategori aktiflik durumu |

### Lokasyon

| Alan | Açıklama |
| --- | --- |
| Id | Lokasyon benzersiz kimliği |
| Ad | Lokasyon adı |
| UstLokasyonId | Hiyerarşik lokasyonda bağlı olunan üst lokasyon |
| AktifMi | Lokasyon aktiflik durumu |

### StokHareketi

| Alan | Açıklama |
| --- | --- |
| Id | Stok hareketi benzersiz kimliği |
| CihazId | Cihaz ilişkisi |
| SarfMalzemeId | Sarf malzeme ilişkisi |
| HareketTipi | Giriş, çıkış, düzeltme vb. |
| Neden | Arıza, kayıp, çalınma, manuel çıkış vb. |
| Miktar | Sarf malzeme için miktar |
| Aciklama | İşlem açıklaması |
| OlusturanKullaniciId | İşlemi yapan kullanıcı |
| OlusturulmaTarihi | İşlem tarihi |

StokHareketi; cihaz veya sarf malzeme üzerinde gerçekleşen stok etkili işlemlerin kayıt altına alınmasını ifade eder. Manuel stok çıkışı, arıza, çalınma, kaybolma, hurda/ıskarta ve sayım düzeltmesi gibi işlemler bu kayıt üzerinden izlenir.

### KritikStokKurali

| Alan | Açıklama |
| --- | --- |
| Id | Kritik stok kuralı benzersiz kimliği |
| LokasyonId | Kuralın geçerli olduğu lokasyon |
| KategoriId | Kuralın geçerli olduğu kategori |
| CihazModeli | Kuralın geçerli olduğu cihaz modeli |
| KritikStokSeviyesi | Minimum stok seviyesi |
| AktifMi | Kural aktiflik durumu |

Kurallar:

- Kritik stok seviyesi lokasyon-cihaz modeli ve lokasyon-kategori kırılımında takip edilir.
- Cihaz modeli boş bırakılırsa kural lokasyon-kategori bazlı yorumlanır.
- Cihaz modeli doluysa kural lokasyon-cihaz modeli bazlı yorumlanır.

## 4. ZimmetServisi Veri Modeli

### Zimmet

| Alan | Açıklama |
| --- | --- |
| Id | Zimmet benzersiz kimliği |
| CihazId | Zimmetlenen cihaz |
| CihazAd | Zimmet anındaki cihaz görüntü adı |
| CihazAssetTag | Zimmet anındaki asset tag |
| CihazSeriNumarasi | Zimmet anındaki seri numarası |
| PersonelId | Zimmet alan personel |
| PersonelAdSoyad | Zimmet anındaki personel adı soyadı |
| PersonelEmail | Zimmet anındaki personel e-posta adresi |
| ZimmetTarihi | Zimmet tarihi |
| ZimmetleyenKullaniciId | Teslim eden kullanıcı |
| IadeTarihi | İade tarihi |
| IadeAlanKullaniciId | İade alan kullanıcı |
| Durum | Aktif, iade sürecinde, iade edildi |
| IadeKontrolDurumu | Fiziki kontrol sonucu |
| IadeKontroluYapanKullaniciId | Fiziki kontrolü yapan kullanıcı |
| IadeNotu | İade veya hasar notu |
| OlusturulmaTarihi | Kayıt oluşturulma tarihi |
| GuncellenmeTarihi | Kayıt güncellenme tarihi |

Kurallar:

- Bir cihaz aynı anda yalnızca bir aktif zimmete sahip olabilir.
- Zimmet geçmişi silinmez.
- İade sürecindeki cihaz tekrar zimmetlenemez.
- `Aktif` ve `IadeSurecinde` açık zimmet kabul edilir.
- Zimmet kaydı personel ve cihazın atama anındaki görüntü bilgilerini de taşır; böylece personel kullanıcısı kendi zimmetlerini başka servislerden ek okuma yapmadan görebilir.
- Faz 5'te zimmet ve iade fotoğrafları uygulanmamıştır.

## 5. MongoDB Audit Log Modeli

### AuditKaydi

| Alan | Açıklama |
| --- | --- |
| Id | MongoDB doküman kimliği |
| EventId | Event benzersiz kimliği |
| EventAdi | Event adı |
| IslemTipi | Event veya CRUD ayrımı |
| KaynakServis | Eventi üreten servis |
| CorrelationId | Servisler arası takip kimliği |
| KullaniciId | İşlemi yapan kullanıcı |
| OccurredAt | Event oluşma zamanı |
| Payload | JSON event içeriği |

Kurallar:

- Audit log kapsamında hem DotNetCore.CAP üzerinden gelen RabbitMQ eventleri hem de CRUD işlemleri kaydedilir.

## 6. ER Diyagramı İçin Notlar

ER diyagramı hazırlanırken servis sınırları ayrı gruplar olarak çizilmelidir:

- KimlikVePersonelServisi tabloları
- EnvanterServisi tabloları
- ZimmetServisi tabloları
- CAP Outbox tabloları: `cap_kimlik`, `cap_envanter`, `cap_zimmet`
- MongoDB audit dokümanı

Mikroservis mimarisinde her servis kendi veritabanına sahip olduğu için servisler arası ilişkiler fiziksel foreign key gibi değil, servisler arası referans id olarak gösterilmelidir.

CAP Outbox tabloları domain tablosu değildir; event üretici servislerin güvenilir yayın altyapısıdır. İş verisi ve event kaydı aynı transaction içinde yazılır, CAP daha sonra RabbitMQ `inventory.events` exchange'ine yayınlar.
