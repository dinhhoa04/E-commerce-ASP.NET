using Microsoft.AspNetCore.Mvc;

namespace SV22T1020123.Admin.Controllers
{
    /// <summary>
    /// Cung cấp các chức năng quản lý dữ liệu liên quan đến mặt hàng
    /// </summary>
    public class ProductController : Controller
    {
        /// <summary>
        /// Tìm kiếm và hiển thị danh sách sản phẩm
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            ViewBag.Title = "Danh sách sản phẩm";
            return View();
        }

        /// <summary>
        /// Tạo mới sản phẩm
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Thêm sản phẩm";
            return View("Edit");
        }

        /// <summary>
        /// Chỉnh sửa thông tin sản phẩm
        /// </summary>
        /// <param name="id">Mã sản phẩm cần thay dổi thông tin</param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin sản phẩm";
            return View();
        }

        /// <summary>
        /// Xóa một sản phẩm
        /// </summary>
        /// <param name="id">Mã sản phẩm cần xóa</param>
        /// <returns></returns>
        public IActionResult Delete(int id)
        {
            ViewBag.Title = "Xóa sản phẩm";
            return View();
            // Sau này xử lý xong nên RedirectToAction("Index");
        }

        // ================== ATTRIBUTES ==================

        /// <summary>
        /// Hiển thị danh sách thuộc tính của mặt hàng
        /// </summary>
        /// <param name="id">Mã mặt hàng cần hiển thị thuộc tính</param>
        /// <returns></returns>
        public IActionResult ListAttributes(int id)
        {
            ViewBag.Title = "Danh sách thuộc tính sản phẩm";
            return View();
        }

        /// <summary>
        /// Bổ sung thuộc tính mới cho sản phẩm
        /// </summary>
        /// <param name="id">Mã mặt hàng cần bổ sung thuộc tính</param>
        /// <returns></returns>
        public IActionResult CreateAttribute(int id)
        {
            ViewBag.Title = "Thêm thuộc tính sản phẩm";
            return View("EditAttribute");
        }

        /// <summary>
        /// Cập nhật thuộc tính của sản phẩm
        /// </summary>
        /// <param name="id">mã sản phẩm có thuộc tính cần thay đổi</param>
        /// <param name="attributeId">Mã thuộc tính cần cập nhật</param>
        /// <returns></returns>
        public IActionResult EditAttribute(int id, int attributeId)
        {
            ViewBag.Title = "Chỉnh sửa thuộc tính sản phẩm";
            return View();
        }

        /// <summary>
        /// Xóa thuộc tính sản phẩm
        /// </summary>
        /// <param name="id">Mã sản phẩm có thuộc tính cần xóa</param>
        /// <param name="attributeId">Mã sản phẩm cần xóa</param>
        /// <returns></returns>
        public IActionResult DeleteAttribute(int id, int attributeId)
        {
            ViewBag.Title = "Xóa thuộc tính sản phẩm";
            return View();
            //return RedirectToAction("ListAttributes", new { id });
        }

        // ================== PHOTOS ==================

        /// <summary>
        /// Hiên thị danh sách ảnh của từng sản phẩm
        /// </summary>
        /// <param name="id">Mã sản phẩm cần hiển thị ảnh</param>
        /// <returns></returns>
        public IActionResult ListPhotos(int id)
        {
            ViewBag.Title = "Danh sách hình ảnh sản phẩm";
            return View();
        }

        /// <summary>
        /// Bổ sung ảnh mới cho sản phẩm
        /// </summary>
        /// <param name="id">Mã sản phẩm cần bổ sung ảnh</param>
        /// <returns></returns>
        public IActionResult CreatePhoto(int id)
        {
            ViewBag.Title = "Thêm hình ảnh sản phẩm";
            return View("EditPhoto");
        }

        /// <summary>
        /// Cập nhật ảnh của sản phẩm
        /// </summary>
        /// <param name="id">Mã sản phẩm cần cập nhật ảnh</param>
        /// <param name="photoId">Mã ảnh cần cập nhật</param>
        /// <returns></returns>
        public IActionResult EditPhoto(int id, int photoId)
        {
            ViewBag.Title = "Cập nhật hình ảnh sản phẩm";
            return View();
        }

        /// <summary>
        /// Xóa ảnh của sản phẩm
        /// </summary>
        /// <param name="id">Mã sản phẩm có ảnh cần xóa</param>
        /// <param name="photoId">Mã ảnh cần xóa</param>
        /// <returns></returns>
        public IActionResult DeletePhoto(int id, int photoId)
        {
            ViewBag.Title = "Xóa hình ảnh sản phẩm";
            return View();
            //RedirectToAction("ListPhotos", new { id });
        }
    }
}