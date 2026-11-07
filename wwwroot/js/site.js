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

    // 7. NÚT CUỘN VỀ ĐẦU TRANG (SCROLL TO TOP)
    var scrollTopBtn = document.getElementById("dmxScrollTopBtn");
    if (scrollTopBtn) {
        window.addEventListener("scroll", function () {
            if (window.scrollY > 300) {
                scrollTopBtn.classList.add("visible");
            } else {
                scrollTopBtn.classList.remove("visible");
            }
        });

        scrollTopBtn.addEventListener("click", function () {
            window.scrollTo({
                top: 0,
                behavior: "smooth"
            });
        });
    }

    // 8. CHATBOT TRỢ LÝ ẢO TƯ VẤN BÁN HÀNG TỰ ĐỘNG
    var chatbotToggle = document.getElementById("dmxChatbotToggle");
    var chatbotWindow = document.getElementById("dmxChatbotWindow");
    var closeChatbot = document.getElementById("dmxCloseChatbot");
    var chatForm = document.getElementById("dmxChatForm");
    var chatInput = document.getElementById("dmxChatInput");
    var chatMessages = document.getElementById("dmxChatMessages");

    function scrollChatToBottom() {
        if (chatMessages) {
            chatMessages.scrollTop = chatMessages.scrollHeight;
        }
    }

    if (chatbotToggle && chatbotWindow && closeChatbot) {
        chatbotToggle.addEventListener("click", function () {
            chatbotWindow.classList.toggle("d-none");
            if (!chatbotWindow.classList.contains("d-none")) {
                scrollChatToBottom();
                if (chatInput) {
                    chatInput.focus();
                }
            }
        });

        closeChatbot.addEventListener("click", function () {
            chatbotWindow.classList.add("d-none");
        });
    }

    function appendUserMessage(text) {
        if (!chatMessages) return;
        var bubble = document.createElement("div");
        bubble.className = "dmx-chat-bubble user";
        bubble.textContent = text;
        chatMessages.appendChild(bubble);
        scrollChatToBottom();
    }

    function appendBotMessage(html) {
        if (!chatMessages) return;
        var bubble = document.createElement("div");
        bubble.className = "dmx-chat-bubble bot";
        bubble.innerHTML = html;
        chatMessages.appendChild(bubble);
        scrollChatToBottom();
    }

    function getBotResponse(query) {
        var q = query.toLowerCase().trim();

        if (q.indexOf("tivi") !== -1 || q.indexOf("ti vi") !== -1 || q.indexOf("oled") !== -1 || q.indexOf("qled") !== -1) {
            return "Hệ thống đang sẵn hàng các dòng Smart Tivi 4K nổi bật:<br/>" +
                   "&bull; <strong>Sony Google TV 4K 55-65 inch</strong>: Công nghệ hình ảnh Cognitive Processor XR đỉnh cao.<br/>" +
                   "&bull; <strong>Samsung QLED / Crystal UHD</strong>: Thiết kế siêu mỏng AirSlim, âm thanh vòm sống động.<br/>" +
                   "&bull; <strong>LG OLED evo</strong>: Độ tương phản vô tận, tần số quét 120Hz cho game thủ.<br/>" +
                   "<div class='mt-2'><a href='/Product?keyword=Tivi' class='btn btn-primary btn-sm py-1 px-3 rounded-pill fw-bold text-white text-decoration-none'>Xem tất cả Tivi khuyến mãi</a></div>";
        }

        if (q.indexOf("tủ lạnh") !== -1 || q.indexOf("tu lanh") !== -1 || q.indexOf("side by side") !== -1) {
            return "Điện Máy Xanh gợi ý các mẫu Tủ lạnh tiết kiệm điện bán chạy nhất:<br/>" +
                   "&bull; <strong>Panasonic Inverter</strong>: Ngăn đông mềm chuẩn Prime Fresh -3 độ C bảo quản thực phẩm trọn vị.<br/>" +
                   "&bull; <strong>Samsung Bespoke</strong>: Mặt kính đa sắc màu thời thượng, lấy nước ngoài tiện lợi.<br/>" +
                   "&bull; <strong>Toshiba Inverter</strong>: Khử mùi diệt khuẩn Ag+ Bio, tiết kiệm điện năng vượt trội.<br/>" +
                   "<div class='mt-2'><a href='/Product?keyword=Tủ lạnh' class='btn btn-primary btn-sm py-1 px-3 rounded-pill fw-bold text-white text-decoration-none'>Khám phá danh sách Tủ lạnh</a></div>";
        }

        if (q.indexOf("máy giặt") !== -1 || q.indexOf("may giat") !== -1 || q.indexOf("sấy") !== -1) {
            return "Các dòng Máy giặt công nghệ mới hỗ trợ bảo vệ sợi vải tối ưu:<br/>" +
                   "&bull; <strong>Electrolux UltimateCare</strong>: Giặt hơi nước diệt khuẩn 99.9%, chống nhăn hiệu quả.<br/>" +
                   "&bull; <strong>LG AI DD Inverter</strong>: Trí tuệ nhân tạo nhận diện chất liệu vải tự điều chỉnh chu trình giặt.<br/>" +
                   "&bull; <strong>Toshiba The GreatWaves</strong>: Giặt sạch sâu bằng sóng siêu nước, vận hành cực êm ái.<br/>" +
                   "<div class='mt-2'><a href='/Product?keyword=Máy giặt' class='btn btn-primary btn-sm py-1 px-3 rounded-pill fw-bold text-white text-decoration-none'>Xem các dòng Máy giặt</a></div>";
        }

        if (q.indexOf("máy lạnh") !== -1 || q.indexOf("may lanh") !== -1 || q.indexOf("điều hòa") !== -1 || q.indexOf("dieu hoa") !== -1) {
            return "Máy lạnh - Điều hòa không khí chính hãng kèm dịch vụ bao trọn gói công lắp đặt:<br/>" +
                   "&bull; <strong>Daikin Inverter</strong>: Luồng gió Coanda không thổi trực tiếp vào người, độ bền cao.<br/>" +
                   "&bull; <strong>Panasonic Inverter</strong>: Khử mùi lọc bụi mịn nanoe-G chuẩn sức khỏe.<br/>" +
                   "&bull; <strong>Casper Inverter</strong>: Làm lạnh nhanh Turbo, giá thành cực kỳ kinh tế.<br/>" +
                   "<div class='mt-2'><a href='/Product?keyword=Máy lạnh' class='btn btn-primary btn-sm py-1 px-3 rounded-pill fw-bold text-white text-decoration-none'>Xem khuyến mãi Máy lạnh</a></div>";
        }

        if (q.indexOf("điện thoại") !== -1 || q.indexOf("dien thoai") !== -1 || q.indexOf("iphone") !== -1 || q.indexOf("samsung") !== -1 || q.indexOf("laptop") !== -1 || q.indexOf("macbook") !== -1) {
            return "Thiết bị di động & Tin học văn phòng chính hãng 100%:<br/>" +
                   "&bull; <strong>iPhone 15 / 16 Series</strong>: Khung viền Titan, camera 48MP, hiệu năng A17/A18 Pro.<br/>" +
                   "&bull; <strong>Samsung Galaxy S24 Ultra</strong>: Quyền năng Galaxy AI, bút S-Pen tiện dụng.<br/>" +
                   "&bull; <strong>Laptop Asus / Acer / Dell / HP / MacBook</strong>: Đầy đủ phiên bản học tập, văn phòng, gaming.<br/>" +
                   "<div class='mt-2'><a href='/Product' class='btn btn-primary btn-sm py-1 px-3 rounded-pill fw-bold text-white text-decoration-none'>Xem sản phẩm công nghệ</a></div>";
        }

        if (q.indexOf("gia dụng") !== -1 || q.indexOf("gia dung") !== -1 || q.indexOf("lọc nước") !== -1 || q.indexOf("loc nuoc") !== -1 || q.indexOf("nồi chiên") !== -1 || q.indexOf("noi chien") !== -1) {
            return "Thiết bị gia dụng hiện đại nâng tầm tiện nghi không gian sống:<br/>" +
                   "&bull; <strong>Máy lọc nước RO Kangaroo / Karofi</strong>: 10 - 11 lõi lọc bổ sung khoáng chất Hydrogen.<br/>" +
                   "&bull; <strong>Nồi chiên không dầu Philips / Tefal</strong>: Rapid Air giảm 90% lượng chất béo.<br/>" +
                   "&bull; <strong>Nồi cơm điện tử cao tần Cuckoo / Toshiba</strong>: Nấu cơm hạt dẻo thơm ngon đều vị.<br/>" +
                   "<div class='mt-2'><a href='/Product' class='btn btn-primary btn-sm py-1 px-3 rounded-pill fw-bold text-white text-decoration-none'>Khám phá Đồ gia dụng</a></div>";
        }

        if (q.indexOf("trả góp") !== -1 || q.indexOf("tra gop") !== -1 || q.indexOf("lãi suất") !== -1 || q.indexOf("lai suat") !== -1) {
            return "Chính sách <strong>Trả góp 0% lãi suất</strong> tại Điện Máy Xanh:<br/>" +
                   "&bull; <strong>Trả góp qua thẻ tín dụng</strong>: Hỗ trợ hơn 25 ngân hàng, thủ tục duyệt ngay trong 30 giây online.<br/>" +
                   "&bull; <strong>Trả góp qua công ty tài chính</strong>: FE Credit, Home Credit, HD SAISON. Chỉ cần CCCD gắn chip (từ 18 tuổi trở lên), không cần chứng minh thu nhập.<br/>" +
                   "&bull; Quý khách có thể chọn phương thức Trả góp trực tiếp tại bước đặt hàng giỏ hàng.";
        }

        if (q.indexOf("bảo hành") !== -1 || q.indexOf("bao hanh") !== -1 || q.indexOf("đổi trả") !== -1 || q.indexOf("doi tra") !== -1) {
            return "Chính sách bảo hành & hậu mãi uy tín hàng đầu:<br/>" +
                   "&bull; <strong>Lỗi là đổi mới</strong> trong vòng 30 ngày tận nhà đối với sản phẩm điện tử, điện lạnh.<br/>" +
                   "&bull; Bảo hành chính hãng từ <strong>12 đến 24 tháng</strong> theo tiêu chuẩn nhà sản xuất.<br/>" +
                   "&bull; Tổng đài bảo hành & kỹ thuật hỗ trợ miễn cước: <strong>1800 1061</strong>.";
        }

        if (q.indexOf("đơn hàng") !== -1 || q.indexOf("don hang") !== -1 || q.indexOf("tra cứu") !== -1 || q.indexOf("tra cuu") !== -1 || q.indexOf("vận chuyển") !== -1 || q.indexOf("giao hàng") !== -1) {
            return "Quý khách có thể dễ dàng theo dõi tiến độ giao hàng:<br/>" +
                   "&bull; Truy cập tính năng <strong>Tra cứu đơn hàng</strong> bằng Mã đơn hàng (VD: DMX-123456) hoặc Số điện thoại đặt hàng.<br/>" +
                   "<div class='mt-2'><a href='/Cart/TrackOrder' class='btn btn-primary btn-sm py-1 px-3 rounded-pill fw-bold text-white text-decoration-none'>Đi đến trang Tra cứu đơn hàng</a></div>";
        }

        if (q.indexOf("mã giảm giá") !== -1 || q.indexOf("voucher") !== -1 || q.indexOf("khuyến mãi") !== -1 || q.indexOf("giam gia") !== -1 || q.indexOf("ưu đãi") !== -1) {
            return "Các mã ưu đãi đặc quyền có thể nhập trực tiếp tại Giỏ hàng:<br/>" +
                   "&bull; <code>DMX100K</code>: Giảm ngay 100.000đ cho đơn hàng từ 1.000.000đ.<br/>" +
                   "&bull; <code>HE2024</code>: Giảm 5% tối đa 300.000đ cho các thiết bị làm mát.<br/>" +
                   "&bull; <code>VIPMEMBER</code>: Giảm 10% tối đa 1.000.000đ cho đơn hàng thành viên từ 10 triệu.<br/>" +
                   "&bull; Miễn phí vận chuyển giao hàng toàn quốc.";
        }

        return "Cảm ơn quý khách đã gửi câu hỏi! Để được tư vấn chi tiết hơn theo nhu cầu riêng, quý khách có thể:<br/>" +
               "&bull; Bấm nút <strong>Yêu cầu gọi lại</strong> trên màn hình, chuyên viên sẽ liên hệ lại sau 5 phút.<br/>" +
               "&bull; Hoặc gọi trực tiếp đến tổng đài miễn cước <strong>1800 1061</strong> (từ 7h30 - 22h00).";
    }

    function handleChatSubmit(query) {
        var trimmed = query.trim();
        if (!trimmed) return;

        appendUserMessage(trimmed);
        if (chatInput) {
            chatInput.value = "";
        }

        setTimeout(function () {
            var botReply = getBotResponse(trimmed);
            appendBotMessage(botReply);
        }, 350);
    }

    if (chatForm && chatInput) {
        chatForm.addEventListener("submit", function (e) {
            e.preventDefault();
            handleChatSubmit(chatInput.value);
        });
    }

    document.querySelectorAll(".dmx-chat-chip").forEach(function (chip) {
        chip.addEventListener("click", function () {
            var query = this.getAttribute("data-query") || this.textContent;
            handleChatSubmit(query);
        });
    });

    // 9. FORM ĐĂNG KÝ YÊU CẦU GỌI LẠI TƯ VẤN (CALLBACK MODAL)
    var callbackForm = document.getElementById("dmxCallbackForm");
    if (callbackForm) {
        callbackForm.addEventListener("submit", function (e) {
            e.preventDefault();
            var nameInput = document.getElementById("callbackName");
            var phoneInput = document.getElementById("callbackPhone");
            var topicInput = document.getElementById("callbackTopic");

            var name = nameInput ? nameInput.value.trim() : "";
            var phone = phoneInput ? phoneInput.value.trim() : "";
            var topic = topicInput ? topicInput.value : "";

            var phoneRegex = /^0[35789][0-9]{8}$/;
            if (!phoneRegex.test(phone)) {
                if (typeof Swal !== "undefined") {
                    Swal.fire({
                        icon: "error",
                        title: "Số điện thoại không hợp lệ",
                        text: "Vui lòng nhập đúng 10 chữ số bắt đầu bằng đầu số 03, 05, 07, 08 hoặc 09.",
                        confirmButtonColor: "#2a83e9",
                        confirmButtonText: "Đã hiểu"
                    });
                } else {
                    alert("Số điện thoại không hợp lệ. Vui lòng nhập đúng 10 số.");
                }
                if (phoneInput) phoneInput.focus();
                return;
            }

            var modalEl = document.getElementById("dmxCallbackModal");
            if (modalEl && typeof bootstrap !== "undefined" && bootstrap.Modal) {
                var modalInstance = bootstrap.Modal.getInstance(modalEl);
                if (modalInstance) {
                    modalInstance.hide();
                }
            }

            callbackForm.reset();

            if (typeof Swal !== "undefined") {
                Swal.fire({
                    icon: "success",
                    title: "Đã tiếp nhận yêu cầu!",
                    html: "Cảm ơn quý khách <strong>" + (name || "quý khách") + "</strong>!<br/>Chuyên viên Điện Máy Xanh sẽ gọi lại số <strong>" + phone + "</strong> để tư vấn về <em>" + topic + "</em> trong vòng 5 phút.",
                    confirmButtonColor: "#2a83e9",
                    confirmButtonText: "Hoàn tất"
                });
            } else {
                alert("Đã tiếp nhận yêu cầu! Chuyên viên sẽ gọi lại số " + phone + " trong vòng 5 phút.");
            }
        });
    }
});
