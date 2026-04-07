
// Hiển thị ảnh được chọn từ input file lên thẻ img
// (Thẻ input có thuộc tính data-img-preview trỏ đến id của thẻ img dung để hiển thị ảnh)
function previewImage(input) {
    if (!input.files || !input.files[0]) return;

    const previewId = input.dataset.imgPreview; // lấy data-img-preview
    if (!previewId) return;

    const img = document.getElementById(previewId);
    if (!img) return;

    const reader = new FileReader();
    reader.onload = function (e) {
        img.src = e.target.result;
    };
    reader.readAsDataURL(input.files[0]);
}

// Tìm kiếm phân trang bằng AJAX
function paginationSearch(event, form, page) {
    if (event) event.preventDefault();
    if (!form) return;

    const url = form.action;
    const method = (form.method || "GET").toUpperCase();
    const targetId = form.dataset.target;

    const formData = new FormData(form);
    formData.append("page", page);

    let fetchUrl = url;
    if (method === "GET") {
        const params = new URLSearchParams(formData).toString();
        fetchUrl = url + "?" + params;
    }

    let targetEl = null;
    if (targetId) {
        targetEl = document.getElementById(targetId);
        if (targetEl) {
            targetEl.innerHTML = `
                <div class="text-center py-4">
                    <span>Đang tải dữ liệu...</span>
                </div>`;
        }
    }

    fetch(fetchUrl, {
        method: method,
        body: method === "GET" ? null : formData
    })
    .then(res => res.text())
    .then(html => {
        if (targetEl) {
            targetEl.innerHTML = html;
        }
    })
    .catch(() => {
        if (targetEl) {
            targetEl.innerHTML = `
                <div class="text-danger">
                    Không tải được dữ liệu
                </div>`;
        }
    });
}

// Mở modal và load nội dung từ link vào modal
(function () {
    //dialogModal là id của modal dùng chung đuơc định nghĩa trong _Layout.cshtml
    const modalEl = document.getElementById("dialogModal");
    if (!modalEl) return;

    const modalContent = modalEl.querySelector(".modal-content");

    // Clear nội dung khi modal đóng
    modalEl.addEventListener('hidden.bs.modal', function () {
        modalContent.innerHTML = '';
    });

    window.openModal = function (event, link) {
        if (!link) return;
        if (event) event.preventDefault();

        const url = link.getAttribute("href");

        // Hiển thị loading
        modalContent.innerHTML = `
            <div class="modal-body text-center py-5">
                <span>Đang tải dữ liệu...</span>
            </div>`;

        // Khởi tạo modal (chỉ tạo 1 lần)
        let modal = bootstrap.Modal.getInstance(modalEl);
        if (!modal) {
            modal = new bootstrap.Modal(modalEl, {
                backdrop: 'static',
                keyboard: false
            });
        }

        modal.show();

        // Load nội dung
        fetch(url)
            .then(res => res.text())
            .then(html => {
                modalContent.innerHTML = html;

                // THÊM ĐOẠN NÀY: Khởi tạo lại AutoNumeric cho các thẻ input trong Modal
                modalContent.querySelectorAll('.money-input').forEach(element => {
                    new AutoNumeric(element, {
                        digitGroupSeparator: '.',
                        decimalCharacter: ',',
                        decimalPlaces: 0,
                        allowDecimalPadding: false,
                        minimumValue: '0',
                        modifyValueOnWheel: false,
                        unformatOnSubmit: true,
                        outputFormat: "string",
                        emptyInputBehavior: "null"
                    });
                });
            })
            .catch(() => {
                modalContent.innerHTML = `
                    <div class="modal-body text-danger">
                        Không tải được dữ liệu
                    </div>`;
            });
    };
})();


// Xử lý các thao tác đơn hàng qua modal (Accept, Reject, Cancel, Finish, Delete)
function processOrder(url) {
    fetch(url, { method: "POST" })
        .then(r => {
            // Nếu Server trả về lỗi 500 thay vì JSON, bắt lỗi tại đây
            if (!r.ok) throw new Error("Lỗi kết nối hoặc lỗi máy chủ (500)");
            return r.json();
        })
        .then(result => {
            if (result.code <= 0) {
                // Hiển thị thông báo lỗi rõ ràng từ try-catch ở Controller
                alert(result.message);
            } else {
                // Đóng Modal và làm mới trang nếu thành công
                let modalInstance = bootstrap.Modal.getInstance(document.getElementById("dialogModal"));
                if (modalInstance) modalInstance.hide();

                if (typeof reloadPage === "function") {
                    reloadPage();
                } else {
                    window.location.reload();
                }
            }
        })
        .catch(err => {
            console.error(err);
            alert("Đã xảy ra lỗi hệ thống: " + err.message);
        });
}