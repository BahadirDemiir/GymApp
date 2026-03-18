document.addEventListener("DOMContentLoaded", function () {
 let map, marker;

  initializeMap();
  initializeImageUpload();
  initializeFormValidation();

  function showMapMessage(message, type) {
    let messageElement = document.querySelector(".map-message");
    if (!messageElement) {
      messageElement = document.createElement("div");
      messageElement.className =
        "map-message alert alert-" + type + " alert-dismissible fade show";
      messageElement.innerHTML = `
                      ${message}
                      <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                  `;

      const mapContainer = document.querySelector(".map-container");
      if (mapContainer) {
        mapContainer.insertBefore(messageElement, mapContainer.firstChild);
      }
    } else {
      messageElement.className =
        "map-message alert alert-" + type + " alert-dismissible fade show";
      messageElement.innerHTML = `
                      ${message}
                      <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                  `;
    }

    setTimeout(() => {
      if (messageElement) {
        messageElement.remove();
      }
    }, 3000);
  }

  function waitForLeaflet() {
    return new Promise((resolve, reject) => {
      let attempts = 0;
      const maxAttempts = 50; 

      const checkLeaflet = () => {
        attempts++;
        if (typeof L !== "undefined") {
          resolve();
        } else if (attempts >= maxAttempts) {
          reject();
        } else {
          setTimeout(checkLeaflet, 100);
        }
      };

      checkLeaflet();
    });
  }

  function initializeMap() {
    const mapContainer = document.getElementById("map");
    if (!mapContainer) {
      console.error("Harita konteyneri bulunamadı!");
      return;
    }

    waitForLeaflet()
      .then(() => {
        createMap();
      })
      .catch(() => {
        console.error("Leaflet yüklenemedi");
        showMapMessage(
          "Harita kütüphanesi yüklenemedi. Lütfen sayfayı yenileyin.",
          "danger"
        );
      });
  }

  function createMap() {
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        function (position) {
          const userLat = position.coords.latitude;
          const userLng = position.coords.longitude;

          createMapWithLocation(userLat, userLng, 15);

          showMapMessage("Mevcut konumunuz haritaya yüklendi!", "success");
        },
        function (error) {
          console.log(
            "Konum alınamadı, İstanbul varsayılan olarak kullanılıyor:",
            error.message
          );
          createMapWithLocation(41.0082, 28.9784, 10);
        },
        {
          enableHighAccuracy: true,
          timeout: 10000,
          maximumAge: 60000,
        }
      );
    } else {
      createMapWithLocation(41.0082, 28.9784, 10);
    }
  }

      function createMapWithLocation(lat, lng, zoom) {
        console.log("Harita oluşturuluyor:", lat, lng, zoom);

        map = L.map("map").setView([lat, lng], zoom);
        console.log("Harita oluşturuldu:", map);


        L.tileLayer(
          "https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png",
          {
            attribution:
              '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>',
            subdomains: "abcd",
            maxZoom: 20,
          }
        ).addTo(map);

        marker = L.marker([lat, lng], {
          draggable: true,
        }).addTo(map);

        marker.on("dragend", function (e) {
          const newLat = e.target.getLatLng().lat;
          const newLng = e.target.getLatLng().lng;
          updateCoordinates(newLat, newLng);
          updateCurrentCoordsDisplay(newLat, newLng);
        });

        updateCoordinates(lat, lng);
        updateCurrentCoordsDisplay(lat, lng);

        setupMapEventListeners();
      }

  function setupMapEventListeners() {
    const getCurrentLocationBtn = document.getElementById("getCurrentLocation");
    if (getCurrentLocationBtn) {
      getCurrentLocationBtn.addEventListener("click", function () {
        if (navigator.geolocation) {
          this.innerHTML =
            '<i class="bi bi-hourglass-split me-2"></i>Konum alınıyor...';
          this.disabled = true;

          navigator.geolocation.getCurrentPosition(
            function (position) {
              const lat = position.coords.latitude;
              const lng = position.coords.longitude;

              map.setView([lat, lng], 15);
              marker.setLatLng([lat, lng]);
              updateCoordinates(lat, lng);
              updateCurrentCoordsDisplay(lat, lng);

              getCurrentLocationBtn.innerHTML =
                '<i class="bi bi-geo-alt me-2"></i>Mevcut Konumu Al';
              getCurrentLocationBtn.disabled = false;

              showMapMessage("Mevcut konumunuz haritaya eklendi!", "success");
            },
            function (error) {
              getCurrentLocationBtn.innerHTML =
                '<i class="bi bi-geo-alt me-2"></i>Mevcut Konumu Al';
              getCurrentLocationBtn.disabled = false;
              showMapMessage("Konum alınamadı: " + error.message, "warning");
            }
          );
        } else {
          showMapMessage(
            "Tarayıcınız konum özelliğini desteklemiyor.",
            "warning"
          );
        }
      });
    }

    map.on("click", function (e) {
      const lat = e.latlng.lat;
      const lng = e.latlng.lng;

      marker.setLatLng([lat, lng]);

      updateCoordinates(lat, lng);
      updateCurrentCoordsDisplay(lat, lng);

      showMapMessage(
        `Yeni konum seçildi: ${lat.toFixed(6)}, ${lng.toFixed(6)}`,
        "info"
      );
    });

    const openGoogleMapsBtn = document.getElementById("openGoogleMaps");
    if (openGoogleMapsBtn) {
      openGoogleMapsBtn.addEventListener("click", function () {
        const lat = parseFloat(document.getElementById("latitude").value);
        const lng = parseFloat(document.getElementById("longitude").value);

        if (!isNaN(lat) && !isNaN(lng)) {
          const mapsUrl = `https://www.google.com/maps?q=${lat},${lng}`;
          window.open(mapsUrl, "_blank");
        } else {
          showMapMessage("Önce koordinatları giriniz.", "warning");
        }
      });
    }

    const centerMapBtn = document.getElementById("centerMap");
    if (centerMapBtn) {
      centerMapBtn.addEventListener("click", function () {
        const lat = parseFloat(document.getElementById("latitude").value);
        const lng = parseFloat(document.getElementById("longitude").value);

        if (!isNaN(lat) && !isNaN(lng)) {
          map.setView([lat, lng], 15);
          marker.setLatLng([lat, lng]);
          showMapMessage("Harita merkezlendi!", "info");
        } else {
          showMapMessage("Önce koordinatları giriniz.", "warning");
        }
      });
    }

    document.getElementById("latitude").addEventListener("change", function () {
      const lat = parseFloat(this.value);
      const lng = parseFloat(document.getElementById("longitude").value);

      if (!isNaN(lat) && !isNaN(lng)) {
        map.setView([lat, lng], 15);
        marker.setLatLng([lat, lng]);
        updateCurrentCoordsDisplay(lat, lng);
      }
    });

    document
      .getElementById("longitude")
      .addEventListener("change", function () {
        const lat = parseFloat(document.getElementById("latitude").value);
        const lng = parseFloat(this.value);

        if (!isNaN(lat) && !isNaN(lng)) {
          map.setView([lat, lng], 15);
          marker.setLatLng([lat, lng]);
          updateCurrentCoordsDisplay(lat, lng);
        }
      });
  }

  function updateCoordinates(lat, lng) {
    document.getElementById("latitude").value = lat.toFixed(6);
    document.getElementById("longitude").value = lng.toFixed(6);
  }

  function updateCurrentCoordsDisplay(lat, lng) {
    const coordsElement = document.getElementById("currentCoords");
    if (coordsElement) {
      coordsElement.textContent = `${lat.toFixed(6)}, ${lng.toFixed(6)}`;
    }
  }

  let selectedFiles = [];

  function initializeImageUpload() {
    const uploadArea = document.getElementById("uploadArea");
    const fileInput = document.getElementById("gymImage");
    const previewContainer = document.getElementById("imagePreviewGrid");

    if (!uploadArea || !fileInput || !previewContainer) {
      console.log("Image upload elements not found");
      return;
    }

    uploadArea.addEventListener("dragover", function (e) {
      e.preventDefault();
      uploadArea.classList.add("dragover");
    });

    uploadArea.addEventListener("dragleave", function (e) {
      e.preventDefault();
      uploadArea.classList.remove("dragover");
    });

    uploadArea.addEventListener("drop", function (e) {
      e.preventDefault();
      uploadArea.classList.remove("dragover");

      const files = Array.from(e.dataTransfer.files);
      handleFiles(files);
    });

    fileInput.addEventListener("change", function (e) {
      const files = Array.from(e.target.files);
      handleFiles(files);
    });

    uploadArea.addEventListener("click", function () {
      fileInput.click();
    });

    function handleFiles(files) {
      const imageFiles = files.filter((file) => file.type.startsWith("image/"));

      if (imageFiles.length === 0) {
        showMapMessage("Lütfen sadece resim dosyaları seçin.", "warning");
        return;
      }

      selectedFiles = [...selectedFiles, ...imageFiles];

      const dataTransfer = new DataTransfer();
      selectedFiles.forEach((file) => dataTransfer.items.add(file));
      fileInput.files = dataTransfer.files;

      updateImagePreview();

      const previewContainerElement = document.getElementById(
        "imagePreviewContainer"
      );
      if (previewContainerElement) {
        previewContainerElement.style.display = "block";
      }
    }

    function updateImagePreview() {
      previewContainer.innerHTML = "";

      selectedFiles.forEach((file, index) => {
        const reader = new FileReader();
        reader.onload = function (e) {
          const previewItem = document.createElement("div");
          previewItem.className = "image-preview-item";
          previewItem.innerHTML = `
                        <img src="${e.target.result}" alt="Preview ${
            index + 1
          }">
                        <button type="button" class="remove-btn remove-image" data-index="${index}">
                            <i class="bi bi-x"></i>
                        </button>
                    `;

          previewContainer.appendChild(previewItem);
        };
        reader.readAsDataURL(file);
      });

      previewContainer.addEventListener("click", function (e) {
        if (e.target.closest(".remove-image")) {
          const index = parseInt(
            e.target.closest(".remove-image").dataset.index
          );
          removeImage(index);
        }
      });
    }

    function removeImage(index) {
      selectedFiles.splice(index, 1);

      const dataTransfer = new DataTransfer();
      selectedFiles.forEach((file) => dataTransfer.items.add(file));
      fileInput.files = dataTransfer.files;

      updateImagePreview();

      if (selectedFiles.length === 0) {
        const previewContainerElement = document.getElementById(
          "imagePreviewContainer"
        );
        if (previewContainerElement) {
          previewContainerElement.style.display = "none";
        }
      }

      showMapMessage("Resim kaldırıldı.", "info");
    }
  }

  function initializeFormValidation() {
    const form = document.getElementById("addGymForm");
    if (!form) return;

    form.addEventListener("submit", function (e) {
      const requiredFields = form.querySelectorAll("[required]");
      let isValid = true;

      requiredFields.forEach((field) => {
        if (!field.value.trim()) {
          field.classList.add("is-invalid");
          isValid = false;
        } else {
          field.classList.remove("is-invalid");
          field.classList.add("is-valid");
        }
      });

      const lat = parseFloat(document.getElementById("latitude").value);
      const lng = parseFloat(document.getElementById("longitude").value);

      if (isNaN(lat) || lat < -90 || lat > 90) {
        document.getElementById("latitude").classList.add("is-invalid");
        isValid = false;
      } else {
        document.getElementById("latitude").classList.remove("is-invalid");
        document.getElementById("latitude").classList.add("is-valid");
      }

      if (isNaN(lng) || lng < -180 || lng > 180) {
        document.getElementById("longitude").classList.add("is-invalid");
        isValid = false;
      } else {
        document.getElementById("longitude").classList.remove("is-invalid");
        document.getElementById("longitude").classList.add("is-valid");
      }

      if (!isValid) {
        e.preventDefault();
        const firstError = form.querySelector(".is-invalid");
        if (firstError) {
          firstError.scrollIntoView({ behavior: "smooth", block: "center" });
        }
      }
    });
  }
});
