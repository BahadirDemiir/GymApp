function on(el, ev, cb) {
  if (el && typeof el.addEventListener === "function") {
    el.addEventListener(ev, cb);
    return true;
  }
  return false;
}

let map;
let markersLayer;
let markerById = new Map();

const mapStyles = {
  light: {
    url: "https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png",
    attribution: "© OpenStreetMap contributors, © CARTO",
    name: "Açık Tema",
  },
  dark: {
    url: "https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png",
    attribution: "© OpenStreetMap contributors, © CARTO",
    name: "Koyu Tema",
  },
  satellite: {
    url: "https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}",
    attribution:
      "© Esri, Maxar, Earthstar Geographics, and the GIS User Community",
    name: "Uydu Görünümü",
  },
  streets: {
    url: "https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png",
    attribution: "© OpenStreetMap contributors",
    name: "Sokak Görünümü",
  },
};

let currentMapStyle = "light";
let currentTileLayer;

const markerStyles = {
  light: {
    active: L.divIcon({
      className: "sbx-pin light-theme",
      html: `<div class="pin"></div>`,
      iconSize: [24, 24],
      iconAnchor: [12, 12],
    }),
    inactive: L.divIcon({
      className: "inactive-marker light-theme",
      html: '<div class="pin-inactive"></div>',
      iconSize: [18, 18],
      iconAnchor: [9, 9],
    }),
  },
  dark: {
    active: L.divIcon({
      className: "sbx-pin dark-theme",
      html: `<div class="pin"></div>`,
      iconSize: [24, 24],
      iconAnchor: [12, 12],
    }),
    inactive: L.divIcon({
      className: "inactive-marker dark-theme",
      html: '<div class="pin-inactive"></div>',
      iconSize: [18, 18],
      iconAnchor: [9, 9],
    }),
  },
  satellite: {
    active: L.divIcon({
      className: "sbx-pin satellite-theme",
      html: `<div class="pin"></div>`,
      iconSize: [24, 24],
      iconAnchor: [12, 12],
    }),
    inactive: L.divIcon({
      className: "inactive-marker satellite-theme",
      html: '<div class="pin-inactive"></div>',
      iconSize: [18, 18],
      iconAnchor: [9, 9],
    }),
  },
  streets: {
    active: L.divIcon({
      className: "sbx-pin streets-theme",
      html: `<div class="pin"></div>`,
      iconSize: [24, 24],
      iconAnchor: [12, 12],
    }),
    inactive: L.divIcon({
      className: "inactive-marker streets-theme",
      html: '<div class="pin-inactive"></div>',
      iconSize: [18, 18],
      iconAnchor: [9, 9],
    }),
  },
};

let activeIcon = markerStyles.light.active;
let inactiveIcon = markerStyles.light.inactive;

let panelFilterBtn;
let cityDropdown;
let cityItemsWrap;
let searchBox;
let storeListEl;
let panelHandle;
let overlayPanel;
let mapWrap;

let nearbyTabBtn; 
let nearbyStateEl; 
let nearbyListEl; 


let latestGyms = [];
let selectedCityId = "";
let citiesCache = [];
let isPanelCollapsed = false;


let nearestDone = false;

function initPanelToggle() {
  panelHandle = document.getElementById("panelHandle");
  overlayPanel = document.querySelector(".overlay-panel");
  mapWrap = document.querySelector(".map-wrap");

  if (panelHandle && overlayPanel) {
    on(panelHandle, "click", () => {
      isPanelCollapsed = !isPanelCollapsed;

      if (isPanelCollapsed) {
        overlayPanel.classList.add("collapsed");
        mapWrap.classList.add("panel-collapsed");
        panelHandle.innerHTML = '<i class="bi bi-chevron-right"></i>';
      } else {
        overlayPanel.classList.remove("collapsed");
        mapWrap.classList.remove("panel-collapsed");
        panelHandle.innerHTML = '<i class="bi bi-chevron-left"></i>';
      }
    });
  }
}


function loadCities() {
  return fetch("/api/gyms/cities")
    .then((r) => r.json())
    .then((cities) => {
      citiesCache = cities || [];
      cityItemsWrap.innerHTML = "";
      citiesCache.forEach((c) => {
        const btn = document.createElement("button");
        btn.className = "dropdown-item";
        btn.dataset.cityId = c.cityId;
        btn.textContent = c.name;
        cityItemsWrap.appendChild(btn);
      });
      highlightActiveCity();
    })
    .catch((err) => console.error("Şehirler yüklenirken hata:", err));
}


function loadMarkers(includeInactive) {
  const activeFlag = !!includeInactive ? "false" : "true"; 
  let url = `/api/gyms?activeOnly=${activeFlag}`;
  if (selectedCityId) url += `&cityId=${selectedCityId}`;

  return fetch(url)
    .then((r) => r.json())
    .then((data) => {
      latestGyms = data || [];
      renderMarkers(latestGyms);
      renderList(filterBySearch(latestGyms, searchBox ? searchBox.value : ""));
      if (
        nearestDone &&
        document.querySelector(".tab-btn.active")?.dataset.target ===
          "#nearbyTab"
      ) {
        computeNearby();
      }
    })
    .catch((err) => console.error(err));
}

function renderMarkers(list) {
  markersLayer.clearLayers();
  markerById.clear();

  list.forEach((g, idx) => {
    const icon = g.isActive ? activeIcon : inactiveIcon;
    const m = L.marker([g.latitude, g.longitude], { icon }).bindPopup(`
        <strong>${escapeHtml(g.name)}</strong><br/>
        Şehir: ${escapeHtml(g.cityName ?? "-")}<br/>
        Durum: ${g.isActive ? "Active" : "Inactive"}<br/>
        Detaylar: <a href="/Gym/GymDetails/${g.id}">Detaylar</a>
      `);
    m.addTo(markersLayer);
    const id = g.id ?? idx;
    markerById.set(String(id), m);
  });

  if (list.length) {
    const bounds = L.latLngBounds(list.map((g) => [g.latitude, g.longitude]));
    map.fitBounds(bounds.pad(0.2));
    setTimeout(() => map.invalidateSize(), 150);
  }
}

function renderList(list) {
  storeListEl.innerHTML = "";
  if (!list.length) {
    storeListEl.innerHTML = `<div class="p-3 text-muted">Seçtiğin filtreyle sonuç yok</div>`;
    return;
  }
  list.forEach((g, idx) => {
    const id = g.id ?? idx;
    const row = document.createElement("div");
    row.className = "store-item";
    row.innerHTML = `
      <div class="store-title">${escapeHtml((g.name || "").toUpperCase())}</div>
      <div class="store-sub">${escapeHtml(
        g.cityName ? g.cityName : "—"
      )}<br/>${escapeHtml(g.address ?? "—")}</div>
      <div class="store-actions">
        <i class="bi bi-info-circle" onclick="openGymDetails(${
          g.id
        })" title="Bilgi"></i>
        <i class="bi bi-arrow-right-circle" title="Yol tarifi"></i> 
      </div>
    `;

    row.addEventListener("mouseenter", () => {
      const m = markerById.get(String(id));
      if (m && m._icon) {
        m._icon.classList.add("bounce");
        setTimeout(() => m._icon?.classList.remove("bounce"), 700);
      }
    });

    row.addEventListener("click", () => {
      const m = markerById.get(String(id));
      if (m) {
        const ll = m.getLatLng();
        map.setView(ll, Math.max(map.getZoom(), 14));
        m.openPopup();
      }
    });

    storeListEl.appendChild(row);
  });
}

function openGymDetails(id) {
  window.location.href = `/Gym/GymDetails/${id}`;
}

function changeMapStyle(styleName) {
  if (!map || !mapStyles[styleName]) return;

  if (currentTileLayer) {
    map.removeLayer(currentTileLayer);
  }

  const style = mapStyles[styleName];
  currentTileLayer = L.tileLayer(style.url, {
    attribution: style.attribution,
  }).addTo(map);

  currentMapStyle = styleName;

  activeIcon = markerStyles[styleName].active;
  inactiveIcon = markerStyles[styleName].inactive;

  updateAllMarkers();

  document
    .querySelectorAll("#mapStyleDropdown + .dropdown-menu .dropdown-item")
    .forEach((item) => {
      item.classList.remove("active");
    });
  document.querySelector(`[data-style="${styleName}"]`).classList.add("active");

  const dropdownBtn = document.getElementById("mapStyleDropdown");
  if (dropdownBtn) {
    dropdownBtn.innerHTML = `<i class="bi bi-palette"></i> ${style.name}`;
  }

  setTimeout(() => map.invalidateSize(), 100);
}

function updateAllMarkers() {
  if (markersLayer && latestGyms.length > 0) {
    markersLayer.clearLayers();
    markerById.clear();

    renderMarkers(latestGyms);
  }
}


function filterBySearch(list, q) {
  const s = (q || "").trim().toLowerCase();
  if (!s) return list;
  return list.filter(
    (g) =>
      (g.name || "").toLowerCase().includes(s) ||
      (g.cityName || "").toLowerCase().includes(s) ||
      (g.address || "").toLowerCase().includes(s)
  );
}

let searchDebounce;
function initSearch() {
  on(searchBox, "input", () => {
    clearTimeout(searchDebounce);
    searchDebounce = setTimeout(() => {
      const filtered = filterBySearch(latestGyms, searchBox.value);
      renderList(filtered);
    }, 150);
  });
}

function highlightActiveCity() {
  if (!cityDropdown) return;
  const all = cityDropdown.querySelectorAll(".dropdown-item");
  all.forEach((x) => x.classList.remove("active"));
  const active = Array.from(all).find(
    (x) => (x.dataset.cityId ?? "") === (selectedCityId ?? "")
  );
  if (active) active.classList.add("active");
}

function initCityDropdown() {
  on(panelFilterBtn, "click", () => {
    const isOpen = !cityDropdown.hasAttribute("hidden");
    if (isOpen) {
      cityDropdown.setAttribute("hidden", "");
      panelFilterBtn.setAttribute("aria-expanded", "false");
    } else {
      cityDropdown.removeAttribute("hidden");
      panelFilterBtn.setAttribute("aria-expanded", "true");
    }
  });

  on(document, "click", (e) => {
    if (
      cityDropdown &&
      panelFilterBtn &&
      !cityDropdown.contains(e.target) &&
      !panelFilterBtn.contains(e.target)
    ) {
      cityDropdown.setAttribute("hidden", "");
      panelFilterBtn && panelFilterBtn.setAttribute("aria-expanded", "false");
    }
  });

  on(cityDropdown, "click", (e) => {
    const item = e.target.closest(".dropdown-item");
    if (!item) return;
    selectedCityId = item.dataset.cityId ?? "";
    highlightActiveCity();
    cityDropdown.setAttribute("hidden", "");
    panelFilterBtn && panelFilterBtn.setAttribute("aria-expanded", "false");
    loadMarkers(false);
  });
}

function initNearbyRefs() {
  const tabButtons = document.querySelectorAll(".tab-btn");
  tabButtons.forEach((btn) => {
    on(btn, "click", () => {
      document
        .querySelectorAll(".tab-btn")
        .forEach((b) => b.classList.remove("active"));
      btn.classList.add("active");
      document
        .querySelectorAll(".tab-content")
        .forEach((tc) => tc.classList.remove("active"));
      const targetSel = btn.dataset.target;
      const target = document.querySelector(targetSel);
      target?.classList.add("active");

      if (targetSel === "#nearbyTab") {
        ensureNearbyComputed();
      }
    });
  });

  nearbyStateEl = document.getElementById("nearbyState");
  nearbyListEl = document.getElementById("nearbyList");
}

function ensureNearbyComputed() {
  if (nearestDone && nearbyListEl && !nearbyListEl.hidden) {
    return; 
  }
  if (!Array.isArray(latestGyms) || latestGyms.length === 0) {
    if (nearbyStateEl) nearbyStateEl.textContent = "Salon verisi bekleniyor…";
    return;
  }
  if (!navigator.geolocation) {
    if (nearbyStateEl)
      nearbyStateEl.textContent = "Tarayıcın konum özelliğini desteklemiyor 😔";
    return;
  }

  if (nearbyStateEl) {
    nearbyStateEl.hidden = false;
    nearbyStateEl.textContent = "Konum alınıyor… (izin vermen gerekebilir) 🧭";
  }

  navigator.geolocation.getCurrentPosition(
    (pos) => {
      const uLat = pos.coords.latitude;
      const uLng = pos.coords.longitude;
      computeNearby(uLat, uLng);
    },
    (err) => {
      const reason =
        err.code === 1
          ? "Konum izni verilmedi"
          : err.code === 2
          ? "Konum belirlenemedi"
          : "Konum isteği zaman aşımına uğradı";
      if (nearbyStateEl)
        nearbyStateEl.textContent =
          reason + " — Yakındakileri göstermek için konum izni vermelisin 🗺️";
    },
    { enableHighAccuracy: true, timeout: 10000, maximumAge: 0 }
  );
}

function computeNearby(uLat, uLng) {
  const withDist = (latestGyms || [])
    .filter((g) => g && g.latitude != null && g.longitude != null)
    .map((g) => ({
      ...g,
      _distKm: haversineKm(uLat, uLng, Number(g.latitude), Number(g.longitude)),
    }))
    .sort((a, b) => a._distKm - b._distKm)
    .slice(0, 3);

  renderNearby(withDist);
  if (nearbyStateEl) nearbyStateEl.hidden = true;
  if (nearbyListEl) nearbyListEl.hidden = false;
  nearestDone = true;
}

function renderNearby(list) {
  if (!nearbyListEl) return;
  nearbyListEl.innerHTML = "";
  if (!list || !list.length) {
    nearbyListEl.innerHTML =
      '<div class="text-muted p-2">Yakında salon bulunamadı 🙃</div>';
    return;
  }

  list.forEach((g) => {
    const id = String(g.id ?? "");
    const div = document.createElement("div");
    div.className = "nearby-card";
    div.innerHTML = `
      <div class="nearby-left">
        <div><strong>${escapeHtml(g.name || "Salon")}</strong></div>
        <div class="nearby-meta">${escapeHtml(g.cityName || "")}${
      g.address ? " • " + escapeHtml(g.address) : ""
    }</div>
        <div class="nearby-meta">~ ${Number(g._distKm).toFixed(1)} km</div>
      </div>
      <div class="nearby-actions">
        <button class="btn btn-sm btn-outline-primary" data-gym-id="${id}">
          <i class="bi bi-geo-alt"></i> Göster
        </button>
      </div>
    `;
    nearbyListEl.appendChild(div);
  });

  nearbyListEl.querySelectorAll("button[data-gym-id]").forEach((btn) => {
    on(btn, "click", () => {
      const id = btn.getAttribute("data-gym-id");
      const marker = markerById.get(String(id));
      if (marker) {
        const ll = marker.getLatLng();
        map.setView(ll, Math.max(map.getZoom(), 15));
        marker.openPopup && marker.openPopup();
      } else {
        const g = latestGyms.find((x) => String(x.id ?? "") === String(id));
        if (g && g.latitude != null && g.longitude != null) {
          map.setView([g.latitude, g.longitude], 15);
        }
      }
    });
  });
}

function haversineKm(lat1, lon1, lat2, lon2) {
  const R = 6371;
  const dLat = ((lat2 - lat1) * Math.PI) / 180;
  const dLon = ((lon2 - lon1) * Math.PI) / 180;
  const a =
    Math.sin(dLat / 2) ** 2 +
    Math.cos((lat1 * Math.PI) / 180) *
      Math.cos((lat2 * Math.PI) / 180) *
      Math.sin(dLon / 2) ** 2;
  const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
  return R * c;
}

function escapeHtml(s) {
  return String(s ?? "").replace(
    /[&<>"']/g,
    (m) =>
      ({
        "&": "&amp;",
        "<": "&lt;",
        ">": "&gt;",
        '"': "&quot;",
        "'": "&#39;",
      }[m])
  );
}

function initMap() {
  map = L.map("trMap", {
    zoomControl: false,
  }).setView([39.0, 35.0], 6);

  currentTileLayer = L.tileLayer(mapStyles.light.url, {
    maxZoom: 19,
    attribution: mapStyles.light.attribution,
  }).addTo(map);

  markersLayer = L.layerGroup().addTo(map);
}

function initUI() {
  panelFilterBtn = document.getElementById("panelFilterBtn");
  cityDropdown = document.getElementById("cityDropdown");
  cityItemsWrap = document.getElementById("cityItems");
  searchBox = document.getElementById("searchBox");
  storeListEl = document.getElementById("storeList");


  nearbyStateEl = document.getElementById("nearbyState");
  nearbyListEl = document.getElementById("nearbyList");


  initMapStyleSwitcher();
}

function initTabs() {

  initNearbyRefs();
}

function initMapStyleSwitcher() {
  const dropdownBtn = document.getElementById("mapStyleDropdown");
  const dropdownMenu = dropdownBtn?.nextElementSibling;

  if (!dropdownBtn || !dropdownMenu) return;

  dropdownBtn.addEventListener("click", (e) => {
    e.preventDefault();
    e.stopPropagation();
    dropdownMenu.classList.toggle("show");
  });

  document.addEventListener("click", (e) => {
    if (!dropdownBtn.contains(e.target) && !dropdownMenu.contains(e.target)) {
      dropdownMenu.classList.remove("show");
    }
  });

  dropdownMenu.querySelectorAll(".dropdown-item").forEach((item) => {
    item.addEventListener("click", (e) => {
      e.preventDefault();
      const styleName = item.getAttribute("data-style");
      if (styleName && mapStyles[styleName]) {
        changeMapStyle(styleName);
        dropdownMenu.classList.remove("show");
      }
    });
  });
}

function init() {
  initMap();
  initUI();
  initPanelToggle();
  initSearch();
  initCityDropdown();
  initTabs();

  loadCities();
  loadMarkers(false).finally(() => {
    renderList(filterBySearch(latestGyms, searchBox ? searchBox.value : ""));
  });
}

document.addEventListener("DOMContentLoaded", init);
