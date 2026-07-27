using System.Text;
using System.Text.Json.Serialization;
using KimlikVePersonelServisi.Api.Data;
using KimlikVePersonelServisi.Api.Options;
using KimlikVePersonelServisi.Api.Repositories;
using KimlikVePersonelServisi.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Windows EventLog yetkisine takılmamak için geliştirme loglarını konsola yönlendiriyoruz.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var jwtAyarlari = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtAyarlari>(jwtAyarlari);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Enum değerlerini 1/2/3 yerine Admin/ITPersoneli gibi okunabilir metinlerle döndürür.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Swagger, geliştirme sürecinde endpointleri hızlıca görmek ve denemek için eklenir.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Swagger Authorize alanına yalnızca JWT token değerini yapıştır. Postman veya curl kullanırken Authorization: Bearer <token> header'ı gönderilir."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var ayarlar = jwtAyarlari.Get<JwtAyarlari>()
            ?? throw new InvalidOperationException("JWT ayarları bulunamadı.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = ayarlar.Issuer,
            ValidAudience = ayarlar.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ayarlar.SigningKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Admin ve IT personeli, kimlik/personel yönetim ekranlarını kullanabilir.
    options.AddPolicy("AdminVeyaITPersoneli", policy =>
        policy.RequireRole("Admin", "ITPersoneli"));

    // Kullanıcı hesabı açma gibi daha hassas işlemler yalnızca admin rolüne bırakılır.
    options.AddPolicy("SadeceAdmin", policy =>
        policy.RequireRole("Admin"));
});

// DataProtection; cookie, antiforgery ve bazı framework anahtarlarını yönetir.
// Anahtarları proje içinde tutmak, geliştirme makinesindeki Windows yetki sorunlarını azaltır.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys")))
    .SetApplicationName("KimlikVePersonelServisi");

// Kimlik ve personel verileri PostgreSQL üzerinde kimlik_personel şemasında tutulur.
builder.Services.AddDbContext<KimlikPersonelDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("KimlikPersonelDb");
    options.UseNpgsql(connectionString);
});

builder.Services.AddSingleton<ISifreServisi, SifreServisi>();
builder.Services.AddSingleton<ITokenServisi, JwtTokenServisi>();
builder.Services.AddScoped<IDepartmanRepository, EfDepartmanRepository>();
builder.Services.AddScoped<IPersonelRepository, EfPersonelRepository>();
builder.Services.AddScoped<IKullaniciRepository, EfKullaniciRepository>();
builder.Services.AddScoped<IKimlikPersonelServisi, KimlikPersonelServisi>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// İlk geliştirme aşamasında servisler HTTP profiliyle çalıştırılıyor; HTTPS zorlaması ApiGateway fazında ele alınacak.
app.UseAuthentication();
app.UseAuthorization();

// Geliştirme ortamında migration uygulamayı kolaylaştırmak için veritabanı bağlantısını erken doğruluyoruz.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<KimlikPersonelDbContext>();
    dbContext.Database.Migrate();

    var sifreServisi = scope.ServiceProvider.GetRequiredService<ISifreServisi>();
    DemoVeriSeeder.Seed(dbContext, sifreServisi);
}

app.MapControllers();

app.Run();
