using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SV22T1020123.BusinessLayers;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SV22T1020123.Shop.Controllers
{
    public class AccountController : Controller
    {
        // =========================================================================
        // 1. ĐĂNG NHẬP / ĐĂNG XUẤT
        // =========================================================================

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // 1. BĂM MẬT KHẨU BẰNG MD5 TRƯỚC KHI SO SÁNH
            string hashedPassword = CryptHelper.HashMD5(password);

            var customer = await PartnerDataService.AuthorizeCustomerAsync(email, hashedPassword);

            if (customer != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, customer.CustomerName),
                    new Claim(ClaimTypes.Email, customer.Email ?? ""),
                    new Claim("CustomerID", customer.CustomerID.ToString())
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("Error", "Email hoặc mật khẩu không chính xác hoặc tài khoản đang bị khóa!");
                return View();
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // =========================================================================
        // 2. ĐĂNG KÝ TÀI KHOẢN MỚI
        // =========================================================================

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            await LoadProvincesToViewBag();
            return View(new SV22T1020123.Models.Partner.Customer());
        }

        [HttpPost]
        public async Task<IActionResult> Register(SV22T1020123.Models.Partner.Customer data, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ModelState.AddModelError("Error", "Mật khẩu xác nhận không khớp!");
                await LoadProvincesToViewBag();
                return View(data);
            }

            bool isValidEmail = await PartnerDataService.ValidatelCustomerEmailAsync(data.Email ?? "", 0);
            if (!isValidEmail)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng bởi một tài khoản khác!");
                await LoadProvincesToViewBag();
                return View(data);
            }

            data.IsLocked = false;
            if (string.IsNullOrEmpty(data.ContactName)) data.ContactName = data.CustomerName;
            data.Province = data.Province ?? "";
            data.Address = data.Address ?? "";
            data.Phone = data.Phone ?? "";

            try
            {
                int customerId = await PartnerDataService.AddCustomerAsync(data);

                if (customerId > 0)
                {
                    // 2. BĂM MẬT KHẨU MỚI BẰNG MD5 TRƯỚC KHI LƯU VÀO DB
                    string hashedPassword = CryptHelper.HashMD5(password);
                    await PartnerDataService.ChangeCustomerPasswordAsync(customerId, hashedPassword);

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

        private async Task LoadProvincesToViewBag()
        {
            var provinces = await DictionaryDataService.ListProvincesAsync();
            ViewBag.Provinces = provinces.Select(p => new SelectListItem
            {
                Value = p.ProvinceName,
                Text = p.ProvinceName
            }).ToList();
        }

        // =========================================================================
        // 3. QUẢN LÝ THÔNG TIN CÁ NHÂN VÀ ĐỔI MẬT KHẨU
        // =========================================================================

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var claimId = User.Claims.FirstOrDefault(c => c.Type == "CustomerID")?.Value;
            if (!int.TryParse(claimId, out int customerId)) return RedirectToAction("Login");

            var customer = await PartnerDataService.GetCustomerAsync(customerId);
            if (customer == null) return RedirectToAction("Login");

            await LoadProvincesToViewBag();
            return View(customer);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Profile(SV22T1020123.Models.Partner.Customer data)
        {
            var claimId = User.Claims.FirstOrDefault(c => c.Type == "CustomerID")?.Value;
            if (!int.TryParse(claimId, out int customerId)) return RedirectToAction("Login");

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

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

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

            // 3. BĂM MẬT KHẨU CŨ BẰNG MD5 ĐỂ KIỂM TRA
            string hashedOldPassword = CryptHelper.HashMD5(oldPassword);
            var customer = await PartnerDataService.AuthorizeCustomerAsync(email, hashedOldPassword);

            if (customer == null)
            {
                ModelState.AddModelError("Error", "Mật khẩu cũ không chính xác!");
                return View();
            }

            // 4. BĂM MẬT KHẨU MỚI BẰNG MD5 TRƯỚC KHI LƯU (Chỉ để 1 lần duy nhất)
            string hashedNewPassword = CryptHelper.HashMD5(newPassword);
            await PartnerDataService.ChangeCustomerPasswordAsync(customerId, hashedNewPassword);

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";

            // Trả về đúng trang ChangePassword
            return RedirectToAction("ChangePassword");
        }
    }
}