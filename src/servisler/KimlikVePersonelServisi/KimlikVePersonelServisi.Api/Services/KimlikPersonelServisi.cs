using KimlikVePersonelServisi.Api.Contracts.Departmanlar;
using KimlikVePersonelServisi.Api.Contracts.Kimlik;
using KimlikVePersonelServisi.Api.Contracts.Kullanicilar;
using KimlikVePersonelServisi.Api.Contracts.Personeller;
using KimlikVePersonelServisi.Api.Domain.Entities;
using KimlikVePersonelServisi.Api.Domain.Enums;
using KimlikVePersonelServisi.Api.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KimlikVePersonelServisi.Api.Services;

public sealed class KimlikPersonelServisi(
    IDepartmanRepository departmanRepository,
    IPersonelRepository personelRepository,
    UserManager<UygulamaKullanici> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    SignInManager<UygulamaKullanici> signInManager,
    ITokenServisi tokenServisi) : IKimlikPersonelServisi
{
    // Servis sınıfı HTTP'den bağımsız iş kurallarını taşır; endpointler yalnızca bu servisi çağırır.
    public IReadOnlyCollection<DepartmanCevap> DepartmanlariListele()
    {
        return departmanRepository.Listele().Select(DepartmanCevabaDonustur).ToList();
    }

    public DepartmanCevap? DepartmanGetir(Guid id)
    {
        var departman = departmanRepository.Getir(id);
        return departman is null ? null : DepartmanCevabaDonustur(departman);
    }

    public Sonuc<DepartmanCevap> DepartmanOlustur(DepartmanOlusturIstek istek)
    {
        if (string.IsNullOrWhiteSpace(istek.Ad))
        {
            return Sonuc<DepartmanCevap>.Basarisiz("Departman adı zorunludur.");
        }

        var departmanAdi = istek.Ad.Trim();
        if (departmanRepository.AdKullaniliyorMu(departmanAdi))
        {
            return Sonuc<DepartmanCevap>.Basarisiz("Aynı ada sahip departman zaten var.");
        }

        if (istek.SorumluPersonelId.HasValue && !personelRepository.VarMi(istek.SorumluPersonelId.Value))
        {
            return Sonuc<DepartmanCevap>.Basarisiz("Sorumlu personel bulunamadı.");
        }

        var departman = new Departman
        {
            Ad = departmanAdi,
            SorumluPersonelId = istek.SorumluPersonelId
        };

        departmanRepository.Ekle(departman);
        departmanRepository.Kaydet();

        return Sonuc<DepartmanCevap>.Basarili(DepartmanCevabaDonustur(departman));
    }

    public Sonuc<DepartmanCevap> DepartmanGuncelle(Guid id, DepartmanGuncelleIstek istek)
    {
        var departman = departmanRepository.Getir(id);
        if (departman is null)
        {
            return Sonuc<DepartmanCevap>.Basarisiz("Departman bulunamadı.");
        }

        if (string.IsNullOrWhiteSpace(istek.Ad))
        {
            return Sonuc<DepartmanCevap>.Basarisiz("Departman adı zorunludur.");
        }

        var departmanAdi = istek.Ad.Trim();
        if (departmanRepository.AdKullaniliyorMu(departmanAdi, id))
        {
            return Sonuc<DepartmanCevap>.Basarisiz("Aynı ada sahip departman zaten var.");
        }

        if (istek.SorumluPersonelId.HasValue && !personelRepository.VarMi(istek.SorumluPersonelId.Value))
        {
            return Sonuc<DepartmanCevap>.Basarisiz("Sorumlu personel bulunamadı.");
        }

        departman.Ad = departmanAdi;
        departman.SorumluPersonelId = istek.SorumluPersonelId;
        departman.AktifMi = istek.AktifMi;

        departmanRepository.Kaydet();
        return Sonuc<DepartmanCevap>.Basarili(DepartmanCevabaDonustur(departman));
    }

    public IReadOnlyCollection<PersonelCevap> PersonelleriListele()
    {
        return personelRepository.Listele().Select(PersonelCevabaDonustur).ToList();
    }

    public PersonelCevap? PersonelGetir(Guid id)
    {
        var personel = personelRepository.Getir(id);
        return personel is null ? null : PersonelCevabaDonustur(personel);
    }

    public Sonuc<PersonelCevap> PersonelOlustur(PersonelOlusturIstek istek)
    {
        if (string.IsNullOrWhiteSpace(istek.Ad) || string.IsNullOrWhiteSpace(istek.Soyad))
        {
            return Sonuc<PersonelCevap>.Basarisiz("Personel adı ve soyadı zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(istek.Email) || !istek.Email.Contains('@'))
        {
            return Sonuc<PersonelCevap>.Basarisiz("Geçerli bir e-posta adresi girilmelidir.");
        }

        if (!departmanRepository.AktifVarMi(istek.DepartmanId))
        {
            return Sonuc<PersonelCevap>.Basarisiz("Aktif departman bulunamadı.");
        }

        var email = istek.Email.Trim();
        if (personelRepository.EmailKullaniliyorMu(email))
        {
            return Sonuc<PersonelCevap>.Basarisiz("Bu e-posta adresiyle kayıtlı personel zaten var.");
        }

        var personel = new Personel
        {
            Ad = istek.Ad.Trim(),
            Soyad = istek.Soyad.Trim(),
            Email = email,
            DepartmanId = istek.DepartmanId,
            Unvan = istek.Unvan.Trim(),
            DepartmanSorumlusuMu = istek.DepartmanSorumlusuMu,
            IseGirisTarihi = istek.IseGirisTarihi
        };

        personelRepository.Ekle(personel);
        personelRepository.Kaydet();

        return Sonuc<PersonelCevap>.Basarili(PersonelCevabaDonustur(personel));
    }

    public Sonuc<PersonelCevap> PersonelGuncelle(Guid id, PersonelGuncelleIstek istek)
    {
        var personel = personelRepository.Getir(id);
        if (personel is null)
        {
            return Sonuc<PersonelCevap>.Basarisiz("Personel bulunamadı.");
        }

        if (!departmanRepository.VarMi(istek.DepartmanId))
        {
            return Sonuc<PersonelCevap>.Basarisiz("Departman bulunamadı.");
        }

        var email = istek.Email.Trim();
        if (personelRepository.EmailKullaniliyorMu(email, id))
        {
            return Sonuc<PersonelCevap>.Basarisiz("Bu e-posta adresi başka bir personel tarafından kullanılıyor.");
        }

        personel.Ad = istek.Ad.Trim();
        personel.Soyad = istek.Soyad.Trim();
        personel.Email = email;
        personel.DepartmanId = istek.DepartmanId;
        personel.Unvan = istek.Unvan.Trim();
        personel.DepartmanSorumlusuMu = istek.DepartmanSorumlusuMu;
        personel.Durum = istek.Durum;
        personel.AktifMi = istek.AktifMi;

        if (istek.Durum == PersonelDurumu.IstenAyrildi)
        {
            personel.AktifMi = false;
            personel.IstenAyrilisTarihi ??= DateOnly.FromDateTime(DateTime.UtcNow);
            PersonelHesaplariniPasiflestir(personel.Id);
        }

        personelRepository.Kaydet();
        return Sonuc<PersonelCevap>.Basarili(PersonelCevabaDonustur(personel));
    }

    public Sonuc<PersonelCevap> PersoneliIstenAyrildiYap(Guid id)
    {
        var personel = personelRepository.Getir(id);
        if (personel is null)
        {
            return Sonuc<PersonelCevap>.Basarisiz("Personel bulunamadı.");
        }

        personel.Durum = PersonelDurumu.IstenAyrildi;
        personel.AktifMi = false;
        personel.IstenAyrilisTarihi = DateOnly.FromDateTime(DateTime.UtcNow);

        // Personel işten ayrıldığında bağlı kullanıcı hesabı da giriş yapamasın diye pasifleştirilir.
        PersonelHesaplariniPasiflestir(personel.Id);

        personelRepository.Kaydet();
        return Sonuc<PersonelCevap>.Basarili(PersonelCevabaDonustur(personel));
    }

    public IReadOnlyCollection<KullaniciCevap> KullanicilariListele()
    {
        return userManager.Users
            .AsNoTracking()
            .OrderBy(kullanici => kullanici.UserName)
            .AsEnumerable()
            .Select(KullaniciCevabaDonustur)
            .ToList();
    }

    public Sonuc<KullaniciCevap> KullaniciOlustur(KullaniciOlusturIstek istek)
    {
        var personel = personelRepository.Getir(istek.PersonelId);
        if (personel is null)
        {
            return Sonuc<KullaniciCevap>.Basarisiz("Kullanıcı oluşturmak için personel kaydı zorunludur.");
        }

        if (personel.Durum == PersonelDurumu.IstenAyrildi || !personel.AktifMi)
        {
            return Sonuc<KullaniciCevap>.Basarisiz("İşten ayrılmış veya pasif personel için kullanıcı oluşturulamaz.");
        }

        var kullaniciAdi = istek.KullaniciAdi.Trim();
        var rolAdi = istek.Rol.ToString();
        if (!roleManager.RoleExistsAsync(rolAdi).GetAwaiter().GetResult())
        {
            return Sonuc<KullaniciCevap>.Basarisiz("Geçerli bir kullanıcı rolü seçilmelidir.");
        }

        if (userManager.FindByNameAsync(kullaniciAdi).GetAwaiter().GetResult() is not null)
        {
            return Sonuc<KullaniciCevap>.Basarisiz("Bu kullanıcı adı zaten kullanılıyor.");
        }

        if (userManager.Users.Any(kullanici => kullanici.PersonelId == istek.PersonelId))
        {
            return Sonuc<KullaniciCevap>.Basarisiz("Bu personel için kullanıcı hesabı zaten oluşturulmuş.");
        }

        var kullanici = new UygulamaKullanici
        {
            UserName = kullaniciAdi,
            NormalizedUserName = kullaniciAdi.ToUpperInvariant(),
            PersonelId = istek.PersonelId
        };

        var kullaniciSonucu = userManager.CreateAsync(kullanici, istek.Sifre).GetAwaiter().GetResult();
        if (!kullaniciSonucu.Succeeded)
        {
            return Sonuc<KullaniciCevap>.Basarisiz(IdentityHatalariniBirlestir(kullaniciSonucu));
        }

        var rolSonucu = userManager.AddToRoleAsync(kullanici, rolAdi).GetAwaiter().GetResult();
        if (!rolSonucu.Succeeded)
        {
            return Sonuc<KullaniciCevap>.Basarisiz(IdentityHatalariniBirlestir(rolSonucu));
        }

        return Sonuc<KullaniciCevap>.Basarili(KullaniciCevabaDonustur(kullanici));
    }

    public Sonuc<GirisCevap> GirisYap(GirisIstek istek)
    {
        var kullaniciAdi = istek.KullaniciAdi.Trim();
        var kullanici = userManager.FindByNameAsync(kullaniciAdi).GetAwaiter().GetResult();
        if (kullanici is null || !kullanici.AktifMi)
        {
            return Sonuc<GirisCevap>.Basarisiz("Kullanıcı adı veya şifre hatalı.");
        }

        var sifreSonucu = signInManager.CheckPasswordSignInAsync(kullanici, istek.Sifre, lockoutOnFailure: true)
            .GetAwaiter()
            .GetResult();
        if (!sifreSonucu.Succeeded)
        {
            return Sonuc<GirisCevap>.Basarisiz("Kullanıcı adı veya şifre hatalı.");
        }

        var personel = personelRepository.Getir(kullanici.PersonelId);
        if (personel is null || personel.Durum == PersonelDurumu.IstenAyrildi || !personel.AktifMi)
        {
            return Sonuc<GirisCevap>.Basarisiz("Personel kaydı aktif olmadığı için giriş yapılamaz.");
        }

        var rol = userManager.GetRolesAsync(kullanici).GetAwaiter().GetResult().FirstOrDefault()
            ?? KullaniciRolu.PersonelKullanicisi.ToString();
        var tokenBilgisi = tokenServisi.TokenOlustur(kullanici, rol);
        var cevap = new GirisCevap(
            tokenBilgisi.Token,
            kullanici.Id,
            kullanici.PersonelId,
            Enum.Parse<KullaniciRolu>(rol),
            tokenBilgisi.GecerlilikZamani);

        return Sonuc<GirisCevap>.Basarili(cevap);
    }

    private static DepartmanCevap DepartmanCevabaDonustur(Departman departman)
        => new(departman.Id, departman.Ad, departman.SorumluPersonelId, departman.AktifMi);

    private static PersonelCevap PersonelCevabaDonustur(Personel personel)
        => new(
            personel.Id,
            personel.Ad,
            personel.Soyad,
            personel.Email,
            personel.DepartmanId,
            personel.Unvan,
            personel.DepartmanSorumlusuMu,
            personel.Durum,
            personel.IseGirisTarihi,
            personel.IstenAyrilisTarihi,
            personel.AktifMi);

    private void PersonelHesaplariniPasiflestir(Guid personelId)
    {
        var kullanicilar = userManager.Users.Where(kullanici => kullanici.PersonelId == personelId).ToList();
        foreach (var kullanici in kullanicilar)
        {
            kullanici.AktifMi = false;
            userManager.UpdateAsync(kullanici).GetAwaiter().GetResult();
        }
    }

    private KullaniciCevap KullaniciCevabaDonustur(UygulamaKullanici kullanici)
    {
        var rol = userManager.GetRolesAsync(kullanici).GetAwaiter().GetResult().FirstOrDefault()
            ?? KullaniciRolu.PersonelKullanicisi.ToString();

        return new(
            kullanici.Id,
            kullanici.UserName ?? string.Empty,
            Enum.Parse<KullaniciRolu>(rol),
            kullanici.PersonelId,
            kullanici.AktifMi);
    }

    private static string IdentityHatalariniBirlestir(IdentityResult sonuc)
    {
        return string.Join(" ", sonuc.Errors.Select(hata => hata.Description));
    }
}
