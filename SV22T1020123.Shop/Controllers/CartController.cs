using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SV22T1020123.BusinessLayers;
using SV22T1020123.Models.Sales;

namespace SV22T1020123.Shop.Controllers
{
    public class CartController : Controller
    {
        private const string SHOPPING_CART = "ShoppingCart";

        // Hàm đọc giỏ hàng từ Session
        private List<OrderDetailViewInfo> GetCart()
        {
            var session = HttpContext.Session;
            string? json = session.GetString(SHOPPING_CART);
            if (!string.IsNullOrEmpty(json))
                return JsonConvert.DeserializeObject<List<OrderDetailViewInfo>>(json) ?? new List<OrderDetailViewInfo>();
            return new List<OrderDetailViewInfo>();
        }

        // Hàm lưu giỏ hàng vào Session
        private void SaveCart(List<OrderDetailViewInfo> cart)
        {
            var session = HttpContext.Session;
            session.SetString(SHOPPING_CART, JsonConvert.SerializeObject(cart));
        }

        // Hiển thị trang giỏ hàng
        public IActionResult Index()
        {
            return View(GetCart());
        }

        // API Thêm sản phẩm vào giỏ (Gọi qua AJAX)
        [HttpPost]
        public async Task<IActionResult> AddToCart(int id, int quantity = 1)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductID == id);

            if (item == null)
            {
                var product = await CatalogDataService.GetProductAsync(id);
                if (product == null) return Json(new { success = false, message = "Sản phẩm không tồn tại" });

                cart.Add(new OrderDetailViewInfo
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    Photo = product.Photo ?? "nophoto.png",
                    SalePrice = product.Price, // Giá bán lấy từ CSDL
                    Quantity = quantity,
                    Unit = product.Unit
                });
            }
            else
            {
                item.Quantity += quantity; // Nếu có rồi thì tăng số lượng
            }

            SaveCart(cart);
            // Trả về tổng số lượng để update con số trên header
            return Json(new { success = true, count = cart.Sum(c => c.Quantity) });
        }

        // Xóa 1 mặt hàng
        public IActionResult RemoveFromCart(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductID == id);
            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        // Cập nhật số lượng
        [HttpPost]
        public IActionResult UpdateCart(int id, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductID == id);
            if (item != null && quantity > 0)
            {
                item.Quantity = quantity;
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        // API Lấy số lượng giỏ hàng hiện tại (để header hiển thị)
        [HttpGet]
        public IActionResult GetCartCount()
        {
            var cart = GetCart();
            return Json(cart.Sum(c => c.Quantity));
        }

        // =========================================================================
        // XÓA TOÀN BỘ GIỎ HÀNG
        // =========================================================================
       
        public IActionResult ClearCart()
        {
            // Thay vì gọi Service của Admin, ta tạo ra một giỏ hàng trống...
            var emptyCart = new List<OrderDetailViewInfo>();

            // ...và lưu đè nó vào Session (Cách này an toàn và đồng bộ nhất)
            SaveCart(emptyCart);

            // Hoặc bạn cũng có thể xóa hẳn key trong Session bằng câu lệnh:
            // HttpContext.Session.Remove(SHOPPING_CART);

            // Xóa xong thì tải lại trang Giỏ hàng để cập nhật giao diện
            return RedirectToAction("Index");
        }
    }
}