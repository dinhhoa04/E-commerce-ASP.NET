using Microsoft.AspNetCore.Mvc;

namespace SV22T1020123.Admin.Controllers
{
    /// <summary>
    /// Cung cấp các chức năng quản lý dữ liệu liên quan đến Nhân viên
    /// </summary>
    public class EmployeeController : Controller
    {
        /// <summary>
        /// Tìm kiếm và hiển thị danh sách nhân viên
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            ViewBag.Title = "Danh sách nhân viên";
            return View();
        }

        /// <summary>
        /// Bổ sung nhân viên
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Thêm nhân viên";
            return View("Edit");
        }

        /// <summary>
        /// Cập nhật thông tin nhân viên
        /// </summary>
        /// <param name="id">Mã nhân viên cần cập nhật thông tin</param>
        /// <returns></returns>
        public IActionResult Edit(int id) 
        {
            ViewBag.Title = "Cập nhật nhân viên";
            return View();
        }

        /// <summary>
        /// Xóa nhân viên
        /// </summary>
        /// <param name="id">Mã nhân viên cần xóa</param>
        /// <returns></returns>
        public IActionResult Delete(int id)
        {
            ViewBag.Title = "Xóa nhân viên";
            return View();
        }

        /// <summary>
        /// Thay dổi mật khẩu cho account nhân viên
        /// </summary>
        /// <param name="id">Mã nhân viên có account cần thay dổi mật khẩu</param>
        /// <returns></returns>
        public IActionResult ChangePassword(int id)
        {
            ViewBag.Title = "Đổi mật khẩu nhân viên";
            return View();
        }

        /// <summary>
        /// Thay đổi vai trò nhân viên
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult ChangeRole(int id)
        {
            ViewBag.Title = "Thay đổi vai trò của nhân viên";
            return View();
        }
    }
}
