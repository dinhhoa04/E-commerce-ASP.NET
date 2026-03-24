using Microsoft.AspNetCore.Mvc;

namespace SV22T1020123.Admin.Controllers
{
    /// <summary>
    /// Cung cấp các chức năng quản lý dữ liệu liên quan đến loại hàng
    /// </summary>
    public class CategoryController : Controller
    {
        /// <summary>
        /// Tìm kiếm và hiển thị danh sách loại hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            ViewBag.Title = "Danh sách loại hàng";
            return View();
        }

        /// <summary>
        /// Bổ sung các loại hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung loại hàng";
            return View("Edit");
        }

        /// <summary>
        /// Cập nhật thông tin loại hàng
        /// </summary>
        /// <param name="id">Mã loại hàng cập nhật</param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            ViewBag.Title = "Cập nhật loại hàng";
            return View();
        }

        /// <summary>
        /// Xóa thông tin loại hàng
        /// </summary>
        /// <param name="id">Mã loại hàng cần xóa</param>
        /// <returns></returns>
        public IActionResult Delete(int id)
        {
            ViewBag.Title = "Xóa loại hàng";
            return View();
        }
    }
}
