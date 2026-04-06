using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SV22T1020123.BusinessLayers;
using SV22T1020123.Models.Sales;
using Microsoft.AspNetCore.Authorization;

namespace SV22T1020123.Shop.Controllers
{
    public class OrderController : Controller
    {
        private const string SHOPPING_CART = "ShoppingCart";

        private List<OrderDetailViewInfo> GetCart()
        {
            var session = HttpContext.Session;
            string? json = session.GetString(SHOPPING_CART);
            if (!string.IsNullOrEmpty(json))
                return JsonConvert.DeserializeObject<List<OrderDetailViewInfo>>(json) ?? new List<OrderDetailViewInfo>();
            return new List<OrderDetailViewInfo>();
        }

        private void ClearCart()
        {
            var session = HttpContext.Session;
            session.Remove(SHOPPING_CART);
        }

        [HttpGet]
        [Authorize] // <--- THÊM DÒNG NÀY ĐỂ BẮT BUỘC ĐĂNG NHẬP
        public async Task<IActionResult> Checkout()
        {
            var cart = GetCart();
            if (cart.Count == 0)
                return RedirectToAction("Index", "Cart");

            var provinces = await DictionaryDataService.ListProvincesAsync();
            ViewBag.Provinces = provinces.Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = p.ProvinceName,
                Text = p.ProvinceName
            }).ToList();

            return View(cart);
        }

        [HttpPost]
        [Authorize] // <--- THÊM DÒNG NÀY ĐỂ BẮT BUỘC ĐĂNG NHẬP
        public async Task<IActionResult> InitOrder(string deliveryProvince, string deliveryAddress)
        {
            var cart = GetCart();
            if (cart.Count == 0) return RedirectToAction("Index", "Cart");

            // Vì đã có [Authorize] nên chắc chắn 100% đã đăng nhập và có CustomerID
            int customerId = 0;
            var claimId = User.Claims.FirstOrDefault(c => c.Type == "CustomerID")?.Value;
            if (int.TryParse(claimId, out int id)) customerId = id;

            int systemEmployeeId = 1;

            var order = new Order()
            {
                CustomerID = customerId, // Lúc này luôn có mã khách hàng
                DeliveryProvince = deliveryProvince ?? "",
                DeliveryAddress = deliveryAddress ?? ""
            };

            int orderID = await SalesDataService.AddOrderAsync(systemEmployeeId, order);

            if (orderID > 0)
            {
                foreach (var item in cart)
                {
                    await SalesDataService.SaveOrderDetailAsync(orderID, item.ProductID, item.Quantity, item.SalePrice);
                }

                ClearCart();
                return RedirectToAction("Success", new { id = orderID });
            }

            return RedirectToAction("Index", "Cart");
        }

        public IActionResult Success(int id)
        {
            ViewBag.OrderID = id;
            return View();
        }

        // ===== CÁC HÀM MỚI THÊM: LỊCH SỬ ĐƠN HÀNG =====

        [HttpGet]
        [Authorize] // Phải đăng nhập mới xem được lịch sử
        public async Task<IActionResult> History()
        {
            var claimId = User.Claims.FirstOrDefault(c => c.Type == "CustomerID")?.Value;
            if (!int.TryParse(claimId, out int customerId)) return RedirectToAction("Login", "Account");

            // Lấy danh sách đơn hàng của riêng khách hàng này
            var orders = await SalesDataService.ListOrdersByCustomerIdAsync(customerId);
            return View(orders);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var claimId = User.Claims.FirstOrDefault(c => c.Type == "CustomerID")?.Value;
            if (!int.TryParse(claimId, out int customerId)) return RedirectToAction("Login", "Account");

            // Lấy thông tin đơn hàng
            var order = await SalesDataService.GetOrderAsync(id);

            // Bảo mật: Nếu đơn hàng không tồn tại hoặc không phải của khách hàng này thì chặn lại
            if (order == null || order.CustomerID != customerId)
                return RedirectToAction("History");

            // Lấy chi tiết các mặt hàng trong đơn
            ViewBag.Details = await SalesDataService.ListDetailsAsync(id);

            return View(order);
        }
        // ==============================================

    }
}