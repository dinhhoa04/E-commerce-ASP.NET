$(document).ready(function () {
    loadCartCount();

    $('.btn-add-cart').click(function () {
        var productId = $(this).data('id');
        addToCart(productId, 1);
    });

    $('#btn-add-to-cart-detail').click(function () {
        var productId = $(this).data('id');
        var qty = parseInt($('#quantity').val()) || 1;
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
            $("#cart-badge").text(res.count);
            showToast("Đã thêm vào giỏ hàng!", "success");
        } else {
            showToast(res.message, "danger");
        }
    });
}

function showToast(message, type = "success") {
    const toast = document.getElementById('cartToast');
    if (!toast) return;
    toast.className = `toast align-items-center text-bg-${type} border-0`;
    document.getElementById('toastMessage').textContent = message;
    const bsToast = new bootstrap.Toast(toast, { delay: 2500 });
    bsToast.show();
}