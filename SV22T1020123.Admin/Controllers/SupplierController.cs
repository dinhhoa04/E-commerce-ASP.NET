using Microsoft.AspNetCore.Mvc;
using SV22T1020123.Models.Common;
using SV22T1020123.BusinessLayers;
using System.Threading.Tasks;

namespace SV22T1020123.Admin.Controllers
{
    /// <summary>
    /// Cung cấp các chức năng quản lý dữ liệu liên quan đến nhà cung cấp
    /// </summary>
    public class SupplierController : Controller
    {
        /// <summary>
        /// Tên biến dùng để lưu điều kiện tìm kiếm khách hàng trong session
        /// </summary>
        private const string SUPPLIER_SEARCH = "SupplierSearchInput";

        /// <summary>
        /// Nhập đầu vào tìm kiếm -> Hiển thị danh sách khách hàng
        /// </summary>
        /// <returns></returns>k
        /// <returns></returns>k
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(SUPPLIER_SEARCH);
            if (input == null)
            {
                input = new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = ApplicationContext.PageSize,
                    SearchValue = ""
                };
            }

            return View(input);
        }
        /// <summary>
        /// Tìm kiếm và trả về kết quả
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            var result = await PartnerDataService.ListSuppliersAsync(input);
            ApplicationContext.SetSessionData(SUPPLIER_SEARCH, input);
            return View(result);
        }



        /// <summary>
        /// Tạo mới 1 nhà cung cấp
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung nhà cung cấp";
            return View("Edit");
        }
        /// <summary>
        /// Chỉnh sửa 1 nhà cung cấp
        /// </summary>
        /// <param name="id">Mã nhà cung cấp cần chỉnh sửa</param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin nhà cung cấp";
            return View();
        }
        /// <summary>
        /// Xóa 1 nhà cung cấp
        /// </summary>
        /// <param name="id">Mã nhà cung cấp cần xóa</param>
        /// <returns></returns>
        public IActionResult Delete(int id)
        {
            return View();
        }
    }
}
