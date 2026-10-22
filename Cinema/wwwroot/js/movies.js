(function () {
    "use strict";

    // 1) Auto-submit when the category changes
    var categorySelect = document.getElementById("categorySelect");
    if (categorySelect) {
        categorySelect.addEventListener("change", function () {
            document.getElementById("filterForm").submit();
        });
    }

    // 2) Fade-in cards as they enter the viewport
    var cards = document.querySelectorAll(".movie-card");
    if ("IntersectionObserver" in window) {
        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    var card = entry.target;
                    var index = Array.prototype.indexOf.call(cards, card);
                    card.style.transitionDelay = (index % 4) * 70 + "ms";
                    card.classList.add("is-visible");
                    observer.unobserve(card);
                }
            });
        }, { threshold: 0.1 });
        cards.forEach(function (card) { observer.observe(card); });
    } else {
        cards.forEach(function (card) { card.classList.add("is-visible"); });
    }

    // 3) Placeholder if an image fails to load
    var placeholder =
        "data:image/svg+xml;utf8," +
        encodeURIComponent(
            '<svg xmlns="http://www.w3.org/2000/svg" width="400" height="600">' +
            '<rect width="100%" height="100%" fill="#dfe1ea"/>' +
            '<text x="50%" y="50%" fill="#8b8fa8" font-family="Arial" font-size="28" ' +
            'text-anchor="middle">No image</text></svg>'
        );
    document.querySelectorAll("img[data-fallback]").forEach(function (img) {
        img.addEventListener("error", function () {
            if (img.src !== placeholder) { img.src = placeholder; }
        });
    });

    // 4) Back to top button
    var backToTop = document.getElementById("backToTop");
    if (backToTop) {
        window.addEventListener("scroll", function () {
            backToTop.classList.toggle("show", window.scrollY > 400);
        });
        backToTop.addEventListener("click", function () {
            window.scrollTo({ top: 0, behavior: "smooth" });
        });
    }
})();
