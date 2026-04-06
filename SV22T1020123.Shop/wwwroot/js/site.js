$(document).ready(function () {
    // 1. Tải số lượng giỏ hàng hiện tại khi load trang
    loadCartCount();

    // 2. Xử lý khi bấm nút "Thêm vào giỏ" ở trang danh sách
    $('.btn-add-cart').click(function () {
        var productId = $(this).data('id');
        addToCart(productId, 1);
    });

    // 3. Xử lý khi bấm nút "Thêm vào giỏ" ở trang chi tiết
    $('#btn-add-to-cart-detail').click(function () {
        var productId = $(this).data('id');
        var qty = $('#quantity').val();
        addToCart(productId, qty);
    });
});

function loadCartCount() {
    $.get("/Cart/GetCartCount", function (data) {
        $("#cart-badge").text(data);
    });
}

function addToCart(productId, quantity) {
    $.post("/Cart/AddToCart", { id: productId, quantity: quantity }, function (res) {
        if (res.success) {
            // Cập nhật con số trên giỏ hàng bằng hiệu ứng chớp tắt nhỏ
            $("#cart-badge").text(res.count).fadeOut(100).fadeIn(100);

            // Bạn có thể dùng thư viện Toast/SweetAlert để hiện thông báo đẹp hơn, ở đây dùng alert tạm
            alert("Đã thêm sản phẩm vào giỏ hàng!");
        } else {
            alert(res.message);
        }
    });
}