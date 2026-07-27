using EnvanterTakip.MvcClient.Services;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Windows EventLog yetkisine takılmamak için geliştirme loglarını konsola yönlendiriyoruz.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// MVC client, ilk aşamada servisleri basit ekranlardan izlemek için kullanılacak.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys")))
    .SetApplicationName("EnvanterTakipMvcClient");
builder.Services.AddHttpClient<KimlikPersonelApiClient>(client =>
{
    var servisAdresi = builder.Configuration["ServisAdresleri:KimlikVePersonelServisi"]
        ?? throw new InvalidOperationException("Kimlik ve personel servisi adresi tanımlı değil.");

    // Client şu aşamada servise doğrudan gider; ApiGateway eklendiğinde yalnızca bu adres değişecektir.
    client.BaseAddress = new Uri(servisAdresi);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // Production ortamında tarayıcının yalnızca HTTPS kullanmasını isteriz.
    app.UseHsts();
}

// Geliştirme profili HTTP kullandığı için HTTPS yönlendirmesini şimdilik zorlamıyoruz.
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
