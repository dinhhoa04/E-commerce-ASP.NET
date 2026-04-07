using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020123.BusinessLayers;
using SV22T1020123.Models.Security;
using System.Threading.Tasks;
using System.Linq;

namespace SV22T1020123.Admin.Controllers
{
    /// <summary>
    /// Cung cấp các chức năng quản lý liên quan đến tài khoản người dùng
    /// </summary>
    [Authorize]
    public class AccountController : Controller
    {
        /// <summary>
        /// Giao diện Đăng nhập
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            // Nếu đã đăng nhập rồi thì tự động đá về trang chủ Admin
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        /// <summary>
        /// Xử lý dữ liệu Đăng nhập
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            ViewBag.UserName = username;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("Error", "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.");
                return View();
            }

            // 1. Mã hoá MD5 mật khẩu 
            string hashedPassword = CryptHelper.HashMD5(password);

            // 2. Kiểm tra username và hashedPassword với cơ sở dữ liệu
            var userAccount = await SecurityDataService.AuthorizeAsync(username, hashedPassword);
            if (userAccount == null)
            {
                ModelState.AddModelError("Error", "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View();
            }

            // 3. Chặn khách hàng đăng nhập vào trang Admin (ĐÃ ĐƯA LÊN TRƯỚC KHI RETURN)
            if (userAccount.RoleNames != null && userAccount.RoleNames.Contains("Customer"))
            {
                ModelState.AddModelError("Error", "Tài khoản không có quyền truy cập hệ thống quản trị.");
                return View();
            }

            // Xử lý đăng nhập thành công
            // 4. Chuẩn bị thông tin sẽ ghi trong principal (ClaimPrincipal)
            var userData = new WebUserData()
            {
                UserId = userAccount.UserId,
                UserName = userAccount.UserName,
                DisplayName = userAccount.DisplayName,
                Email = userAccount.Email,
                Photo = userAccount.Photo ?? "nophoto.png",
                Roles = userAccount.RoleNames.Split(',').ToList()
            };

            // 5. Tạo chứng nhận (ClaimsPrincipal) cho người dùng
            var principal = userData.CreatePrincipal();

            // 6. Cấp chứng nhận và lưu Cookie (Bổ sung scheme để hệ thống hiểu)
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // 7. Vào trang chủ Admin
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Đăng xuất tài khoản người dùng
        /// </summary>
        public async Task<IActionResult> Logout()
        {
            try
            {
                HttpContext.Session.Clear();
                await HttpContext.SignOutAsync();
                return RedirectToAction("Login");
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Login");
            }

        }

        /// <summary>
        /// Hiển thị giao diện Đổi mật khẩu (GET)
        /// </summary>
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        /// <summary>
        /// Nhận dữ liệu từ form và lưu xuống Database (POST)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            try
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

                // 1. Lấy thông tin tài khoản đang đăng nhập
                var userData = User.GetUserData();
                if (userData == null || string.IsNullOrEmpty(userData.Email))
                    return RedirectToAction("Login");

                string username = userData.Email;

                // 2. BĂM MẬT KHẨU CŨ SANG MD5 VÀ KIỂM TRA
                string hashedOldPassword = CryptHelper.HashMD5(oldPassword);
                var userAccount = await SecurityDataService.AuthorizeAsync(username, hashedOldPassword);

                if (userAccount == null)
                {
                    ModelState.AddModelError("Error", "Mật khẩu cũ không chính xác!");
                    return View();
                }

                // 3. BĂM MẬT KHẨU MỚI SANG MD5 VÀ LƯU XUỐNG DB
                string hashedNewPassword = CryptHelper.HashMD5(newPassword);
                bool isSuccess = await SecurityDataService.ChangePasswordAsync(username, hashedNewPassword);

                if (isSuccess)
                {
                    TempData["SuccessMessage"] = "Đổi mật khẩu thành công! Hãy dùng mật khẩu mới cho lần đăng nhập sau.";
                    return RedirectToAction("ChangePassword");
                }

                ModelState.AddModelError("Error", "Không thể cập nhật mật khẩu vào cơ sở dữ liệu!");
                return View();
            }
            catch
            {
                ModelState.AddModelError("Error", "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau.");
                return View();
            }
        }
    }
}