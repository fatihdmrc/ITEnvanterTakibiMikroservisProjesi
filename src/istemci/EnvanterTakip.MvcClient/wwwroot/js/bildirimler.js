(function () {
  "use strict";

  const merkez = document.getElementById("bildirimMerkezi");
  const sayac = document.getElementById("bildirimSayaci");
  const liste = document.getElementById("bildirimListesi");
  const bildirimler = [];

  if (!merkez || !sayac || !liste || !window.signalR) {
    return;
  }

  fetch("/Bildirim/BaglantiBilgisi", {
    method: "GET",
    headers: { Accept: "application/json" },
    credentials: "same-origin"
  })
    .then(response => {
      if (!response.ok) {
        return null;
      }

      return response.json();
    })
    .then(baglanti => {
      if (!baglanti || !baglanti.hubUrl || !baglanti.token) {
        return;
      }

      merkez.classList.remove("d-none");

      const connection = new signalR.HubConnectionBuilder()
        .withUrl(baglanti.hubUrl, {
          accessTokenFactory: () => baglanti.token
        })
        .withAutomaticReconnect()
        .build();

      connection.on("KritikStokBildirimiAlindi", bildirim => {
        bildirimEkle(bildirim);
        toastGoster(bildirim);
      });

      connection.start().catch(() => {
        console.warn("Bildirim servisine bağlanılamadı.");
      });
    })
    .catch(() => {
      console.warn("Bildirim bağlantı bilgisi alınamadı.");
    });

  function bildirimEkle(bildirim) {
    bildirimler.unshift(bildirim);

    if (bildirimler.length > 10) {
      bildirimler.pop();
    }

    liste.innerHTML = "";
    for (const item of bildirimler) {
      liste.appendChild(bildirimSatiriOlustur(item));
    }

    sayac.textContent = bildirimler.length.toString();
    sayac.classList.toggle("d-none", bildirimler.length === 0);
  }

  function bildirimSatiriOlustur(bildirim) {
    const satir = document.createElement("div");
    satir.className = "bildirim-kaydi";

    const baslik = document.createElement("div");
    baslik.className = "fw-semibold text-danger";
    baslik.textContent = bildirim.baslik || "Kritik stok uyarısı";

    const mesaj = document.createElement("div");
    mesaj.className = "small";
    mesaj.textContent = bildirim.mesaj || "Kritik stok seviyesi altına düşüldü.";

    const zaman = document.createElement("div");
    zaman.className = "bildirim-zamani mt-1";
    zaman.textContent = tarihFormatla(bildirim.yayinlanmaZamaniUtc || bildirim.olusmaZamaniUtc);

    satir.appendChild(baslik);
    satir.appendChild(mesaj);
    satir.appendChild(zaman);

    return satir;
  }

  function toastGoster(bildirim) {
    let container = document.querySelector(".bildirim-toast-container");
    if (!container) {
      container = document.createElement("div");
      container.className = "bildirim-toast-container";
      document.body.appendChild(container);
    }

    const toast = document.createElement("div");
    toast.className = "toast align-items-center text-bg-danger border-0";
    toast.setAttribute("role", "alert");
    toast.setAttribute("aria-live", "assertive");
    toast.setAttribute("aria-atomic", "true");
    toast.innerHTML = [
      '<div class="d-flex">',
      '<div class="toast-body">',
      escapeHtml(bildirim.mesaj || "Kritik stok seviyesi altına düşüldü."),
      "</div>",
      '<button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Kapat"></button>',
      "</div>"
    ].join("");

    container.appendChild(toast);

    const bootstrapToast = new bootstrap.Toast(toast, { delay: 8000 });
    toast.addEventListener("hidden.bs.toast", () => toast.remove());
    bootstrapToast.show();
  }

  function tarihFormatla(deger) {
    if (!deger) {
      return "";
    }

    return new Date(deger).toLocaleString("tr-TR");
  }

  function escapeHtml(deger) {
    return String(deger)
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;")
      .replaceAll('"', "&quot;")
      .replaceAll("'", "&#039;");
  }
})();
