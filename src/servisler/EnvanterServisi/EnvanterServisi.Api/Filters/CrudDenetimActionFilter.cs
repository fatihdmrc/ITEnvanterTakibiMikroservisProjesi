using System.Reflection;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnvanterServisi.Api.Contracts.Denetim;
using EnvanterServisi.Api.Sabitler;
using EnvanterServisi.Api.Services.Harici;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EnvanterServisi.Api.Filters;

public sealed class CrudDenetimActionFilter(DenetimApiClient denetimApiClient) : IAsyncActionFilter
{
    private static readonly JsonSerializerOptions JsonAyarlari = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executedContext = await next();

        if (!AuditlenebilirMi(executedContext))
        {
            return;
        }

        var path = context.HttpContext.Request.Path.Value ?? string.Empty;
        var sonuc = SonucDegeriniAl(executedContext.Result);
        var routeValues = context.RouteData.Values.ToDictionary(deger => deger.Key, deger => deger.Value?.ToString());
        var actionArguments = context.ActionArguments
            .Where(argument => argument.Value is not CancellationToken)
            .ToDictionary(argument => argument.Key, argument => argument.Value);

        var istek = new CrudDenetimKaydiOlusturIstek(
            "EnvanterServisi",
            IslemTurunuBelirle(context.HttpContext.Request.Method, path),
            VarlikTurunuBelirle(path),
            DegeriOku(sonuc, "Id") ?? routeValues.GetValueOrDefault("id"),
            VarlikAdiniOku(sonuc),
            KullaniciIdGetir(context.HttpContext.User),
            RolGetir(context.HttpContext.User),
            context.HttpContext.Request.Method,
            path,
            string.Format(EnvanterMesajlari.CrudDenetimAciklamasi, context.HttpContext.Request.Method, path),
            JsonSerializer.Serialize(new { routeValues, actionArguments, sonuc }, JsonAyarlari),
            DateTime.UtcNow);

        await denetimApiClient.CrudKaydiGonderAsync(istek, BearerTokenAl(context.HttpContext.Request.Headers.Authorization.ToString()), context.HttpContext.RequestAborted);
    }

    private static bool AuditlenebilirMi(ActionExecutedContext context)
    {
        var method = context.HttpContext.Request.Method;
        if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method))
        {
            return false;
        }

        if (context.Exception is not null)
        {
            return false;
        }

        var statusCode = StatusCodeAl(context.Result);
        return statusCode is >= 200 and < 300;
    }

    private static int? StatusCodeAl(IActionResult? result)
        => result switch
        {
            ObjectResult objectResult => objectResult.StatusCode ?? (objectResult is CreatedAtActionResult ? StatusCodes.Status201Created : StatusCodes.Status200OK),
            StatusCodeResult statusCodeResult => statusCodeResult.StatusCode,
            JsonResult jsonResult => jsonResult.StatusCode ?? StatusCodes.Status200OK,
            EmptyResult => StatusCodes.Status200OK,
            _ => null
        };

    private static object? SonucDegeriniAl(IActionResult? result)
        => result switch
        {
            ObjectResult objectResult => objectResult.Value,
            JsonResult jsonResult => jsonResult.Value,
            _ => null
        };

    private static string IslemTurunuBelirle(string method, string path)
    {
        if (path.Contains("/durum-hareketleri", StringComparison.OrdinalIgnoreCase)) return "CihazDurumHareketi";
        if (path.Contains("/stok-hareketleri", StringComparison.OrdinalIgnoreCase)) return "StokHareketi";
        if (path.Contains("/kritik-kurallar", StringComparison.OrdinalIgnoreCase) && method == "POST") return "KritikStokKuraliOlustur";
        if (path.Contains("/kritik-kurallar", StringComparison.OrdinalIgnoreCase) && method == "PUT") return "KritikStokKuraliGuncelle";

        return method switch
        {
            "POST" => "Olustur",
            "PUT" => "Guncelle",
            "PATCH" => "Guncelle",
            "DELETE" => "Sil",
            _ => method
        };
    }

    private static string? VarlikTurunuBelirle(string path)
    {
        if (path.Contains("/cihazlar", StringComparison.OrdinalIgnoreCase)) return "Cihaz";
        if (path.Contains("/sarf-malzemeler", StringComparison.OrdinalIgnoreCase)) return "SarfMalzeme";
        if (path.Contains("/kategoriler", StringComparison.OrdinalIgnoreCase)) return "Kategori";
        if (path.Contains("/lokasyonlar", StringComparison.OrdinalIgnoreCase)) return "Lokasyon";
        if (path.Contains("/kritik-kurallar", StringComparison.OrdinalIgnoreCase)) return "KritikStokKurali";
        return null;
    }

    private static string? VarlikAdiniOku(object? sonuc)
        => DegeriOku(sonuc, "AssetTag")
            ?? DegeriOku(sonuc, "SeriNumarasi")
            ?? DegeriOku(sonuc, "Ad")
            ?? DegeriOku(sonuc, "SarfMalzemeAdi");

    private static string? DegeriOku(object? kaynak, string propertyName)
    {
        var property = kaynak?.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        return property?.GetValue(kaynak)?.ToString();
    }

    private static Guid? KullaniciIdGetir(ClaimsPrincipal user)
    {
        var claimDegeri = user.FindFirst("KullaniciId")?.Value;
        return Guid.TryParse(claimDegeri, out var kullaniciId) ? kullaniciId : null;
    }

    private static string? RolGetir(ClaimsPrincipal user)
        => user.FindFirst("Rol")?.Value ?? user.FindFirst(ClaimTypes.Role)?.Value;

    private static string? BearerTokenAl(string authorization)
    {
        const string onEk = "Bearer ";
        return authorization.StartsWith(onEk, StringComparison.OrdinalIgnoreCase)
            ? authorization[onEk.Length..].Trim()
            : null;
    }
}
