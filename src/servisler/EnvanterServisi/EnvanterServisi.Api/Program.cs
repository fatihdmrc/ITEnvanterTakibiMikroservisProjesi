using System.Text;
using System.Text.Json.Serialization;
using EnvanterServisi.Api.Data;
using EnvanterServisi.Api.Filters;
using EnvanterServisi.Api.Options;
using EnvanterServisi.Api.Repositories;
using EnvanterServisi.Api.Services;
using EnvanterServisi.Api.Services.Harici;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var jwtAyarlari = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtAyarlari>(jwtAyarlari);
var envanterConnectionString = builder.Configuration.GetConnectionString("EnvanterDb")
    ?? throw new InvalidOperationException("Envanter veritabanı bağlantısı bulunamadı.");

builder.Services.AddControllers(options =>
    {
        options.Filters.Add<CrudDenetimActionFilter>();
    })
    .AddJsonOptions(options =>
    {
        // Enum değerleri API cevabında sayısal değil, okunabilir metin olarak görünür.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

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
        Description = "Swagger Authorize alanına yalnızca JWT token değerini yapıştır."
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
    options.AddPolicy("AdminVeyaITPersoneli", policy =>
        policy.RequireRole("Admin", "ITPersoneli"));
});

builder.Services.AddDbContext<EnvanterDbContext>(options =>
{
    options.UseNpgsql(envanterConnectionString);
});

builder.Services.AddCap(options =>
{
    options.UsePostgreSql(postgreSql =>
    {
        postgreSql.DataSource = NpgsqlDataSource.Create(envanterConnectionString);
        postgreSql.Schema = "cap_envanter";
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

    options.DefaultGroupName = "envanter-servisi";
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

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(EfGenericRepository<>));
builder.Services.AddScoped<IKategoriRepository, EfKategoriRepository>();
builder.Services.AddScoped<ILokasyonRepository, EfLokasyonRepository>();
builder.Services.AddScoped<ICihazRepository, EfCihazRepository>();
builder.Services.AddScoped<ISarfMalzemeRepository, EfSarfMalzemeRepository>();
builder.Services.AddScoped<IKritikStokKuraliRepository, EfKritikStokKuraliRepository>();
builder.Services.AddScoped<IStokHareketiRepository, EfStokHareketiRepository>();
builder.Services.AddScoped<CrudDenetimActionFilter>();
builder.Services.AddScoped<IEnvanterServisi, EnvanterYonetimServisi>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// İlk fazlarda servisler HTTP profiliyle çalıştırılıyor; HTTPS zorlaması ApiGateway fazında ele alınacak.
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EnvanterDbContext>();
    await dbContext.Database.MigrateAsync();
    await DemoVeriSeeder.SeedAsync(dbContext);
}

app.MapControllers();

app.Run();
