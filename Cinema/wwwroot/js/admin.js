(function () {
    "use strict";

    // 1) Confirm before any delete form
    document.querySelectorAll("form[data-confirm]").forEach(function (form) {
        form.addEventListener("submit", function (e) {
            if (!window.confirm(form.getAttribute("data-confirm"))) {
                e.preventDefault();
            }
        });
    });

    // 2) Live preview of the main image / photo
    var mainInput = document.querySelector("[data-preview-main]");
    var mainPreview = document.getElementById("mainPreview");
    var mainHint = document.getElementById("mainPreviewHint");
    if (mainInput && mainPreview) {
        mainInput.addEventListener("change", function () {
            var file = mainInput.files && mainInput.files[0];
            if (!file) { return; }
            mainPreview.src = URL.createObjectURL(file);
            mainPreview.style.display = "block";
            if (mainHint) { mainHint.style.display = "none"; }
        });
    }

    // 3) Live preview of the extra images
    var multiInput = document.querySelector("[data-preview-multi]");
    var subPreview = document.getElementById("subPreview");
    if (multiInput && subPreview) {
        multiInput.addEventListener("change", function () {
            subPreview.innerHTML = "";
            subPreview.classList.add("adm-subs--new");
            Array.prototype.forEach.call(multiInput.files, function (file) {
                var box = document.createElement("div");
                box.className = "adm-sub";
                var img = document.createElement("img");
                img.src = URL.createObjectURL(file);
                img.alt = "";
                box.appendChild(img);
                subPreview.appendChild(box);
            });
        });
    }

    // 4) Filter the cast picker
    var actorFilter = document.querySelector("[data-actor-filter]");
    if (actorFilter) {
        var actors = document.querySelectorAll(".adm-actor");
        actorFilter.addEventListener("input", function () {
            var term = actorFilter.value.trim().toLowerCase();
            actors.forEach(function (a) {
                var match = a.getAttribute("data-name").toLowerCase().indexOf(term) !== -1;
                a.style.display = match ? "" : "none";
            });
        });
    }

    // 5) Mobile sidebar
    var burger = document.getElementById("admBurger");
    var sidebar = document.getElementById("admSidebar");
    if (burger && sidebar) {
        burger.addEventListener("click", function () { sidebar.classList.toggle("open"); });
        document.addEventListener("click", function (e) {
            if (sidebar.classList.contains("open") && !sidebar.contains(e.target) && e.target !== burger) {
                sidebar.classList.remove("open");
            }
        });
    }

    // 6) Auto-hide success messages
    document.querySelectorAll(".alert-success").forEach(function (el) {
        setTimeout(function () {
            el.style.transition = "opacity .5s";
            el.style.opacity = "0";
            setTimeout(function () { el.remove(); }, 500);
        }, 4000);
    });
})();
