using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SV22T1020123.BusinessLayers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace SV22T1020123.Shop.Controllers
{
    public class AccountController : Controller
    {
        /// <summary>
        /// Giao diện đăng nhập
        /// </summary>
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        /// <summary>
        /// Xử lý đăng nhập bằng Database thật
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Kiểm tra thông tin từ Database
            var customer = await PartnerDataService.AuthorizeCustomerAsync(email, password);

            if (customer != null)
            {
                // Tạo danh sách quyền (Claims) để lưu vào Cookie
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, customer.CustomerName),
                    new Claim(ClaimTypes.Email, customer.Email ?? ""),
                    new Claim("CustomerID", customer.CustomerID.ToString())
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // Ghi Cookie đăng nhập
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("Error", "Email hoặc mật khẩu không chính xác hoặc tài khoản đang bị khóa!");
                return View();
            }
        }

        /// <summary>
        /// Đăng xuất
        /// </summary>
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Giao diện đăng ký
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            await LoadProvincesToViewBag();
            return View(new SV22T1020123.Models.Partner.Customer());
        }

        /// <summary>
        /// Xử lý đăng ký tài khoản mới và lưu mật khẩu vào Database
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Register(SV22T1020123.Models.Partner.Customer data, string password, string confirmPassword)
        {
            // 1. Kiểm tra mật khẩu khớp
            if (password != confirmPassword)
            {
                ModelState.AddModelError("Error", "Mật khẩu xác nhận không khớp!");
                await LoadProvincesToViewBag();
                return View(data);
            }

            // 2. Kiểm tra email trùng
            bool isValidEmail = await PartnerDataService.ValidatelCustomerEmailAsync(data.Email ?? "", 0);
            if (!isValidEmail)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng bởi một tài khoản khác!");
                await LoadProvincesToViewBag();
                return View(data);
            }

            // 3. Chuẩn hóa dữ liệu
            data.IsLocked = false;
            if (string.IsNullOrEmpty(data.ContactName)) data.ContactName = data.CustomerName;
            data.Province = data.Province ?? "";
            data.Address = data.Address ?? "";
            data.Phone = data.Phone ?? "";

            // 4. Lưu vào Database
            try
            {
                // Lưu thông tin cơ bản của khách hàng
                int customerId = await PartnerDataService.AddCustomerAsync(data);

                if (customerId > 0)
                {
                    // Lưu mật khẩu thật vào Database
                    await PartnerDataService.ChangeCustomerPasswordAsync(customerId, password);
                    return RedirectToAction("Login");
                }
            }
            catch
            {
                ModelState.AddModelError("Error", "Hệ thống đang bận hoặc dữ liệu không hợp lệ. Vui lòng thử lại sau.");
            }

            await LoadProvincesToViewBag();
            return View(data);
        }

        /// <summary>
        /// Hàm bổ trợ load danh sách tỉnh thành
        /// </summary>
        private async Task LoadProvincesToViewBag()
        {
            var provinces = await DictionaryDataService.ListProvincesAsync();
            ViewBag.Provinces = provinces.Select(p => new SelectListItem
            {
                Value = p.ProvinceName,
                Text = p.ProvinceName
            }).ToList();
        }

        // ===== CÁC HÀM MỚI THÊM: QUẢN LÝ THÔNG TIN CÁ NHÂN VÀ ĐỔI MẬT KHẨU =====

        /// <summary>
        /// Giao diện Thông tin cá nhân
        /// </summary>
        [HttpGet]
        [Authorize] // Yêu cầu phải đăng nhập mới được vào
        public async Task<IActionResult> Profile()
        {
            var claimId = User.Claims.FirstOrDefault(c => c.Type == "CustomerID")?.Value;
            if (!int.TryParse(claimId, out int customerId)) return RedirectToAction("Login");

            // Lấy thông tin khách hàng từ DB
            var customer = await PartnerDataService.GetCustomerAsync(customerId);
            if (customer == null) return RedirectToAction("Login");

            await LoadProvincesToViewBag();
            return View(customer);
        }

        /// <summary>
        /// Xử lý cập nhật thông tin cá nhân
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Profile(SV22T1020123.Models.Partner.Customer data)
        {
            var claimId = User.Claims.FirstOrDefault(c => c.Type == "CustomerID")?.Value;
            if (!int.TryParse(claimId, out int customerId)) return RedirectToAction("Login");

            // Đảm bảo không bị thay đổi ID và trạng thái khóa
            data.CustomerID = customerId;
            data.IsLocked = false;
            if (string.IsNullOrEmpty(data.ContactName)) data.ContactName = data.CustomerName;
            data.Province = data.Province ?? "";
            data.Address = data.Address ?? "";
            data.Phone = data.Phone ?? "";

            try
            {
                await PartnerDataService.UpdateCustomerAsync(data);
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
            }
            catch
            {
                ModelState.AddModelError("Error", "Có lỗi xảy ra, không thể cập nhật!");
            }

            await LoadProvincesToViewBag();
            return View(data);
        }

        /// <summary>
        /// Giao diện đổi mật khẩu
        /// </summary>
        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        /// <summary>
        /// Xử lý đổi mật khẩu
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("Error", "Mật khẩu xác nhận không khớp!");
                return View();
            }

            var claimId = User.Claims.FirstOrDefault(c => c.Type == "CustomerID")?.Value;
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (!int.TryParse(claimId, out int customerId) || string.IsNullOrEmpty(email))
                return RedirectToAction("Login");

            // Kiểm tra mật khẩu cũ có đúng không
            var customer = await PartnerDataService.AuthorizeCustomerAsync(email, oldPassword);
            if (customer == null)
            {
                ModelState.AddModelError("Error", "Mật khẩu cũ không chính xác!");
                return View();
            }

            // Cập nhật mật khẩu mới
            await PartnerDataService.ChangeCustomerPasswordAsync(customerId, newPassword);

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Profile");
        }
        // =====================================================================


    }
}