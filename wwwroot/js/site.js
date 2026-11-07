document.addEventListener("DOMContentLoaded", function () {
    // 1. Bộ đếm ngược Giờ vàng Giá sốc (Flash Sale)
    var hoursEl = document.getElementById("cd-hours");
    var minsEl = document.getElementById("cd-mins");
    var secsEl = document.getElementById("cd-secs");

    if (hoursEl && minsEl && secsEl) {
        var totalSeconds = 5 * 3600 + 42 * 60 + 19;
        setInterval(function () {
            if (totalSeconds <= 0) {
                totalSeconds = 6 * 3600;
            }
            totalSeconds--;
            var h = Math.floor(totalSeconds / 3600);
            var m = Math.floor((totalSeconds % 3600) / 60);
            var s = totalSeconds % 60;

            hoursEl.textContent = h < 10 ? "0" + h : h;
            minsEl.textContent = m < 10 ? "0" + m : m;
            secsEl.textContent = s < 10 ? "0" + s : s;
        }, 1000);
    }

    // 2. Hiển thị thông báo tự động bằng SweetAlert2
    if (typeof Swal !== "undefined" && window.dmxNotification && window.dmxNotification.message) {
        var notif = window.dmxNotification;
        if (notif.type === "success") {
            Swal.fire({
                icon: "success",
                title: "Thành công",
                text: notif.message,
                toast: true,
                position: "top-end",
                showConfirmButton: false,
                timer: 3200,
                timerProgressBar: true
            });
        } else if (notif.type === "error") {
            Swal.fire({
                icon: "error",
                title: "Thông báo",
                text: notif.message,
                confirmButtonColor: "#2a83e9",
                confirmButtonText: "Đã hiểu"
            });
        }
    }

    // 3. Hộp thoại xác nhận SweetAlert2 cho các biểu mẫu (.swal-confirm-form)
    var confirmForms = document.querySelectorAll(".swal-confirm-form");
    confirmForms.forEach(function (form) {
        form.addEventListener("submit", function (e) {
            if (form.dataset.swalConfirmed === "true") {
                return;
            }
            e.preventDefault();

            var title = form.getAttribute("data-swal-title") || "Xác nhận thao tác?";
            var text = form.getAttribute("data-swal-text") || "Bạn có chắc chắn muốn thực hiện hành động này không?";
            var icon = form.getAttribute("data-swal-icon") || "warning";
            var confirmText = form.getAttribute("data-swal-confirm") || "Đồng ý";
            var confirmColor = form.getAttribute("data-swal-color") || "#d0021c";

            if (typeof Swal !== "undefined") {
                Swal.fire({
                    title: title,
                    text: text,
                    icon: icon,
                    showCancelButton: true,
                    confirmButtonColor: confirmColor,
                    cancelButtonColor: "#667085",
                    confirmButtonText: confirmText,
                    cancelButtonText: "Hủy bỏ",
                    reverseButtons: true
                }).then(function (result) {
                    if (result.isConfirmed) {
                        form.dataset.swalConfirmed = "true";
                        form.submit();
                    }
                });
            } else {
                form.dataset.swalConfirmed = "true";
                form.submit();
            }
        });
    });

    // 4. Hộp thoại xác nhận SweetAlert2 cho các liên kết (.swal-confirm-link)
    var confirmLinks = document.querySelectorAll(".swal-confirm-link");
    confirmLinks.forEach(function (link) {
        link.addEventListener("click", function (e) {
            e.preventDefault();
            var href = link.getAttribute("href");
            var title = link.getAttribute("data-swal-title") || "Xác nhận?";
            var text = link.getAttribute("data-swal-text") || "Bạn có chắc chắn muốn tiếp tục?";
            var icon = link.getAttribute("data-swal-icon") || "question";

            if (typeof Swal !== "undefined") {
                Swal.fire({
                    title: title,
                    text: text,
                    icon: icon,
                    showCancelButton: true,
                    confirmButtonColor: "#2a83e9",
                    cancelButtonColor: "#667085",
                    confirmButtonText: "Đồng ý",
                    cancelButtonText: "Hủy bỏ",
                    reverseButtons: true
                }).then(function (result) {
                    if (result.isConfirmed && href) {
                        window.location.href = href;
                    }
                });
            } else if (href) {
                window.location.href = href;
            }
        });
    });

    // 5. TÌM KIẾM TRỰC TIẾP (LIVE SEARCH / AUTOCOMPLETE)
    var searchInput = document.getElementById("liveSearchInput");
    var searchDropdown = document.getElementById("liveSearchDropdown");
    var searchTimer = null;

    if (searchInput && searchDropdown) {
        function closeSearchDropdown() {
            searchDropdown.classList.add("d-none");
            searchDropdown.innerHTML = "";
        }

        searchInput.addEventListener("input", function () {
            var val = this.value.trim();
            clearTimeout(searchTimer);

            if (val.length < 2) {
                closeSearchDropdown();
                return;
            }

            searchTimer = setTimeout(function () {
                fetch("/Product/LiveSearch?keyword=" + encodeURIComponent(val))
                    .then(function (res) { return res.json(); })
                    .then(function (data) {
                        if (!data || !data.success || !data.products || data.products.length === 0) {
                            searchDropdown.innerHTML = '<div class="p-3 text-center text-muted small"><i class="bi bi-search me-1"></i>Không tìm thấy sản phẩm phù hợp với "' + val + '"</div>';
                            searchDropdown.classList.remove("d-none");
                            return;
                        }

                        var html = "";
                        data.products.forEach(function (p) {
                            html += '<a href="/Product/Detail/' + p.id + '" class="dmx-search-item">';
                            html += '  <img src="' + p.imageUrl + '" alt="' + p.name + '" class="dmx-search-item-img" />';
                            html += '  <div class="dmx-search-item-info">';
                            html += '    <div class="d-flex align-items-center gap-1 mb-1">';
                            html += '      <span class="badge bg-light text-secondary border font-monospace" style="font-size: 10px;">' + p.code + '</span>';
                            if (p.categoryName) {
                                html += '      <span class="badge bg-primary-subtle text-primary" style="font-size: 10px;">' + p.categoryName + '</span>';
                            }
                            html += '    </div>';
                            html += '    <div class="dmx-search-item-title" title="' + p.name + '">' + p.name + '</div>';
                            html += '    <div class="dmx-search-item-prices">';
                            html += '      <span class="dmx-search-item-price">' + p.formattedPrice + '</span>';
                            if (p.originalPrice > p.price) {
                                html += '      <span class="dmx-search-item-orig">' + p.formattedOriginalPrice + '</span>';
                                html += '      <span class="badge bg-danger text-white dmx-search-item-badge">-' + p.discountPercent + '%</span>';
                            }
                            html += '    </div>';
                            html += '  </div>';
                            html += '</a>';
                        });

                        html += '<a href="/Product?keyword=' + encodeURIComponent(val) + '" class="dmx-search-footer">';
                        html += '  Xem tất cả ' + data.totalMatches + ' kết quả cho "' + val + '" <i class="bi bi-arrow-right"></i>';
                        html += '</a>';

                        searchDropdown.innerHTML = html;
                        searchDropdown.classList.remove("d-none");
                    })
                    .catch(function () {
                        closeSearchDropdown();
                    });
            }, 250);
        });

        document.addEventListener("click", function (e) {
            if (!searchInput.contains(e.target) && !searchDropdown.contains(e.target)) {
                closeSearchDropdown();
            }
        });

        searchInput.addEventListener("keydown", function (e) {
            if (e.key === "Escape") {
                closeSearchDropdown();
            }
        });
    }

    // 6. THANH SO SÁNH SẢN PHẨM NỔI (COMPARE BAR)
    var compareBar = document.getElementById("dmxCompareBar");
    var compareItemsContainer = document.getElementById("dmxCompareItems");
    var compareCountSpan = document.getElementById("dmxCompareCount");
    var btnClearCompare = document.getElementById("btnClearCompareBar");

    function renderCompareBar(data) {
        if (!compareBar || !compareItemsContainer || !compareCountSpan) return;

        if (!data || data.count === 0 || !data.items || data.items.length === 0) {
            compareBar.classList.add("d-none");
            compareItemsContainer.innerHTML = "";
            compareCountSpan.textContent = "0";
            return;
        }

        compareCountSpan.textContent = data.count;
        var html = '<span class="fw-bold small text-primary d-none d-md-inline"><i class="bi bi-arrow-left-right me-1"></i>So sánh:</span>';
        data.items.forEach(function (item) {
            html += '<div class="dmx-compare-item">';
            html += '  <img src="' + item.imageUrl + '" alt="' + item.name + '" />';
            html += '  <div class="dmx-compare-item-name" title="' + item.name + '">' + item.name + '</div>';
            html += '  <button type="button" class="dmx-compare-item-del btn-del-compare-single" data-id="' + item.id + '" title="Bỏ">x</button>';
            html += '</div>';
        });

        compareItemsContainer.innerHTML = html;
        compareBar.classList.remove("d-none");

        // Gắn sự kiện xóa từng item
        document.querySelectorAll(".btn-del-compare-single").forEach(function (btn) {
            btn.addEventListener("click", function (e) {
                e.stopPropagation();
                var pid = this.dataset.id;
                fetch("/Product/RemoveFromCompare?productId=" + pid, { method: "POST" })
                    .then(function (r) { return r.json(); })
                    .then(function () { fetchCompareBar(); });
            });
        });
    }

    function fetchCompareBar() {
        if (!compareBar) return;
        fetch("/Product/GetCompareBar")
            .then(function (r) { return r.json(); })
            .then(function (data) {
                renderCompareBar(data);
            })
            .catch(function () {});
    }

    // Nút so sánh trên card và trang chi tiết
    document.querySelectorAll(".btn-card-compare").forEach(function (btn) {
        btn.addEventListener("click", function (e) {
            e.preventDefault();
            var pid = this.dataset.id;
            fetch("/Product/AddToCompare?productId=" + pid, { method: "POST" })
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    if (typeof Swal !== "undefined") {
                        Swal.fire({
                            icon: data.success ? "success" : "info",
                            title: data.success ? "Đã thêm vào so sánh" : "Thông báo",
                            text: data.message,
                            toast: true,
                            position: "top-end",
                            showConfirmButton: false,
                            timer: 2600,
                            timerProgressBar: true
                        });
                    }
                    fetchCompareBar();
                });
        });
    });

    if (btnClearCompare) {
        btnClearCompare.addEventListener("click", function () {
            fetch("/Product/ClearCompare", { method: "POST" })
                .then(function (r) { return r.json(); })
                .then(function () {
                    fetchCompareBar();
                });
        });
    }

    fetchCompareBar();
});
