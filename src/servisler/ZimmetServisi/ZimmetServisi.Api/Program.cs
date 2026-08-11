using System.Text;
using System.Text.Json.Serialization;
using ZimmetServisi.Api.Data;
using ZimmetServisi.Api.Filters;
using ZimmetServisi.Api.Options;
using ZimmetServisi.Api.Repositories;
using ZimmetServisi.Api.Services;
using ZimmetServisi.Api.Services.Harici;
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
var zimmetConnectionString = builder.Configuration.GetConnectionString("ZimmetDb")
    ?? throw new InvalidOperationException("Zimmet veritabanı bağlantısı bulunamadı.");

builder.Services.AddControllers(options =>
    {
        options.Filters.Add<CrudDenetimActionFilter>();
    })
    .AddJsonOptions(options =>
    {
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

builder.Services.AddDbContext<ZimmetDbContext>(options =>
{
    options.UseNpgsql(zimmetConnectionString);
});

builder.Services.AddCap(options =>
{
    options.UsePostgreSql(postgreSql =>
    {
        postgreSql.DataSource = NpgsqlDataSource.Create(zimmetConnectionString);
        postgreSql.Schema = "cap_zimmet";
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

    options.DefaultGroupName = "zimmet-servisi";
    options.FailedRetryCount = 5;
    options.FailedRetryInterval = 60;
});

builder.Services.AddHttpClient<KimlikPersonelApiClient>(client =>
{
    var servisAdresi = builder.Configuration["ServisAdresleri:KimlikVePersonelServisi"]
        ?? throw new InvalidOperationException("Kimlik ve personel servisi adresi tanımlı değil.");

    client.BaseAddress = new Uri(servisAdresi);
});

builder.Services.AddHttpClient<EnvanterApiClient>(client =>
{
    var servisAdresi = builder.Configuration["ServisAdresleri:EnvanterServisi"]
        ?? throw new InvalidOperationException("Envanter servisi adresi tanımlı değil.");

    client.BaseAddress = new Uri(servisAdresi);
});

builder.Services.AddHttpClient<DenetimApiClient>(client =>
{
    var servisAdresi = builder.Configuration["ServisAdresleri:DenetimKaydiServisi"]
        ?? throw new InvalidOperationException("Denetim kaydi servisi adresi tanimli degil.");

    client.BaseAddress = new Uri(servisAdresi);
    client.Timeout = TimeSpan.FromSeconds(2);
});

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(EfGenericRepository<>));
builder.Services.AddScoped<IZimmetRepository, EfZimmetRepository>();
builder.Services.AddScoped<CrudDenetimActionFilter>();
builder.Services.AddScoped<IZimmetServisi, ZimmetYonetimServisi>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ZimmetDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.MapControllers();

app.Run();
