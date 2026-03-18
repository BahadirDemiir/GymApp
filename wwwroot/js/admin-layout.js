document.getElementById("sidebarToggle").addEventListener("click", function () {
  const sidebar = document.getElementById("adminSidebar");
  const content = document.getElementById("adminContent");
  const toggle = this;

  sidebar.classList.toggle("collapsed");
  content.classList.toggle("expanded");
  toggle.classList.toggle("collapsed");

  localStorage.setItem(
    "sidebarCollapsed",
    sidebar.classList.contains("collapsed")
  );
});

function toggleMobileSidebar() {
  const sidebar = document.getElementById("adminSidebar");
  sidebar.classList.toggle("mobile-open");
}

document.addEventListener("DOMContentLoaded", function () {
  const sidebarCollapsed = localStorage.getItem("sidebarCollapsed") === "true";
  if (sidebarCollapsed) {
    document.getElementById("adminSidebar").classList.add("collapsed");
    document.getElementById("adminContent").classList.add("expanded");
    document.getElementById("sidebarToggle").classList.add("collapsed");
  }
});

document.addEventListener("click", function (e) {
  const sidebar = document.getElementById("adminSidebar");
  const toggle = document.getElementById("sidebarToggle");

  if (
    window.innerWidth <= 768 &&
    !sidebar.contains(e.target) &&
    !toggle.contains(e.target) &&
    sidebar.classList.contains("mobile-open")
  ) {
    sidebar.classList.remove("mobile-open");
  }
});
