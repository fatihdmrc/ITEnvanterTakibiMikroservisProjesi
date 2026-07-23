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
9. Varsa cihaz fotoğrafı zimmet kaydına eklenir.
10. Cihaz durumu `Zimmetli` yapılır.
11. `ZimmetOlusturuldu` eventi yayınlanır.
12. Audit log kaydı oluşur.

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
5. `CihazKontroleAlindi` eventi yayınlanır.
6. Fiziki kontrol yapılır ve kontrolü yapan kullanıcı kaydedilir.
7. Cihaz sağlamsa cihaz durumu `Kullanilabilir` yapılır.
8. Cihaz arızalıysa cihaz durumu `Bakimda` yapılır.
9. Cihaz ağır hasarlı veya kullanılamaz durumdaysa cihaz durumu `HurdaIskarta` yapılır.
10. Hasarlı teslim alındıysa iade notu girilir.
11. İade sürecinde birden fazla fotoğraf eklenebilir.
12. Zimmet kaydı iade edildi olarak kapatılır.
13. `ZimmetIadeEdildi` eventi yayınlanır.
14. Audit log kaydı oluşur.

İş kuralları:

- `Incelemede` durumundaki cihaz tekrar zimmetlenemez.
- Hasarlı teslim alınan cihazda not tutulmalıdır.
- İade fotoğrafları zimmet geçmişiyle birlikte korunmalıdır.
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
6. `StokAzaldi` eventi yayınlanır.
7. Kritik stok kontrolü yapılır.
8. Kritik seviyeye düşüldüyse `KritikStokSeviyesineDusuldu` eventi yayınlanır.
9. Audit log kaydı oluşur.
10. SignalR bildirimi yalnızca kritik stok seviyesi altına düşüldüyse gönderilir.

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
6. `KritikStokSeviyesineDusuldu` eventi RabbitMQ'ya yayınlanır.
7. DenetimKaydiServisi event kaydını MongoDB'ye yazar.
8. BildirimServisi SignalR ile kritik stok bildirimi üretir.

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
8. `CihazHurdayaAyrildi` eventi yayınlanır.
9. Audit log kaydı oluşur.

## 7. Görsele Dönüştürme Notları

Bu dokümandaki akışlar draw.io veya PlantUML ile activity diagram olarak çizilebilir.

Önerilen görseller:

- Zimmet oluşturma activity diagram
- Zimmet iade activity diagram
- Personel işten ayrılma activity diagram
- Kritik stok bildirim activity diagram
- Hurda/ıskarta elden çıkarma activity diagram
