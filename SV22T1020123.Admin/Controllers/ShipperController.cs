using Microsoft.AspNetCore.Mvc;

namespace SV22T1020123.Admin.Controllers
{
    /// <summary>
    /// Cung cấp các chức năng quản lý dữ liệu liên quan đến người vận chuyển
    /// </summary>
    public class ShipperController : Controller
    {
        /// <summary>
        /// Tìm kiếm và hiển thị danh sách người vận chuyển
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            ViewBag.Title = "Danh sách vận chuyển";
            return View();
        }

        /// <summary>
        /// Bổ sung người vận chuyển mới
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung vận chuyển";
            return View("Edit");
        }

        /// <summary>
        /// Cập nhật thông tin người vận chuyển
        /// </summary>
        /// <param name="id">Mã người vận chuyển cần cập nhật thông tin</param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin vận chuyển";
            return View();
        }

        /// <summary>
        /// Xóa một người vận chuyển
        /// </summary>
        /// <param name="id">Mã người vận chuyển cần xóa</param>
        /// <returns></returns>
        public IActionResult Delete(int id)
        {
            ViewBag.Title = "Xóa vận chuyển";
            return View();
        }

    }
}
