using DenetimKaydiServisi.Api.Contracts.Events;
using DenetimKaydiServisi.Api.Services;
using DotNetCore.CAP;

namespace DenetimKaydiServisi.Api.Consumers;

public sealed class DenetimEventConsumer(IDenetimKaydiServisi denetimKaydiServisi) : ICapSubscribe
{
    [CapSubscribe(EventAdlari.PersonelIstenAyrildi)]
    public Task PersonelIstenAyrildi(PersonelIstenAyrildiEvent payload, CancellationToken cancellationToken = default)
        => denetimKaydiServisi.EventKaydiOlusturAsync(
            new EventDenetimKaydiOlusturIstek(
                payload.EventId,
                EventAdlari.PersonelIstenAyrildi,
                "KimlikVePersonelServisi",
                "Personel",
                payload.PersonelId.ToString(),
                payload.AdSoyad,
                null,
                $"{payload.AdSoyad} personeli isten ayrildi.",
                payload.OlusmaZamaniUtc,
                payload),
            cancellationToken);

    [CapSubscribe(EventAdlari.CihazDurumuDegisti)]
    public Task CihazDurumuDegisti(CihazDurumuDegistiEvent payload, CancellationToken cancellationToken = default)
        => denetimKaydiServisi.EventKaydiOlusturAsync(
            new EventDenetimKaydiOlusturIstek(
                payload.EventId,
                EventAdlari.CihazDurumuDegisti,
                "EnvanterServisi",
                "Cihaz",
                payload.CihazId.ToString(),
                payload.AssetTag ?? payload.SeriNumarasi,
                payload.OlusturanKullaniciId,
                $"Cihaz durumu {payload.OncekiDurum} -> {payload.YeniDurum} olarak degisti.",
                payload.OlusmaZamaniUtc,
                payload),
            cancellationToken);

    [CapSubscribe(EventAdlari.KritikStokSeviyesineDusuldu)]
    public Task KritikStokSeviyesineDusuldu(KritikStokSeviyesineDusulduEvent payload, CancellationToken cancellationToken = default)
        => denetimKaydiServisi.EventKaydiOlusturAsync(
            new EventDenetimKaydiOlusturIstek(
                payload.EventId,
                EventAdlari.KritikStokSeviyesineDusuldu,
                "EnvanterServisi",
                payload.VarlikTuru,
                (payload.SarfMalzemeId ?? payload.KategoriId).ToString(),
                payload.SarfMalzemeAdi ?? payload.CihazModeli,
                null,
                "Kritik stok seviyesi altina dusuldu.",
                payload.OlusmaZamaniUtc,
                payload),
            cancellationToken);

    [CapSubscribe(EventAdlari.ZimmetOlusturuldu)]
    public Task ZimmetOlusturuldu(ZimmetOlusturulduEvent payload, CancellationToken cancellationToken = default)
        => denetimKaydiServisi.EventKaydiOlusturAsync(
            new EventDenetimKaydiOlusturIstek(
                payload.EventId,
                EventAdlari.ZimmetOlusturuldu,
                "ZimmetServisi",
                "Zimmet",
                payload.ZimmetId.ToString(),
                payload.CihazAssetTag ?? payload.CihazAd,
                payload.ZimmetleyenKullaniciId,
                $"{payload.PersonelAdSoyad} personeline zimmet olusturuldu.",
                payload.OlusmaZamaniUtc,
                payload),
            cancellationToken);

    [CapSubscribe(EventAdlari.ZimmetIadeAlindi)]
    public Task ZimmetIadeAlindi(ZimmetIadeAlindiEvent payload, CancellationToken cancellationToken = default)
        => denetimKaydiServisi.EventKaydiOlusturAsync(
            new EventDenetimKaydiOlusturIstek(
                payload.EventId,
                EventAdlari.ZimmetIadeAlindi,
                "ZimmetServisi",
                "Zimmet",
                payload.ZimmetId.ToString(),
                payload.PersonelAdSoyad,
                payload.IadeAlanKullaniciId,
                "Zimmet iadesi fiziki kontrol surecine alindi.",
                payload.OlusmaZamaniUtc,
                payload),
            cancellationToken);

    [CapSubscribe(EventAdlari.ZimmetIadeEdildi)]
    public Task ZimmetIadeEdildi(ZimmetIadeEdildiEvent payload, CancellationToken cancellationToken = default)
        => denetimKaydiServisi.EventKaydiOlusturAsync(
            new EventDenetimKaydiOlusturIstek(
                payload.EventId,
                EventAdlari.ZimmetIadeEdildi,
                "ZimmetServisi",
                "Zimmet",
                payload.ZimmetId.ToString(),
                payload.PersonelAdSoyad,
                payload.IadeKontroluYapanKullaniciId,
                "Zimmet iade kontrolu tamamlandi.",
                payload.OlusmaZamaniUtc,
                payload),
            cancellationToken);

}
