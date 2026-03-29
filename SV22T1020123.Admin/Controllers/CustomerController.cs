using Microsoft.AspNetCore.Mvc;
using SV22T1020123.Admin;
using SV22T1020123.Models.Common;
using SV22T1020123.Models.Partner;
using SV22T1020123.Admin;
using SV22T1020123.BusinessLayers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq.Expressions;


namespace SV22T1020123.Web.Controllers
{
    /// <summary>
    /// Các chức năng của quản lý khách hàng
    /// </summary>
    public class CustomerController : Controller
    {

        // private const int PAGESIZE = 10;//Hard Code
        /// <summary>
        /// Teen của biến dùng để lưu điều kiện tìm kiếm khachs hàng trong session
        /// </summary>
        private const string CUSTOMER_SEARCH = "CustomerSearchInput";

        /// <summary>
        /// Nhập đầu vào tìm kiếm ->  hiển thị kết quả tìm kiếm
        /// </summary>
        /// <returns></returns>
        /// 
        /*  public async Task<IActionResult> Index(int page = 1, string searchValue = "")
          {
              var input = new PaginationSearchInput()
              {
                  Page = page,
                  PageSize = PAGESIZE,
                  SearchValue = searchValue
              };
              var result = await PartnerDataService.ListCustomersAsync(input);
              ViewBag.SearchValue = searchValue;
              return View(result);
          }*/
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>("CUSTOMER_SEARCH");
            if (input == null)
                input = new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = ApplicationContext.PageSize,
                    SearchValue = ""
                };
            return View(input);
        }
        /// <summary>
        /// Tìm kiếm và trả về kết quả
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            var result = await PartnerDataService.ListCustomersAsync(input);
            ApplicationContext.SetSessionData("CUSTOMER_SEARCH", input);
            return View(result);
        }

        // GET: /Customer/Create
        //bổ sung khách hàng(tạo mới khách hàng) 
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung khách hàng";
            var model = new Customer()
            {
                CustomerID = 0
            };
            return View("Edit", model); // dùng chung view Edit
        }

        // GET: /Customer/Edit/5
        //Cập nhật khách hàng
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật khách hàng";
            var model = await PartnerDataService.GetCustomerAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            ViewBag.CustomerId = id;
            return View(model);
        }
        [HttpPost]

        public async Task<IActionResult> SaveData(Customer data)
        {
            try
            {
                ViewBag.Title = data.CustomerID == 0 ? "Bổ sung khách hàng" : "Cập nhật thông tin khách hàng";
                //TODO:Kiểm tra dl có hợp lệ không (phải làm)
                //Sử dụng ModelState để lưu các tình huống lỗi và thông báo lỗi cho người dùng(trên View)
                //Giả định: chỉ yêu cầu nhập tên,email,tỉnh/thành
                if (string.IsNullOrWhiteSpace(data.CustomerName))
                    ModelState.AddModelError("CustomerName", "Chi mi,sao không nhập tên hợp lệ");
                if (string.IsNullOrWhiteSpace(data.Email))
                    ModelState.AddModelError(nameof(data.Email), "Email không được để trống");
                else if (!await PartnerDataService.ValidatelCustomerEmailAsync(data.Email, data.CustomerID))
                    ModelState.AddModelError(nameof(data.Email), "Email này bị trùng");
                if (string.IsNullOrEmpty(data.Province))
                    ModelState.AddModelError(nameof(data.Province), "Vui lòng chọn tỉnh thành");
                if (!ModelState.IsValid)
                    return View("Edit", data);
                //(Tùy chọn) Hiệu chỉnh dữ liệu theo qui định của hệ thống
                if (string.IsNullOrWhiteSpace(data.ContactName)) data.ContactName = data.CustomerName;
                if (string.IsNullOrEmpty(data.Phone)) data.Phone = "";
                if (string.IsNullOrEmpty(data.Address)) data.Address = "";

                //Lưu dữ liệu vào CSDL

                if (data.CustomerID == 0)
                {
                    await PartnerDataService.AddCustomerAsync(data);
                }
                else
                {
                    await PartnerDataService.UpdateCustomerAsync(data);
                }
                return RedirectToAction("Index");

            }

            catch (Exception ex)
            {
                //ghi log lỗi dựa vào thông tin trong Exception (ex.Message, ex.StackTrace)
                ModelState.AddModelError("Error", "Dữ liệu không hợp lệ hoặc có lỗi xảy ra");
                return View("Edit", data);
            }
        }


        // GET: /Customer/Delete/5
        //Xóa khách hàng 
        public async Task<IActionResult> Delete(int id)
        {
            //Nếu method là POST thì xóa
            if(Request.Method=="POST")
            {
                await PartnerDataService.DeleteCustomerAsync(id);
                return RedirectToAction("Index");
            }    
            var model = await PartnerDataService.GetCustomerAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            ViewBag.CanDelete =!await PartnerDataService.IsUsedCustomerAsync(id);
            return View(model);
        }

        // GET: /Customer/ChangePassword/5
        //Thay đổi mật khẩu 
        public IActionResult ChangePassword(int id)
        {
            ViewBag.Title = "Đổi mật khẩu khách hàng";
            ViewBag.CustomerId = id;
            return View();
        }
    }
}