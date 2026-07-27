using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KimlikVePersonelServisi.Api.Domain.Entities;
using KimlikVePersonelServisi.Api.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace KimlikVePersonelServisi.Api.Services;

public interface ITokenServisi
{
    TokenBilgisi TokenOlustur(Kullanici kullanici);
}

public sealed record TokenBilgisi(string Token, DateTimeOffset GecerlilikZamani);

public sealed class JwtTokenServisi(IOptions<JwtAyarlari> jwtAyarlari) : ITokenServisi
{
    public TokenBilgisi TokenOlustur(Kullanici kullanici)
    {
        var ayarlar = jwtAyarlari.Value;
        var gecerlilikZamani = DateTimeOffset.UtcNow.AddMinutes(ayarlar.GecerlilikDakikasi);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ayarlar.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, kullanici.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, kullanici.KullaniciAdi),
            new("KullaniciId", kullanici.Id.ToString()),
            new("KullaniciAdi", kullanici.KullaniciAdi),
            new("PersonelId", kullanici.PersonelId.ToString()),
            new(ClaimTypes.Role, kullanici.Rol.ToString()),
            new("Rol", kullanici.Rol.ToString())
        };

        // Token içinde kullanıcı taşıyoruz, ileride event metadata ve audit log için aynı bilgiler kullanılacak.
        var token = new JwtSecurityToken(
            issuer: ayarlar.Issuer,
            audience: ayarlar.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: gecerlilikZamani.UtcDateTime,
            signingCredentials: credentials);

        return new TokenBilgisi(new JwtSecurityTokenHandler().WriteToken(token), gecerlilikZamani);
    }
}
