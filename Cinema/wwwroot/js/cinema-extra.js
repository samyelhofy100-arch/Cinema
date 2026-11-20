
(function () {
    "use strict";

    // Quantity buttons
    document.querySelectorAll(".qty-btn[data-qty]").forEach(function (btn) {

        btn.addEventListener("click", function () {

            var input = btn.parentElement.querySelector(".qty-input");

            if (!input) return;

            var min = parseInt(input.min || "1", 10);
            var max = parseInt(input.max || "10", 10);

            var current = parseInt(input.value, 10) || min;

            var change = parseInt(
                btn.getAttribute("data-qty"),
                10
            );

            var next = current + change;

            input.value = Math.min(
                max,
                Math.max(min, next)
            );

            input.dispatchEvent(new Event("change", {
                bubbles: true
            }));
        });

    });

})();
