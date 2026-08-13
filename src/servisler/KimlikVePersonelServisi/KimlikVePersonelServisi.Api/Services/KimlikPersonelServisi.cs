using KimlikVePersonelServisi.Api.Contracts.Departmanlar;
using KimlikVePersonelServisi.Api.Contracts.Events;
using KimlikVePersonelServisi.Api.Contracts.Kimlik;
using KimlikVePersonelServisi.Api.Contracts.Kullanicilar;
using KimlikVePersonelServisi.Api.Contracts.Personeller;
using KimlikVePersonelServisi.Api.Data;
using KimlikVePersonelServisi.Api.Domain.Entities;
using KimlikVePersonelServisi.Api.Domain.Enums;
using KimlikVePersonelServisi.Api.Repositories;
using KimlikVePersonelServisi.Api.Sabitler;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KimlikVePersonelServisi.Api.Services;

public sealed class KimlikPersonelServisi(
    KimlikPersonelDbContext dbContext,
    IDepartmanRepository departmanRepository,
    IPersonelRepository personelRepository,
    UserManager<UygulamaKullanici> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    SignInManager<UygulamaKullanici> signInManager,
    ITokenServisi tokenServisi,
    ICapPublisher capPublisher) : IKimlikPersonelServisi
{
    // Servis sınıfı HTTP'den bağımsız iş kurallarını taşır; endpointler yalnızca bu servisi çağırır.
    public async Task<IReadOnlyCollection<DepartmanCevap>> DepartmanlariListeleAsync(CancellationToken cancellationToken = default)
    {
        var departmanlar = await departmanRepository.ListeleAsync(cancellationToken);
        return departmanlar.Select(DepartmanCevabaDonustur).ToList();
    }

    public async Task<DepartmanCevap?> DepartmanGetirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var departman = await departmanRepository.GetirAsync(id, cancellationToken);
        return departman is null ? null : DepartmanCevabaDonustur(departman);
    }

    public async Task<Sonuc<DepartmanCevap>> DepartmanOlusturAsync(DepartmanOlusturIstek istek, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(istek.Ad))
        {
            return Sonuc<DepartmanCevap>.Basarisiz(KimlikPersonelMesajlari.DepartmanAdiZorunlu);
        }

        var departmanAdi = istek.Ad.Trim();
        if (await departmanRepository.AdKullaniliyorMuAsync(departmanAdi, cancellationToken: cancellationToken))
        {
            return Sonuc<DepartmanCevap>.Basarisiz(KimlikPersonelMesajlari.DepartmanAdiKullaniliyor);
        }

        if (istek.SorumluPersonelId.HasValue && !await personelRepository.VarMiAsync(istek.SorumluPersonelId.Value, cancellationToken))
        {
            return Sonuc<DepartmanCevap>.Basarisiz(KimlikPersonelMesajlari.SorumluPersonelBulunamadi);
        }

        var departman = new Departman
        {
            Ad = departmanAdi,
            SorumluPersonelId = istek.SorumluPersonelId
        };

        departmanRepository.Ekle(departman);
        await departmanRepository.KaydetAsync(cancellationToken);

        return Sonuc<DepartmanCevap>.Basarili(DepartmanCevabaDonustur(departman));
    }

    public async Task<Sonuc<DepartmanCevap>> DepartmanGuncelleAsync(Guid id, DepartmanGuncelleIstek istek, CancellationToken cancellationToken = default)
    {
        var departman = await departmanRepository.GetirAsync(id, cancellationToken);
        if (departman is null)
        {
            return Sonuc<DepartmanCevap>.Basarisiz(KimlikPersonelMesajlari.DepartmanBulunamadi);
        }

        if (string.IsNullOrWhiteSpace(istek.Ad))
        {
            return Sonuc<DepartmanCevap>.Basarisiz(KimlikPersonelMesajlari.DepartmanAdiZorunlu);
        }

        var departmanAdi = istek.Ad.Trim();
        if (await departmanRepository.AdKullaniliyorMuAsync(departmanAdi, id, cancellationToken))
        {
            return Sonuc<DepartmanCevap>.Basarisiz(KimlikPersonelMesajlari.DepartmanAdiKullaniliyor);
        }

        if (istek.SorumluPersonelId.HasValue && !await personelRepository.VarMiAsync(istek.SorumluPersonelId.Value, cancellationToken))
        {
            return Sonuc<DepartmanCevap>.Basarisiz(KimlikPersonelMesajlari.SorumluPersonelBulunamadi);
        }

        departman.Ad = departmanAdi;
        departman.SorumluPersonelId = istek.SorumluPersonelId;
        departman.AktifMi = istek.AktifMi;

        await departmanRepository.KaydetAsync(cancellationToken);
        return Sonuc<DepartmanCevap>.Basarili(DepartmanCevabaDonustur(departman));
    }

    public async Task<IReadOnlyCollection<PersonelCevap>> PersonelleriListeleAsync(CancellationToken cancellationToken = default)
    {
        var personeller = await personelRepository.ListeleAsync(cancellationToken);
        return personeller.Select(PersonelCevabaDonustur).ToList();
    }

    public async Task<PersonelCevap?> PersonelGetirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var personel = await personelRepository.GetirAsync(id, cancellationToken);
        return personel is null ? null : PersonelCevabaDonustur(personel);
    }

    public async Task<Sonuc<PersonelCevap>> PersonelOlusturAsync(PersonelOlusturIstek istek, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(istek.Ad) || string.IsNullOrWhiteSpace(istek.Soyad))
        {
            return Sonuc<PersonelCevap>.Basarisiz(KimlikPersonelMesajlari.PersonelAdSoyadZorunlu);
        }

        if (string.IsNullOrWhiteSpace(istek.Email) || !istek.Email.Contains('@'))
        {
            return Sonuc<PersonelCevap>.Basarisiz(KimlikPersonelMesajlari.GecerliEmailZorunlu);
        }

        if (!await departmanRepository.AktifVarMiAsync(istek.DepartmanId, cancellationToken))
        {
            return Sonuc<PersonelCevap>.Basarisiz(KimlikPersonelMesajlari.AktifDepartmanBulunamadi);
        }

        var email = istek.Email.Trim();
        if (await personelRepository.EmailKullaniliyorMuAsync(email, cancellationToken: cancellationToken))
        {
            return Sonuc<PersonelCevap>.Basarisiz(KimlikPersonelMesajlari.PersonelEmailKullaniliyor);
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
        await personelRepository.KaydetAsync(cancellationToken);

        return Sonuc<PersonelCevap>.Basarili(PersonelCevabaDonustur(personel));
    }

    public async Task<Sonuc<PersonelCevap>> PersonelGuncelleAsync(Guid id, PersonelGuncelleIstek istek, CancellationToken cancellationToken = default)
    {
        var personel = await personelRepository.GetirAsync(id, cancellationToken);
        if (personel is null)
        {
            return Sonuc<PersonelCevap>.Basarisiz(KimlikPersonelMesajlari.PersonelBulunamadi);
        }

        var istenAyrildiEventiYayinlanacakMi = personel.Durum != PersonelDurumu.IstenAyrildi
            && istek.Durum == PersonelDurumu.IstenAyrildi;

        if (!await departmanRepository.VarMiAsync(istek.DepartmanId, cancellationToken))
        {
            return Sonuc<PersonelCevap>.Basarisiz(KimlikPersonelMesajlari.DepartmanBulunamadi);
        }

        var email = istek.Email.Trim();
        if (await personelRepository.EmailKullaniliyorMuAsync(email, id, cancellationToken))
        {
            return Sonuc<PersonelCevap>.Basarisiz(KimlikPersonelMesajlari.PersonelEmailBaskaPersonelde);
        }

        personel.Ad = istek.Ad.Trim();
        personel.Soyad = istek.Soyad.Trim();
        personel.Email = email;
        personel.DepartmanId = istek.DepartmanId;
        personel.Unvan = istek.Unvan.Trim();
        personel.DepartmanSorumlusuMu = istek.DepartmanSorumlusuMu;
        personel.Durum = istek.Durum;
        personel.AktifMi = istek.AktifMi;

        using var transaction = dbContext.Database.BeginTransaction(capPublisher, autoCommit: false);

        if (istek.Durum == PersonelDurumu.IstenAyrildi)
        {
            personel.AktifMi = false;
            personel.IstenAyrilisTarihi ??= DateOnly.FromDateTime(DateTime.UtcNow);
            await PersonelHesaplariniPasiflestirAsync(personel.Id, cancellationToken);
        }

        await personelRepository.KaydetAsync(cancellationToken);
        if (istenAyrildiEventiYayinlanacakMi)
        {
            await PersonelIstenAyrildiEventiYayinlaAsync(personel, cancellationToken);
        }

        transaction.Commit();
        return Sonuc<PersonelCevap>.Basarili(PersonelCevabaDonustur(personel));
    }

    public async Task<Sonuc<PersonelCevap>> PersoneliIstenAyrildiYapAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var personel = await personelRepository.GetirAsync(id, cancellationToken);
        if (personel is null)
        {
            return Sonuc<PersonelCevap>.Basarisiz(KimlikPersonelMesajlari.PersonelBulunamadi);
        }

        var istenAyrildiEventiYayinlanacakMi = personel.Durum != PersonelDurumu.IstenAyrildi;
        using var transaction = dbContext.Database.BeginTransaction(capPublisher, autoCommit: false);

        personel.Durum = PersonelDurumu.IstenAyrildi;
        personel.AktifMi = false;
        personel.IstenAyrilisTarihi = DateOnly.FromDateTime(DateTime.UtcNow);

        // Personel işten ayrıldığında bağlı kullanıcı hesabı da giriş yapamasın diye pasifleştirilir.
        await PersonelHesaplariniPasiflestirAsync(personel.Id, cancellationToken);

        await personelRepository.KaydetAsync(cancellationToken);
        if (istenAyrildiEventiYayinlanacakMi)
        {
            await PersonelIstenAyrildiEventiYayinlaAsync(personel, cancellationToken);
        }

        transaction.Commit();
        return Sonuc<PersonelCevap>.Basarili(PersonelCevabaDonustur(personel));
    }

    public async Task<IReadOnlyCollection<KullaniciCevap>> KullanicilariListeleAsync(CancellationToken cancellationToken = default)
    {
        var kullanicilar = await userManager.Users
            .AsNoTracking()
            .OrderBy(kullanici => kullanici.UserName)
            .ToListAsync(cancellationToken);

        var cevaplar = new List<KullaniciCevap>();
        foreach (var kullanici in kullanicilar)
        {
            cevaplar.Add(await KullaniciCevabaDonusturAsync(kullanici));
        }

        return cevaplar;
    }

    public async Task<Sonuc<KullaniciCevap>> KullaniciOlusturAsync(KullaniciOlusturIstek istek, CancellationToken cancellationToken = default)
    {
        var personel = await personelRepository.GetirAsync(istek.PersonelId, cancellationToken);
        if (personel is null)
        {
            return Sonuc<KullaniciCevap>.Basarisiz(KimlikPersonelMesajlari.KullaniciIcinPersonelZorunlu);
        }

        if (personel.Durum == PersonelDurumu.IstenAyrildi || !personel.AktifMi)
        {
            return Sonuc<KullaniciCevap>.Basarisiz(KimlikPersonelMesajlari.PasifPersoneleKullaniciOlusturulamaz);
        }

        var kullaniciAdi = istek.KullaniciAdi.Trim();
        var rolAdi = istek.Rol.ToString();
        if (!await roleManager.RoleExistsAsync(rolAdi))
        {
            return Sonuc<KullaniciCevap>.Basarisiz(KimlikPersonelMesajlari.GecerliRolZorunlu);
        }

        if (await userManager.FindByNameAsync(kullaniciAdi) is not null)
        {
            return Sonuc<KullaniciCevap>.Basarisiz(KimlikPersonelMesajlari.KullaniciAdiKullaniliyor);
        }

        if (await userManager.Users.AnyAsync(kullanici => kullanici.PersonelId == istek.PersonelId, cancellationToken))
        {
            return Sonuc<KullaniciCevap>.Basarisiz(KimlikPersonelMesajlari.PersonelinKullaniciHesabiVar);
        }

        var kullanici = new UygulamaKullanici
        {
            UserName = kullaniciAdi,
            NormalizedUserName = kullaniciAdi.ToUpperInvariant(),
            PersonelId = istek.PersonelId
        };

        var kullaniciSonucu = await userManager.CreateAsync(kullanici, istek.Sifre);
        if (!kullaniciSonucu.Succeeded)
        {
            return Sonuc<KullaniciCevap>.Basarisiz(IdentityHatalariniBirlestir(kullaniciSonucu));
        }

        var rolSonucu = await userManager.AddToRoleAsync(kullanici, rolAdi);
        if (!rolSonucu.Succeeded)
        {
            return Sonuc<KullaniciCevap>.Basarisiz(IdentityHatalariniBirlestir(rolSonucu));
        }

        return Sonuc<KullaniciCevap>.Basarili(await KullaniciCevabaDonusturAsync(kullanici));
    }

    public async Task<Sonuc<GirisCevap>> GirisYapAsync(GirisIstek istek, CancellationToken cancellationToken = default)
    {
        var kullaniciAdi = istek.KullaniciAdi.Trim();
        var kullanici = await userManager.FindByNameAsync(kullaniciAdi);
        if (kullanici is null || !kullanici.AktifMi)
        {
            return Sonuc<GirisCevap>.Basarisiz(KimlikPersonelMesajlari.KullaniciAdiVeyaSifreHatali);
        }

        var sifreSonucu = await signInManager.CheckPasswordSignInAsync(kullanici, istek.Sifre, lockoutOnFailure: true);
        if (!sifreSonucu.Succeeded)
        {
            return Sonuc<GirisCevap>.Basarisiz(KimlikPersonelMesajlari.KullaniciAdiVeyaSifreHatali);
        }

        var personel = await personelRepository.GetirAsync(kullanici.PersonelId, cancellationToken);
        if (personel is null || personel.Durum == PersonelDurumu.IstenAyrildi || !personel.AktifMi)
        {
            return Sonuc<GirisCevap>.Basarisiz(KimlikPersonelMesajlari.PersonelAktifDegilGirisYapilamaz);
        }

        var rol = (await userManager.GetRolesAsync(kullanici)).FirstOrDefault()
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

    private async Task PersonelHesaplariniPasiflestirAsync(Guid personelId, CancellationToken cancellationToken = default)
    {
        var kullanicilar = await userManager.Users
            .Where(kullanici => kullanici.PersonelId == personelId)
            .ToListAsync(cancellationToken);

        foreach (var kullanici in kullanicilar)
        {
            kullanici.AktifMi = false;
            await userManager.UpdateAsync(kullanici);
        }
    }

    private async Task<KullaniciCevap> KullaniciCevabaDonusturAsync(UygulamaKullanici kullanici)
    {
        var rol = (await userManager.GetRolesAsync(kullanici)).FirstOrDefault()
            ?? KullaniciRolu.PersonelKullanicisi.ToString();

        return new(
            kullanici.Id,
            kullanici.UserName ?? string.Empty,
            Enum.Parse<KullaniciRolu>(rol),
            kullanici.PersonelId,
            kullanici.AktifMi);
    }

    private Task PersonelIstenAyrildiEventiYayinlaAsync(Personel personel, CancellationToken cancellationToken)
    {
        var eventPayload = new PersonelIstenAyrildiEvent(
            Guid.NewGuid(),
            personel.Id,
            $"{personel.Ad} {personel.Soyad}".Trim(),
            personel.Email,
            personel.DepartmanId,
            personel.IstenAyrilisTarihi ?? DateOnly.FromDateTime(DateTime.UtcNow),
            DateTime.UtcNow);

        return capPublisher.PublishAsync(EventAdlari.PersonelIstenAyrildi, eventPayload, cancellationToken: cancellationToken);
    }

    private static string IdentityHatalariniBirlestir(IdentityResult sonuc)
    {
        return string.Join(" ", sonuc.Errors.Select(hata => hata.Description));
    }
}
