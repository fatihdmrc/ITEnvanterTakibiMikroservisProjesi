using System.Text;
using System.Text.Json.Serialization;
using BildirimServisi.Api.Consumers;
using BildirimServisi.Api.Hubs;
using BildirimServisi.Api.Options;
using BildirimServisi.Api.Sabitler;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

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

builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy(BildirimMesajlari.MvcClientCors, policy =>
    {
        var mvcClientAdresi = builder.Configuration["Cors:MvcClient"] ?? BildirimMesajlari.VarsayilanMvcClientAdresi;

        policy
            .WithOrigins(mvcClientAdresi)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
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
            ?? throw new InvalidOperationException(BildirimMesajlari.JwtAyarlariYok);

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

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrWhiteSpace(accessToken) && path.StartsWithSegments(BildirimMesajlari.BildirimHubYolu))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(BildirimMesajlari.AdminVeyaITPersoneliPolicy, policy =>
        policy.RequireRole(BildirimMesajlari.AdminRolu, BildirimMesajlari.ITPersoneliRolu));
});

var mongoAyarlari = builder.Configuration.GetSection("MongoDb").Get<MongoDbAyarlari>()
    ?? throw new InvalidOperationException(BildirimMesajlari.MongoDbAyarlariYok);

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

    options.DefaultGroupName = "bildirim-servisi";
    options.FailedRetryCount = 5;
    options.FailedRetryInterval = 60;
});

builder.Services.AddTransient<KritikStokBildirimConsumer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors(BildirimMesajlari.MvcClientCors);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<BildirimHub>(BildirimMesajlari.BildirimHubYolu).RequireAuthorization(BildirimMesajlari.AdminVeyaITPersoneliPolicy);

app.Run();
