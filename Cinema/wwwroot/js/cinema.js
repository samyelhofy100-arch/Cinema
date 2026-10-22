(function () {
    "use strict";

    // 1) Reveal elements as they scroll into view
    var items = document.querySelectorAll(".reveal");
    if ("IntersectionObserver" in window) {
        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    var el = entry.target;
                    var index = Array.prototype.indexOf.call(items, el);
                    el.style.transitionDelay = (index % 6) * 60 + "ms";
                    el.classList.add("is-visible");
                    observer.unobserve(el);
                }
            });
        }, { threshold: 0.08 });
        items.forEach(function (el) { observer.observe(el); });
    } else {
        items.forEach(function (el) { el.classList.add("is-visible"); });
    }

    // 2) Placeholder for images that fail to load
    var placeholder =
        "data:image/svg+xml;utf8," +
        encodeURIComponent(
            '<svg xmlns="http://www.w3.org/2000/svg" width="400" height="400">' +
            '<rect width="100%" height="100%" fill="#dfe1ea"/>' +
            '<text x="50%" y="50%" fill="#8b8fa8" font-family="Arial" font-size="28" ' +
            'text-anchor="middle">No image</text></svg>'
        );
    function useFallback(img) {
        if (img.src !== placeholder) { img.src = placeholder; }
    }
    document.addEventListener("error", function (e) {
        var t = e.target;
        if (t && t.tagName === "IMG" && t.hasAttribute("data-fallback")) { useFallback(t); }
    }, true);
    document.querySelectorAll("img[data-fallback]").forEach(function (img) {
        if (img.complete && img.naturalWidth === 0) { useFallback(img); }
    });

    // 3) Lightbox: the clicked gallery thumbnail fills the modal image
    var lightbox = document.getElementById("lightbox");
    if (lightbox) {
        lightbox.addEventListener("show.bs.modal", function (event) {
            var trigger = event.relatedTarget;
            var img = document.getElementById("lightboxImg");
            if (trigger && img) {
                img.src = trigger.getAttribute("data-src");
            }
        });
    }

    // 4) Live filter for the Cinemas / Actors pages
    var input = document.querySelector("[data-filter-input]");
    if (input) {
        var cards = document.querySelectorAll("[data-filter-item]");
        var empty = document.querySelector(".empty-state");
        input.addEventListener("input", function () {
            var term = input.value.trim().toLowerCase();
            var visible = 0;
            cards.forEach(function (card) {
                var match = card.getAttribute("data-name").toLowerCase().indexOf(term) !== -1;
                card.style.display = match ? "" : "none";
                if (match) { visible++; }
            });
            if (empty) { empty.classList.toggle("show", visible === 0); }
        });
    }
})();
