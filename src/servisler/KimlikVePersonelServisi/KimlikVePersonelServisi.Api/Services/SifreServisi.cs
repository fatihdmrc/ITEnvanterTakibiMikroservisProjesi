using System.Security.Cryptography;

namespace KimlikVePersonelServisi.Api.Services;

public interface ISifreServisi
{
    bool SifreKurallarinaUygunMu(string sifre, out string hata);
    string HashOlustur(string sifre);
    bool Dogrula(string sifre, string sifreHash);
}

public sealed class SifreServisi : ISifreServisi
{
    private const int SaltBoyutu = 16;
    private const int HashBoyutu = 32;
    private const int IterasyonSayisi = 100_000;

    public bool SifreKurallarinaUygunMu(string sifre, out string hata)
    {
        if (sifre.Length is < 8 or > 64)
        {
            hata = "Şifre minimum 8, maksimum 64 karakter olmalıdır.";
            return false;
        }

        if (!sifre.Any(char.IsDigit))
        {
            hata = "Şifrede en az bir rakam bulunmalıdır.";
            return false;
        }

        if (!sifre.Any(char.IsUpper))
        {
            hata = "Şifrede en az bir büyük harf bulunmalıdır.";
            return false;
        }

        if (!sifre.Any(char.IsLower))
        {
            hata = "Şifrede en az bir küçük harf bulunmalıdır.";
            return false;
        }

        if (!sifre.Any(karakter => !char.IsLetterOrDigit(karakter)))
        {
            hata = "Şifrede en az bir sembol bulunmalıdır.";
            return false;
        }

        hata = string.Empty;
        return true;
    }

    public string HashOlustur(string sifre)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltBoyutu);
        var hash = Rfc2898DeriveBytes.Pbkdf2(sifre, salt, IterasyonSayisi, HashAlgorithmName.SHA256, HashBoyutu);

        // Format bilgisini hash ile birlikte tutuyoruz; böylece ileride algoritma değişirse doğrulama yönetilebilir.
        return $"PBKDF2-SHA256.{IterasyonSayisi}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Dogrula(string sifre, string sifreHash)
    {
        var parcalar = sifreHash.Split('.');
        if (parcalar.Length != 4 || parcalar[0] != "PBKDF2-SHA256")
        {
            return false;
        }

        var iterasyonSayisi = int.Parse(parcalar[1]);
        var salt = Convert.FromBase64String(parcalar[2]);
        var beklenenHash = Convert.FromBase64String(parcalar[3]);
        var hesaplananHash = Rfc2898DeriveBytes.Pbkdf2(sifre, salt, iterasyonSayisi, HashAlgorithmName.SHA256, beklenenHash.Length);

        return CryptographicOperations.FixedTimeEquals(hesaplananHash, beklenenHash);
    }
}
