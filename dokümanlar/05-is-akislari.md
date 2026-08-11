# İş Akışları Dokümanı

## 1. Zimmet Oluşturma Akışı

Amaç:

- Uygun durumdaki seri numaralı bir cihazın aktif bir personele zimmetlenmesi.

Aktör:

- Admin veya ITPersoneli

Akış:

1. Kullanıcı zimmet oluşturma ekranından personel ve cihaz seçer.
2. Sistem personelin aktif olup olmadığını kontrol eder.
3. Personel işten ayrılmış veya pasifse işlem durdurulur.
4. Sistem cihazın varlığını ve durumunu kontrol eder.
5. Cihaz `Kullanilabilir` değilse işlem durdurulur.
6. Cihazın aktif zimmet kaydı olup olmadığı kontrol edilir.
7. Aktif zimmet yoksa zimmet kaydı oluşturulur.
8. Teslim eden kullanıcı bilgisi zorunlu olarak kaydedilir.
9. Cihaz durumu EnvanterServisi cihaz durum hareketi üzerinden `Zimmetli` yapılır.
10. ZimmetServisi aynı transaction içinde CAP Outbox kaydı oluşturur ve `zimmet.olusturuldu` eventini yayınlar.

Başarısızlık durumları:

- Personel bulunamaz.
- Personel aktif değildir.
- Personel işten ayrılmıştır.
- Cihaz bulunamaz.
- Cihaz zimmetlenebilir durumda değildir.
- Cihaz zaten aktif zimmettedir.

## 2. Zimmet İade ve Fiziki Kontrol Akışı

Amaç:

- Personele zimmetli cihazın iade alınması ve fiziki kontrol sonucuna göre yeni durumunun belirlenmesi.

Aktör:

- Admin veya ITPersoneli

Akış:

1. Kullanıcı aktif zimmet kaydını seçer.
2. İade işlemi başlatılır.
3. Zimmet kaydı iade sürecine alınır.
4. Cihaz durumu `Incelemede` yapılır.
5. Fiziki kontrol yapılır ve kontrolü yapan kullanıcı kaydedilir.
6. Cihaz sağlamsa cihaz durumu `Kullanilabilir` yapılır.
7. Cihaz arızalıysa cihaz durumu `Bakimda` yapılır.
8. Cihaz ağır hasarlı veya kullanılamaz durumdaysa cihaz durumu `HurdaIskarta` yapılır.
9. Hasarlı teslim alındıysa cihaz durumu `HasarliTeslimAlindi` yapılır ve iade notu girilir.
10. Zimmet kaydı iade edildi olarak kapatılır.
11. İade alma aşamasında `zimmet.iade-alindi` ve `cihaz.kontrole-alindi` eventleri yayınlanır.
12. İade kontrolü tamamlandığında `zimmet.iade-edildi` eventi yayınlanır.
13. Hasarlı teslim alınan cihazlarda ayrıca `cihaz.hasarli-teslim-alindi` eventi yayınlanır.

İş kuralları:

- `Incelemede` durumundaki cihaz tekrar zimmetlenemez.
- Hasarlı teslim alınan cihazda not tutulmalıdır.
- Zimmet ve iade fotoğrafları Faz 5 kapsamından çıkarılmıştır.
- Hasarlı teslim alınan cihaz için ayrı bir bakım süreci izlenmez.
- Cihaz bakımdan geldikten sonra fiziki test ile durumu manuel olarak güncellenebilir.

## 3. Personel İşten Ayrılma Akışı

Amaç:

- İşten ayrılan personelin sistem erişimini kapatmak ve aktif zimmetlerini görünür hale getirmek.

Aktör:

- Admin veya ITPersoneli

Akış:

1. Kullanıcı personel kaydını seçer.
2. Personel işten ayrıldı olarak işaretlenir.
3. `Personel.Durum = IstenAyrildi` yapılır.
4. `Personel.IstenAyrilisTarihi` kaydedilir.
5. İlgili kullanıcı hesabı pasifleştirilir.
6. Personel sisteme giriş yapamaz hale gelir.
7. `PersonelIstenAyrildi` eventi yayınlanır.
8. ZimmetServisi personelin aktif zimmetlerini kontrol eder.
9. Aktif zimmet varsa "iade bekliyor" durumu üretilir.
10. DenetimKaydiServisi audit log kaydı oluşturur.

İş kuralları:

- İşten ayrılan personel silinmez.
- İşten ayrılmış personele yeni zimmet oluşturulamaz.
- Aktif zimmet varsa işten ayrılma engellenmez; durum görünür hale getirilir.
- Departman sorumlusu işten ayrılırsa yeni sorumlu atanmalıdır.

## 4. Manuel Stok Çıkışı Akışı

Amaç:

- Zimmetleme dışındaki fiziksel stok azalma nedenlerini kayıt altına almak.

Aktör:

- Admin veya ITPersoneli

Akış:

1. Kullanıcı stok çıkışı yapılacak cihazı veya sarf malzemeyi seçer.
2. Stok çıkış nedeni seçilir.
3. Açıklama girilir.
4. Seri numaralı cihaz için cihaz durumu uygun yeni duruma alınır.
5. Sarf malzeme için `EldekiMiktar` azaltılır.
6. Cihaz için durum değiştiyse `cihaz.durumu-degisti` eventi yayınlanır.
7. Kritik stok kontrolü yapılır.
8. Kritik seviyeye düşüldüyse `stok.kritik-seviyeye-dusuldu` eventi yayınlanır.
9. Audit log kaydı Faz 7 DenetimKaydiServisi ile oluşur.
10. SignalR bildirimi Faz 9'da yalnızca kritik stok seviyesi altına düşüldüyse gönderilir.

Stok çıkış nedenleri:

- Manuel stok çıkışı
- Arıza
- Çalınma
- Kaybolma
- Hurda veya ıskartaya ayrılma

## 5. Kritik Stok Bildirim Akışı

Amaç:

- Kullanılabilir stok kritik seviyenin altına düştüğünde IT ekibini bilgilendirmek.

Akış:

1. Stok hareketi gerçekleşir.
2. EnvanterServisi ilgili kategori için kullanılabilir stok değerini hesaplar.
3. Seri numaralı cihazlarda kullanılabilir stok cihaz durumlarından hesaplanır.
4. Sarf malzemelerinde kullanılabilir stok `EldekiMiktar` alanından okunur.
5. Hesaplanan değer kritik stok seviyesinin altındaysa event üretilir.
6. CAP Outbox kaydı oluşturulur ve `stok.kritik-seviyeye-dusuldu` eventi RabbitMQ'ya yayınlanır.
7. DenetimKaydiServisi Faz 7'de event kaydını MongoDB'ye yazacaktır.
8. BildirimServisi Faz 9'da SignalR ile kritik stok bildirimi üretecektir.

## 6. Hurda / Iskarta / Elden Çıkarma Akışı

Amaç:

- Cihazın hurda veya ıskarta durumuna alınması ve toplam varlık hesabındaki etkisinin belirlenmesi.

Akış:

1. Kullanıcı cihazı seçer.
2. Hurda/ıskarta nedeni girilir.
3. Cihaz depoda bekliyorsa `ToplamVarligaDahilMi = true` kalır.
4. Cihaz atıldı veya satıldıysa `ToplamVarligaDahilMi = false` yapılır.
5. `EnvanterdenCikisTarihi` girilir.
6. Satış varsa `SatilanKisiVeyaKurum` bilgisi girilir.
7. Cihaz durumu `HurdaIskarta` yapılır.
8. `cihaz.durumu-degisti` eventi yayınlanır.
9. Audit log kaydı Faz 7 DenetimKaydiServisi ile oluşur.

## 7. Görsele Dönüştürme Notları

## 7. Faz 7 Denetim Kaydı Akışı

Başarılı domain eventleri:

1. Kaynak servis iş verisini PostgreSQL'e yazar.
2. CAP outbox kaydı aynı işlem kapsamında oluşur.
3. CAP event'i RabbitMQ `inventory.events` exchange'ine yayınlar.
4. DenetimKaydiServisi ilgili event'i tüketir.
5. Event payload'ı MongoDB `DenetimKayitlari` koleksiyonuna `Event` kayıt türüyle yazılır.

Başarılı CRUD/mutasyon işlemleri:

1. Kaynak servis HTTP isteğini işler.
2. İşlem başarılı dönerse global audit filter devreye girer.
3. Filter, işlem özetini `POST /api/denetim-kayitlari/crud` endpointine gönderir.
4. DenetimKaydiServisi kaydı MongoDB'ye `Crud` kayıt türüyle yazar.
5. Denetim çağrısı başarısız olsa bile ana işlem sonucu değiştirilmez.

## 8. Görsele Dönüştürme Notları

Bu dokümandaki akışlar draw.io veya PlantUML ile activity diagram olarak çizilebilir.

Önerilen görseller:

- Zimmet oluşturma activity diagram
- Zimmet iade activity diagram
- Personel işten ayrılma activity diagram
- Kritik stok bildirim activity diagram
- Hurda/ıskarta elden çıkarma activity diagram
