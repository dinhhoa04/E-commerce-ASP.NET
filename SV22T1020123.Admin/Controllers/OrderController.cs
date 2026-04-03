using Microsoft.AspNetCore.Mvc;
using SV22T1020123.Admin;
using SV22T1020123.BusinessLayers;
using SV22T1020123.Models.Catalog;
using SV22T1020123.Models.Common;
using SV22T1020123.Models.Sales;

namespace SV22T1020123.Web.Controllers
{
    public class OrderController : Controller
    {
        private const string PRODUCT_SEARCH = "SearchSellProduct";
        private const string ORDER_SEARCH = "OrderSearchInput";

        // ========== DANH SÁCH ĐƠN HÀNG ==========

        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<OrderSearchInput>(ORDER_SEARCH);
            if (input == null)
                input = new OrderSearchInput()
                {
                    Page = 1,
                    PageSize = ApplicationContext.PageSize,
                    SearchValue = "",
                    Status = 0,
                    DateFrom = null,
                    DateTo = null
                };
            return View(input);
        }

        public async Task<IActionResult> Search(OrderSearchInput input)
        {
            var result = await SalesDataService.ListOrdersAsync(input);
            ApplicationContext.SetSessionData(ORDER_SEARCH, input);
            return View(result);
        }

        // ========== LẬP ĐƠN HÀNG ==========

        public IActionResult Create()
        {
            ViewBag.Title = "Lập đơn hàng";
            var input = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH);
            if (input == null)
                input = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = 5,
                    SearchValue = ""
                };
            return View(input);
        }

        public async Task<IActionResult> SearchProduct(ProductSearchInput input)
        {
            var result = await CatalogDataService.ListProductsAsync(input);
            ApplicationContext.SetSessionData(PRODUCT_SEARCH, input);
            return View(result);
        }

        public IActionResult ShowCart()
        {
            var cart = ShoppingCartService.GetShoppingCart();
            return PartialView(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddCartItem(int productID, int quantity, decimal price)
        {
            if (quantity <= 0)
                return Json(new ApiResult(0, "Số lượng không hợp lệ"));
            if (price < 0)
                return Json(new ApiResult(0, "Giá không hợp lệ"));

            var product = await CatalogDataService.GetProductAsync(productID);
            if (product == null)
                return Json(new ApiResult(0, "Mặt hàng không tồn tại"));
            if (!product.IsSelling)
                return Json(new ApiResult(0, "Mặt hàng đã ngừng bán"));

            ShoppingCartService.AddCartItem(new OrderDetailViewInfo()
            {
                ProductID = productID,
                Quantity = quantity,
                SalePrice = price,
                ProductName = product.ProductName,
                Unit = product.Unit,
                Photo = product.Photo ?? "nophoto.png"
            });

            return Json(new ApiResult(1));
        }

        public IActionResult EditCartItem(int productId = 0)
        {
            var item = ShoppingCartService.GetCartItem(productId);
            return PartialView(item);
        }

        [HttpPost]
        public IActionResult UpdateCartItem(int productId, int quantity, decimal salePrice)
        {
            if (quantity <= 0)
                return Json(new ApiResult(0, "Số lượng không hợp lệ"));
            if (salePrice < 0)
                return Json(new ApiResult(0, "Giá bán không hợp lệ"));

            ShoppingCartService.UpdateCartItem(productId, quantity, salePrice);
            return Json(new ApiResult(1));
        }

        public IActionResult DeleteCartItem(int productId = 0)
        {
            if (Request.Method == "POST")
            {
                ShoppingCartService.RemoveCartItem(productId);
                return Json(new ApiResult(1));
            }
            var item = ShoppingCartService.GetCartItem(productId);
            return PartialView(item);
        }

        public IActionResult ClearCart()
        {
            if (Request.Method == "POST")
            {
                ShoppingCartService.ClearCart();
                return Json(new ApiResult(1));
            }
            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(int customerID = 0,
    string province = "", string address = "")
        {
            var cart = ShoppingCartService.GetShoppingCart();
            if (cart.Count == 0)
                return Json(new ApiResult(0, "Giỏ hàng trống, không thể lập đơn hàng"));

            var order = new Order()
            {
                CustomerID = customerID == 0 ? null : customerID,
                DeliveryProvince = province,
                DeliveryAddress = address,
            };

            int orderID = await SalesDataService.AddOrderAsync(customerID, order);

            foreach (var item in cart)
            {
                await SalesDataService.AddDetailAsync(new OrderDetail()
                {
                    OrderID = orderID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    SalePrice = item.SalePrice,
                });
            }

            ShoppingCartService.ClearCart();
            return Json(new ApiResult(orderID));
        }
        // ========== CHI TIẾT ĐƠN HÀNG ==========

        public async Task<IActionResult> Detail(int id)
        {
            var model = await SalesDataService.GetOrderAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            ViewBag.Details = await SalesDataService.ListDetailsAsync(id);
            return View(model);
        }

        // ========== XỬ LÝ TRẠNG THÁI ĐƠN HÀNG ==========

        public async Task<IActionResult> Accept(int id)
        {
            if (Request.Method == "POST")
            {
                // TODO: lấy employeeID từ session đăng nhập
                int employeeID = 1;
                await SalesDataService.AcceptOrderAsync(id, employeeID);
                return Json(new ApiResult(1));
            }
            var model = await SalesDataService.GetOrderAsync(id);
            if (model == null)
                return Json(new ApiResult(0, "Không tìm thấy đơn hàng"));
            return PartialView(model);
        }

        public async Task<IActionResult> Reject(int id)
        {
            if (Request.Method == "POST")
            {
                int employeeID = 1;
                await SalesDataService.RejectOrderAsync(id, employeeID);
                return Json(new ApiResult(1));
            }
            var model = await SalesDataService.GetOrderAsync(id);
            if (model == null)
                return Json(new ApiResult(0, "Không tìm thấy đơn hàng"));
            return PartialView(model);
        }

        public async Task<IActionResult> Cancel(int id)
        {
            if (Request.Method == "POST")
            {
                await SalesDataService.CancelOrderAsync(id);
                return Json(new ApiResult(1));
            }
            var model = await SalesDataService.GetOrderAsync(id);
            if (model == null)
                return Json(new ApiResult(0, "Không tìm thấy đơn hàng"));
            return PartialView(model);
        }

        public async Task<IActionResult> Shipping(int id)
        {
            if (Request.Method == "POST")
            {
                int shipperID = int.Parse(Request.Form["shipperID"].ToString() ?? "0");
                if (shipperID <= 0)
                    return Json(new ApiResult(0, "Vui lòng chọn người giao hàng"));
                await SalesDataService.ShipOrderAsync(id, shipperID);
                return Json(new ApiResult(1));
            }
            var model = await SalesDataService.GetOrderAsync(id);
            if (model == null)
                return Json(new ApiResult(0, "Không tìm thấy đơn hàng"));
            return PartialView(model);
        }

        public async Task<IActionResult> Finish(int id)
        {
            if (Request.Method == "POST")
            {
                await SalesDataService.CompleteOrderAsync(id);
                return Json(new ApiResult(1));
            }
            var model = await SalesDataService.GetOrderAsync(id);
            if (model == null)
                return Json(new ApiResult(0, "Không tìm thấy đơn hàng"));
            return PartialView(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (Request.Method == "POST")
            {
                await SalesDataService.DeleteOrderAsync(id);
                return Json(new ApiResult(1));
            }
            var model = await SalesDataService.GetOrderAsync(id);
            if (model == null)
                return Json(new ApiResult(0, "Không tìm thấy đơn hàng"));
            return PartialView(model);
        }
    }
}