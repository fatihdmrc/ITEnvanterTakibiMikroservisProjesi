using System.Text.Json.Serialization;
using MailServisi.Api.Consumers;
using MailServisi.Api.Options;
using MailServisi.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.Configure<MongoDbAyarlari>(builder.Configuration.GetSection("MongoDb"));
builder.Services.Configure<GmailAyarlari>(builder.Configuration.GetSection("Gmail"));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var mongoAyarlari = builder.Configuration.GetSection("MongoDb").Get<MongoDbAyarlari>()
    ?? throw new InvalidOperationException("MongoDB ayarlari bulunamadi.");

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

    options.DefaultGroupName = "mail-servisi";
    options.FailedRetryCount = 0;
});

builder.Services.AddScoped<IZimmetMailServisi, GmailZimmetMailServisi>();
builder.Services.AddTransient<ZimmetMailConsumer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
