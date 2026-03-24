using Microsoft.AspNetCore.Mvc;

namespace SV22T1020123.Admin.Controllers
{
    /// <summary>
    /// cung cấp các chức năng quản lý dữ liệu liên quan đến nhà cung cấp
    /// </summary>
    public class SupplierController : Controller
    {
        /// <summary>
        /// Tìm kiếm và hiển thị danh sách nhà cung cấp
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            ViewBag.Title = "Danh sách nhà cung cấp";
            return View();
        }

        /// <summary>
        /// Bổ sung nhà cung cấp mới
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung nhà cung cấp";
            return View("Edit");
        }

        /// <summary>
        /// Cập nhật thông tin nhà cung cấp
        /// </summary>
        /// <param name="id">Mã nhà cung cấp cần cập nhật thông tin</param>
        /// <returns></returns>
        public IActionResult Edit(int id) 
        { 
            ViewBag.Title = "Cập nhật thông tin nhà cung cấp";
            return View();
        }

        /// <summary>
        /// Xóa một nhà cung cấp
        /// </summary>
        /// <param name="id">Mã nhà cung cấp cần xóa</param>
        /// <returns></returns>
        public IActionResult Delete(int id)
        {
            return View();
        }

    }
}
