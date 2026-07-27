var builder = WebApplication.CreateBuilder(args);

// Swagger, geliştirme sürecinde endpointleri hızlıca görmek ve denemek için eklenir.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Bu endpoint, servis ayağa kalktı mı sorusuna en basit doğrulama cevabını verir.
app.MapGet("/saglik", () =>
{
    return Results.Ok(new
    {
        servisAdi = "EnvanterServisi",
        durum = "Calisiyor",
        utcZamani = DateTimeOffset.UtcNow
    });
})
.WithName("SaglikKontrolu")
.WithOpenApi();

app.Run();
