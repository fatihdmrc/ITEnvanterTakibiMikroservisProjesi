namespace DenetimKaydiServisi.Api.Options;

public sealed class MongoDbAyarlari
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string DenetimKayitlariCollectionName { get; set; } = "DenetimKayitlari";
}
