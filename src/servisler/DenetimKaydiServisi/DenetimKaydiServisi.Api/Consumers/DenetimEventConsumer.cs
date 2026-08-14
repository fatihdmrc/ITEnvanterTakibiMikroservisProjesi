using DenetimKaydiServisi.Api.Contracts.Events;
using DenetimKaydiServisi.Api.Sabitler;
using DenetimKaydiServisi.Api.Services;
using DotNetCore.CAP;

namespace DenetimKaydiServisi.Api.Consumers;

public sealed class DenetimEventConsumer(
    IDenetimKaydiServisi denetimKaydiServisi,
    ILogger<DenetimEventConsumer> logger) : ICapSubscribe
{
    [CapSubscribe(EventAdlari.PersonelIstenAyrildi)]
    public async Task PersonelIstenAyrildi(PersonelIstenAyrildiEvent payload, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(DenetimMesajlari.EventAlindiLogu, EventAdlari.PersonelIstenAyrildi, payload.EventId);
        await denetimKaydiServisi.EventKaydiOlusturAsync(
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
        logger.LogInformation(DenetimMesajlari.EventTetiklenenIslemTamamlandiLogu, EventAdlari.PersonelIstenAyrildi, payload.EventId);
    }

    [CapSubscribe(EventAdlari.CihazDurumuDegisti)]
    public async Task CihazDurumuDegisti(CihazDurumuDegistiEvent payload, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(DenetimMesajlari.EventAlindiLogu, EventAdlari.CihazDurumuDegisti, payload.EventId);
        await denetimKaydiServisi.EventKaydiOlusturAsync(
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
        logger.LogInformation(DenetimMesajlari.EventTetiklenenIslemTamamlandiLogu, EventAdlari.CihazDurumuDegisti, payload.EventId);
    }

    [CapSubscribe(EventAdlari.KritikStokSeviyesineDusuldu)]
    public async Task KritikStokSeviyesineDusuldu(KritikStokSeviyesineDusulduEvent payload, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(DenetimMesajlari.EventAlindiLogu, EventAdlari.KritikStokSeviyesineDusuldu, payload.EventId);
        await denetimKaydiServisi.EventKaydiOlusturAsync(
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
        logger.LogInformation(DenetimMesajlari.EventTetiklenenIslemTamamlandiLogu, EventAdlari.KritikStokSeviyesineDusuldu, payload.EventId);
    }

    [CapSubscribe(EventAdlari.ZimmetOlusturuldu)]
    public async Task ZimmetOlusturuldu(ZimmetOlusturulduEvent payload, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(DenetimMesajlari.EventAlindiLogu, EventAdlari.ZimmetOlusturuldu, payload.EventId);
        await denetimKaydiServisi.EventKaydiOlusturAsync(
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
        logger.LogInformation(DenetimMesajlari.EventTetiklenenIslemTamamlandiLogu, EventAdlari.ZimmetOlusturuldu, payload.EventId);
    }

    [CapSubscribe(EventAdlari.ZimmetIadeAlindi)]
    public async Task ZimmetIadeAlindi(ZimmetIadeAlindiEvent payload, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(DenetimMesajlari.EventAlindiLogu, EventAdlari.ZimmetIadeAlindi, payload.EventId);
        await denetimKaydiServisi.EventKaydiOlusturAsync(
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
        logger.LogInformation(DenetimMesajlari.EventTetiklenenIslemTamamlandiLogu, EventAdlari.ZimmetIadeAlindi, payload.EventId);
    }

    [CapSubscribe(EventAdlari.ZimmetIadeEdildi)]
    public async Task ZimmetIadeEdildi(ZimmetIadeEdildiEvent payload, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(DenetimMesajlari.EventAlindiLogu, EventAdlari.ZimmetIadeEdildi, payload.EventId);
        await denetimKaydiServisi.EventKaydiOlusturAsync(
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
        logger.LogInformation(DenetimMesajlari.EventTetiklenenIslemTamamlandiLogu, EventAdlari.ZimmetIadeEdildi, payload.EventId);
    }

}
