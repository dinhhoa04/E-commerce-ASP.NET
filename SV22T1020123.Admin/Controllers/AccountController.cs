using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
// Thêm thư viện gọi tầng Database của bạn vào đây:
// using SV22T1020123.BusinessLayers;

namespace SV22T1020123.Admin.Controllers
{
    [Authorize] // Bảo vệ toàn bộ AccountController, trừ những hàm có [AllowAnonymous]
    public class AccountController : Controller
    {
        [AllowAnonymous] // Cho phép khách chưa đăng nhập truy cập trang này
        [HttpGet]
        public IActionResult Login()
        {
            // Nếu đã đăng nhập rồi thì tự động đá về trang chủ Admin
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            ViewBag.Username = username;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Error", "Vui lòng nhập đầy đủ email và mật khẩu!");
                return View();
            }

            // =========================================================================
            // BƯỚC NÀY GỌI XUỐNG DATABASE ĐỂ KIỂM TRA MẬT KHẨU CỦA NHÂN VIÊN.
            // Ví dụ chuẩn trong hệ thống LiteCommerce:
            // var userAccount = await UserAccountService.AuthorizeAsync(UserAccountService.UserTypes.Employee, username, password);
            //
            // (Hiện tại mình để Mock Data tạm, bạn thay dòng if dưới đây bằng việc 
            // kiểm tra object userAccount trả về từ DB nhé!)
            // =========================================================================

            bool isLoginSuccess = (username == "admin@gmail.com" && password == "123");
            int employeeID = 1; // Lấy ID của nhân viên từ CSDL
            string fullName = "Admin Cửa Hàng";

            if (!isLoginSuccess)
            {
                ModelState.AddModelError("Error", "Đăng nhập thất bại. Vui lòng kiểm tra lại!");
                return View();
            }

            // Nếu đúng mật khẩu, tạo "Thẻ bài" (Cookie) cho Admin
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, fullName),
                new Claim(ClaimTypes.Email, username),
                new Claim("EmployeeID", employeeID.ToString()) // Rất quan trọng để các tính năng duyệt đơn hàng lấy mã NV
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear(); // Xóa sạch giỏ hàng tạm lúc lập đơn
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        // ===== CHỨC NĂNG ĐỔI MẬT KHẨU CHO ADMIN =====

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ModelState.AddModelError("Error", "Vui lòng nhập đầy đủ thông tin!");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("Error", "Xác nhận mật khẩu mới không khớp!");
                return View();
            }

            // Lấy email (username) của Admin đang đăng nhập từ Cookie
            var username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            // =====================================================================
            // TODO: GỌI XUỐNG CSDL ĐỂ KIỂM TRA MẬT KHẨU CŨ & LƯU MẬT KHẨU MỚI TẠI ĐÂY
            // Ví dụ (Nếu bạn có UserAccountService):
            // var user = await UserAccountService.AuthorizeAsync(UserAccountService.UserTypes.Employee, username, oldPassword);
            // if (user == null) {
            //     ModelState.AddModelError("Error", "Mật khẩu cũ không chính xác!");
            //     return View();
            // }
            // await UserAccountService.ChangePasswordAsync(UserAccountService.UserTypes.Employee, username, newPassword);
            // =====================================================================

            // Tạm thời báo thành công (Khi nào có code nối CSDL thì bỏ dòng này đi)
            TempData["SuccessMessage"] = "Đổi mật khẩu thành công! Bạn có thể sử dụng mật khẩu mới cho lần đăng nhập sau.";

            return RedirectToAction("ChangePassword");
        }
    }
}