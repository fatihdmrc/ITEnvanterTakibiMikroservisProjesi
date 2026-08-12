(() => {
  const sidebar = document.getElementById("appSidebar");
  const toggle = document.getElementById("sidebarToggle");
  const backdrop = document.getElementById("sidebarBackdrop");

  if (!sidebar || !toggle || !backdrop) {
    return;
  }

  const closeSidebar = () => {
    sidebar.classList.remove("open");
    backdrop.classList.remove("open");
  };

  toggle.addEventListener("click", () => {
    sidebar.classList.toggle("open");
    backdrop.classList.toggle("open");
  });

  backdrop.addEventListener("click", closeSidebar);
  sidebar.querySelectorAll("a").forEach((link) => link.addEventListener("click", closeSidebar));
})();
