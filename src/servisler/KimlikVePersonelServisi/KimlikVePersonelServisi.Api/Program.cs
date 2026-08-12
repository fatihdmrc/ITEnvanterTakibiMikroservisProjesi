using System.Text;
using System.Text.Json.Serialization;
using KimlikVePersonelServisi.Api.Filters;
using KimlikVePersonelServisi.Api.Data;
using KimlikVePersonelServisi.Api.Domain.Entities;
using KimlikVePersonelServisi.Api.Options;
using KimlikVePersonelServisi.Api.Repositories;
using KimlikVePersonelServisi.Api.Services;
using KimlikVePersonelServisi.Api.Services.Harici;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Windows EventLog yetkisine takılmamak için geliştirme loglarını konsola yönlendiriyoruz.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var jwtAyarlari = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtAyarlari>(jwtAyarlari);
var kimlikPersonelConnectionString = builder.Configuration.GetConnectionString("KimlikPersonelDb")
    ?? throw new InvalidOperationException("Kimlik/personel veritabanı bağlantısı bulunamadı.");

builder.Services.AddControllers(options =>
    {
        options.Filters.Add<CrudDenetimActionFilter>();
    })
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
    options.UseNpgsql(kimlikPersonelConnectionString);
});

builder.Services.AddCap(options =>
{
    options.UsePostgreSql(postgreSql =>
    {
        postgreSql.DataSource = NpgsqlDataSource.Create(kimlikPersonelConnectionString);
        postgreSql.Schema = "cap_kimlik";
    });

    options.UseRabbitMQ(rabbitMq =>
    {
        rabbitMq.HostName = builder.Configuration["RabbitMQ:HostName"] ?? "localhost";
        rabbitMq.UserName = builder.Configuration["RabbitMQ:UserName"] ?? "guest";
        rabbitMq.Password = builder.Configuration["RabbitMQ:Password"] ?? "guest";
        rabbitMq.VirtualHost = builder.Configuration["RabbitMQ:VirtualHost"] ?? "/";
        rabbitMq.Port = builder.Configuration.GetValue<int?>("RabbitMQ:Port") ?? 5672;
        rabbitMq.ExchangeName = builder.Configuration["RabbitMQ:ExchangeName"] ?? "inventory.events";
    });

    options.DefaultGroupName = "kimlik-personel-servisi";
    options.FailedRetryCount = 5;
    options.FailedRetryInterval = 60;
});

builder.Services.AddHttpClient<DenetimApiClient>(client =>
{
    var servisAdresi = builder.Configuration["ServisAdresleri:DenetimKaydiServisi"]
        ?? throw new InvalidOperationException("Denetim kaydi servisi adresi tanimli degil.");

    client.BaseAddress = new Uri(servisAdresi);
    client.Timeout = TimeSpan.FromSeconds(2);
});

// Identity kullanıcı, rol, şifre hash ve hesap kilitleme altyapısını yönetir; dış API çağrılarında yine JWT kullanılır.
builder.Services
    .AddIdentityCore<UygulamaKullanici>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequiredUniqueChars = 1;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.User.RequireUniqueEmail = false;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<KimlikPersonelDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<ITokenServisi, JwtTokenServisi>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(EfGenericRepository<>));
builder.Services.AddScoped<IDepartmanRepository, EfDepartmanRepository>();
builder.Services.AddScoped<IPersonelRepository, EfPersonelRepository>();
builder.Services.AddScoped<CrudDenetimActionFilter>();
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

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UygulamaKullanici>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var demoVeriyiSifirla = app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>("DemoVeri:Sifirla");
    await DemoVeriSeeder.SeedAsync(dbContext, userManager, roleManager, demoVeriyiSifirla);
}

app.MapControllers();

app.Run();
