(() => {
  const sidebar = document.getElementById("appSidebar");
  const toggle = document.getElementById("sidebarToggle");
  const desktopToggle = document.getElementById("desktopSidebarToggle");
  const backdrop = document.getElementById("sidebarBackdrop");
  const collapsedStorageKey = "it-envanter-sidebar-collapsed";

  if (!sidebar || !backdrop) {
    return;
  }

  const setDesktopToggleState = () => {
    if (!desktopToggle) {
      return;
    }

    const collapsed = document.body.classList.contains("sidebar-collapsed");
    const icon = desktopToggle.querySelector(".material-symbols-outlined");
    desktopToggle.setAttribute("aria-label", collapsed ? "Menüyü genişlet" : "Menüyü daralt");
    desktopToggle.setAttribute("title", collapsed ? "Menüyü genişlet" : "Menüyü daralt");

    if (icon) {
      icon.textContent = collapsed ? "chevron_right" : "chevron_left";
    }
  };

  if (localStorage.getItem(collapsedStorageKey) === "true") {
    document.body.classList.add("sidebar-collapsed");
  }

  setDesktopToggleState();

  const closeSidebar = () => {
    sidebar.classList.remove("open");
    backdrop.classList.remove("open");
  };

  if (toggle) {
    toggle.addEventListener("click", () => {
      sidebar.classList.toggle("open");
      backdrop.classList.toggle("open");
    });
  }

  if (desktopToggle) {
    desktopToggle.addEventListener("click", () => {
      document.body.classList.toggle("sidebar-collapsed");
      localStorage.setItem(collapsedStorageKey, document.body.classList.contains("sidebar-collapsed").toString());
      setDesktopToggleState();
    });
  }

  backdrop.addEventListener("click", closeSidebar);
  sidebar.querySelectorAll("a").forEach((link) => link.addEventListener("click", closeSidebar));
})();
