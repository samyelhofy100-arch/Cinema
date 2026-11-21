(function () {
    "use strict";

    // خريطة الكراسي
    var form = document.getElementById("seatForm");
    if (form) {
        var max = parseInt(form.getAttribute("data-max"), 10) || 8;
        var price = parseFloat(form.getAttribute("data-price")) || 0;
        var boxes = Array.prototype.slice.call(form.querySelectorAll('input[name="seatIds"]'));
        var count = document.getElementById("bkCount");
        var total = document.getElementById("bkTotal");

        var update = function () {
            var n = boxes.filter(function (b) { return b.checked && !b.disabled; }).length;
            count.textContent = n;
            total.textContent = (n * price).toFixed(2).replace(/\.00$/, "");
        };

        boxes.forEach(function (b) {
            b.addEventListener("change", function () {
                var n = boxes.filter(function (x) { return x.checked && !x.disabled; }).length;
                if (n > max) {
                    b.checked = false;
                    window.alert("You can select up to " + max + " seats.");
                }
                update();
            });
        });
        update();
    }

    // عدّاد الحجز
    document.querySelectorAll("[data-seconds]").forEach(function (el) {
        var left = parseInt(el.getAttribute("data-seconds"), 10) || 0;
        var tick = function () {
            if (left <= 0) { el.textContent = "Expired"; window.location.reload(); return; }
            var m = Math.floor(left / 60), s = left % 60;
            el.textContent = (m < 10 ? "0" : "") + m + ":" + (s < 10 ? "0" : "") + s;
            left--;
            window.setTimeout(tick, 1000);
        };
        tick();
    });
})();