using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SV22T1020123.BusinessLayers;
using SV22T1020123.Models.Common;
using SV22T1020123.Models.HR;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SV22T1020123.Admin.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private const string EMPLOYEE_SEARCH = "EmployeeSearchInput";

        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(EMPLOYEE_SEARCH);
            if (input == null)
                input = new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = ApplicationContext.PageSize,
                    SearchValue = ""
                };
            return View(input);
        }

        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            var result = await HRDataService.ListEmployeesAsync(input);
            ApplicationContext.SetSessionData(EMPLOYEE_SEARCH, input);
            return View(result);
        }

        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung nhân viên";
            var model = new Employee()
            {
                EmployeeID = 0,
                IsWorking = true
            };
            return View("Edit", model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin nhân viên";
            var model = await HRDataService.GetEmployeeAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (Request.Method == "POST")
            {
                await HRDataService.DeleteEmployeeAsync(id);
                return RedirectToAction("Index");
            }
            var model = await HRDataService.GetEmployeeAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            ViewBag.CanDelete = !await HRDataService.IsUsedEmployeeAsync(id);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveData(Employee data, IFormFile? uploadPhoto)
        {
            try
            {
                ViewBag.Title = data.EmployeeID == 0 ? "Bổ sung nhân viên" : "Cập nhật thông tin nhân viên";

                if (string.IsNullOrWhiteSpace(data.FullName))
                    ModelState.AddModelError(nameof(data.FullName), "Vui lòng nhập họ tên nhân viên");

                if (string.IsNullOrWhiteSpace(data.Email))
                    ModelState.AddModelError(nameof(data.Email), "Vui lòng nhập email nhân viên");
                else if (!await HRDataService.ValidateEmployeeEmailAsync(data.Email, data.EmployeeID))
                    ModelState.AddModelError(nameof(data.Email), "Email đã được sử dụng bởi nhân viên khác");

                if (!ModelState.IsValid)
                    return View("Edit", data);

                if (uploadPhoto != null)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(uploadPhoto.FileName)}";
                    var filePath = Path.Combine(ApplicationContext.WWWRootPath, "images/employees", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadPhoto.CopyToAsync(stream);
                    }
                    data.Photo = fileName;
                }

                if (string.IsNullOrEmpty(data.Address)) data.Address = "";
                if (string.IsNullOrEmpty(data.Phone)) data.Phone = "";
                if (string.IsNullOrEmpty(data.Photo)) data.Photo = "nophoto.png";

                if (data.EmployeeID == 0)
                    await HRDataService.AddEmployeeAsync(data);
                else
                    await HRDataService.UpdateEmployeeAsync(data);

                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang bận hoặc dữ liệu không hợp lệ");
                return View("Edit", data);
            }
        }

        // =========================================================================
        // KHU VỰC CẤP LẠI MẬT KHẨU (NÚT CHÌA KHÓA VÀNG)
        // =========================================================================

        public async Task<IActionResult> ChangePassword(int id)
        {
            ViewBag.Title = "Đổi mật khẩu nhân viên";
            var model = await HRDataService.GetEmployeeAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SavePassword(int employeeID, string newPassword, string confirmPassword)
        {
            // Lấy thông tin nhân viên lên trước để truyền lại View nếu bị lỗi
            var model = await HRDataService.GetEmployeeAsync(employeeID);
            if (model == null) return RedirectToAction("Index");

            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ModelState.AddModelError("Error", "Vui lòng nhập đầy đủ mật khẩu!");
                return View("ChangePassword", model);
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("Error", "Mật khẩu xác nhận không khớp!");
                return View("ChangePassword", model);
            }

            try
            {
                // Băm mật khẩu ra mã MD5
                string hashedNewPassword = SV22T1020123.Admin.CryptHelper.HashMD5(newPassword);

                // Gọi Service lưu vào DB
                bool isSuccess = await SecurityDataService.ChangePasswordAsync(model.Email, hashedNewPassword);

                if (isSuccess)
                {
                    // Nếu lưu thành công, đá về trang danh sách nhân viên
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("Error", "Không thể cập nhật mật khẩu, vui lòng thử lại!");
                return View("ChangePassword", model);
            }
            catch
            {
                ModelState.AddModelError("Error", "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau!");
                return View("ChangePassword", model);
            }
        }

        // =========================================================================
        // KHU VỰC PHÂN QUYỀN VAI TRÒ
        // =========================================================================

        public async Task<IActionResult> ChangeRole(int id)
        {
            ViewBag.Title = "Phân quyền nhân viên";
            var model = await HRDataService.GetEmployeeAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveRole(int employeeID, string[] roles)
        {
            try
            {
                // 1. Ghép các quyền người dùng tích chọn thành một chuỗi cách nhau bởi dấu phẩy
                string roleNames = roles != null ? string.Join(",", roles) : "";

                // 2. Lấy thông tin nhân viên lên
                var employee = await HRDataService.GetEmployeeAsync(employeeID);
                if (employee == null) return RedirectToAction("Index");

                // 3. Gán quyền mới cho nhân viên
                employee.RoleNames = roleNames;

                // 4. Lưu xuống Database
                // Dùng hàm cập nhật nhân viên của thầy để lưu lại chuỗi quyền này
                await HRDataService.UpdateEmployeeAsync(employee);

                TempData["SuccessMessage"] = "Cập nhật phân quyền thành công!";
                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("Error", "Hệ thống xảy ra lỗi khi phân quyền.");
                return RedirectToAction("ChangeRole", new { id = employeeID });
            }
        }
    }
}