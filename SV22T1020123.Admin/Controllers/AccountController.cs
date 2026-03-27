using Microsoft.AspNetCore.Mvc;

namespace SV22T1020123.Admin.Controllers
{
    /// <summary>
    /// Các chức năng của tài khoản
    /// </summary>
   
    public class AccountController : Controller
    {
        //Đăng nhập
        public IActionResult Login()
        {
            return View();
        }
        //Đăng xuất 
        public IActionResult Logout()
        {
            return RedirectToAction("Login");
        }
        //Thay đổi password
        public IActionResult ChangePassword()
        {
            return View();
        }
    }
}
