using Microsoft.AspNetCore.Mvc;

namespace SV22T1020123.Web.Controllers
{
    public class OrderController : Controller
    {
      /// <summary>
      /// Màn hình hiển thị danh sách đơn hàng
      /// </summary>
      /// <returns></returns>
        public IActionResult Index()
        {
            ViewBag.Title = "Danh sách đơn hàng";
            return View(); // Views/Order/Index.cshtml
        }

     /// <summary>
     /// Tìm kiếm đơn hàng
     /// </summary>
     /// <param name="keyword"></param>
     /// <returns></returns>
        public IActionResult Search(string? keyword)
        {
            ViewBag.Title = "Tìm kiếm đơn hàng";
            ViewBag.Keyword = keyword;
            return View(); // Views/Order/Search.cshtml
        }
        /// <summary>
        /// Tạo đơn hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Tạo đơn hàng";
            return View(); // Views/Order/Create.cshtml
        }

        // GET: /Order/Detail/{id}
        public IActionResult Detail(int id)
        {
            ViewBag.Title = "Chi tiết đơn hàng";
            ViewBag.OrderId = id;
            return View(); // Views/Order/Detail.cshtml
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public IActionResult EditCartItem(int id, int productId)
        {
            ViewBag.Title = "Cập nhật sản phẩm trong giỏ";
            ViewBag.OrderId = id;
            ViewBag.ProductId = productId;
            return View(); // Views/Order/EditCartItem.cshtml
        }

        /// <summary>
        /// Hiển thị trang xác nhận xóa một sản phẩm khỏi giỏ hàng của đơn.
        /// Route: /Order/DeleteCartItem/{id}?productId={productId}
        /// View: Views/Order/DeleteCartItem.cshtml
        /// </summary>
        /// <param name="id">Mã đơn hàng (OrderId).</param>
        /// <param name="productId">Mã sản phẩm cần xóa khỏi giỏ hàng.</param>
        public IActionResult DeleteCartItem(int id, int productId)
        {
            ViewBag.Title = "Xóa sản phẩm khỏi giỏ";
            ViewBag.OrderId = id;
            ViewBag.ProductId = productId;
            return View(); // Views/Order/DeleteCartItem.cshtml
        }

        /// <summary>
        /// Xóa toàn bộ giỏ hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult ClearCart()
        {
            ViewBag.Title = "Xóa toàn bộ giỏ hàng";
            return View(); // Views/Order/ClearCart.cshtml
        }

        /// <summary>
        /// Duyệt đơn hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Accept(int id)
        {
            ViewBag.Title = "Duyệt đơn hàng";
            ViewBag.OrderId = id;
            return View(); // Views/Order/Accept.cshtml
        }

       /// <summary>
       /// Giao hàng
       /// </summary>
       /// <param name="Mã shipper"></param>
       /// <returns></returns>
        public IActionResult Shipping(int id)
        {
            ViewBag.Title = "Giao hàng";
            ViewBag.OrderId = id;
            return View(); // Views/Order/Shipping.cshtml
        }
        /// <summary>
        /// Hoàn tất đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng cần được hoàn tất</param>
        /// <returns></returns>
        public IActionResult Finish(int id)
        {
            ViewBag.Title = "Hoàn tất đơn hàng";
            ViewBag.OrderId = id;
            return View(); // Views/Order/Finish.cshtml
        }

       /// <summary>
       /// Từ chối đơn hàng
       /// </summary>
       /// <param name="id">Mã đơn hàng cần từ chối</param>
       /// <returns></returns>
        public IActionResult Reject(int id)
        {
            ViewBag.Title = "Từ chối đơn hàng";
            ViewBag.OrderId = id;
            return View(); // Views/Order/Reject.cshtml
        }

        // GET: /Order/Cancel/{id}
        public IActionResult Cancel(int id)
        {
            ViewBag.Title = "Hủy đơn hàng";
            ViewBag.OrderId = id;
            return View(); // Views/Order/Cancel.cshtml
        }

        // GET: /Order/Delete/{id}
        public IActionResult Delete(int id)
        {
            ViewBag.Title = "Xóa đơn hàng";
            ViewBag.OrderId = id;
            return View(); // Views/Order/Delete.cshtml
        }

        // ========= NOTE =========
        // View DeleteItem.cshtml có sẵn trong thư mục nhưng route bạn đưa KHÔNG có.
        // Nếu sau này bạn cần dùng DeleteItem, mở lại 2 dòng dưới đây:

        // GET: /Order/DeleteItem/{id}?productId={productId}
        // public IActionResult DeleteItem(int id, int productId)
        // {
        //     ViewBag.Title = "Xóa mặt hàng trong đơn";
        //     ViewBag.OrderId = id;
        //     ViewBag.ProductId = productId;
        //     return View(); // Views/Order/DeleteItem.cshtml
        // }
    }
}