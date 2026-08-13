using System.Text;
using System.Text.Json.Serialization;
using DenetimKaydiServisi.Api.Consumers;
using DenetimKaydiServisi.Api.Options;
using DenetimKaydiServisi.Api.Repositories;
using DenetimKaydiServisi.Api.Sabitler;
using DenetimKaydiServisi.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var jwtAyarlari = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtAyarlari>(jwtAyarlari);
builder.Services.Configure<MongoDbAyarlari>(builder.Configuration.GetSection("MongoDb"));

builder.Services.AddControllers()
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
        Description = "Swagger Authorize alanina yalnizca JWT token degerini yapistir."
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
            ?? throw new InvalidOperationException(DenetimMesajlari.JwtAyarlariYok);

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
        policy.RequireRole(DenetimMesajlari.AdminRolu, DenetimMesajlari.ITPersoneliRolu));
});

var mongoAyarlari = builder.Configuration.GetSection("MongoDb").Get<MongoDbAyarlari>()
    ?? throw new InvalidOperationException(DenetimMesajlari.MongoDbAyarlariYok);

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoAyarlari.ConnectionString));
builder.Services.AddSingleton(provider =>
{
    var client = provider.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoAyarlari.DatabaseName);
});

builder.Services.AddCap(options =>
{
    options.UseMongoDB(mongoAyarlari.ConnectionString);

    options.UseRabbitMQ(rabbitMq =>
    {
        rabbitMq.HostName = builder.Configuration["RabbitMQ:HostName"] ?? "localhost";
        rabbitMq.UserName = builder.Configuration["RabbitMQ:UserName"] ?? "guest";
        rabbitMq.Password = builder.Configuration["RabbitMQ:Password"] ?? "guest";
        rabbitMq.VirtualHost = builder.Configuration["RabbitMQ:VirtualHost"] ?? "/";
        rabbitMq.Port = builder.Configuration.GetValue<int?>("RabbitMQ:Port") ?? 5672;
        rabbitMq.ExchangeName = builder.Configuration["RabbitMQ:ExchangeName"] ?? "inventory.events";
    });

    options.DefaultGroupName = "denetim-kaydi-servisi";
    options.FailedRetryCount = 5;
    options.FailedRetryInterval = 60;
});

builder.Services.AddSingleton<IDenetimKaydiRepository, MongoDenetimKaydiRepository>();
builder.Services.AddScoped<IDenetimKaydiServisi, DenetimKaydiYonetimServisi>();
builder.Services.AddTransient<DenetimEventConsumer>();

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
    var repository = scope.ServiceProvider.GetRequiredService<IDenetimKaydiRepository>();
    await repository.IndeksleriOlusturAsync();
}

app.MapControllers();

app.Run();
