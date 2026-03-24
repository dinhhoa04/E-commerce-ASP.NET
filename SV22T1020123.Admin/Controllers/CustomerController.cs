using Microsoft.AspNetCore.Mvc;
using SV22T1020123.BusinessLayers;
using SV22T1020123.Models.Common;

namespace SV22T1020123.Admin.Controllers
{
    public class CustomerController : Controller
    {
        private const int PAGE_SIZE = 10;
        private const string CUSTOMER_SEARCH = "CustomerSearchInput";

        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(CUSTOMER_SEARCH);
            if (input == null)
                input = new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = ""
                };
            return View(input);
        }

        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            var result = await PartnerDataService.ListCustomerAsync(input);
            ApplicationContext.SetSessionData(CUSTOMER_SEARCH, input);
            return PartialView("_CustomerTable", result);  // ← trả về Partial View
        }

        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung khách hàng";
            return View("Edit");
        }

        public IActionResult Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin khách hàng";
            return View();
        }

        public IActionResult Delete(int id)
        {
            ViewBag.Title = "Xóa khách hàng";
            return View();
        }

        public IActionResult ChangePassword(int id)
        {
            ViewBag.Title = "Thay đổi mật khẩu";
            return View();
        }
    }
}