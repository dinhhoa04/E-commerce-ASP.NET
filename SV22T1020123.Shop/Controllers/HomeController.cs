using Microsoft.AspNetCore.Mvc;
using SV22T1020123.BusinessLayers;
using SV22T1020123.Models.Catalog;
using SV22T1020123.Models.Common;
using SV22T1020123.Shop.Models;
using System.Diagnostics;

namespace SV22T1020123.Shop.Controllers
{
    public class HomeController : Controller
    {
        private const int PAGE_SIZE = 12; // Hiển thị 12 sản phẩm mỗi trang (chia lưới 3x4 hoặc 4x3 đẹp nhất)

        // CHỨC NĂNG 4: Xem, tìm kiếm danh mục mặt hàng
        public async Task<IActionResult> Index(int categoryId = 0, string searchValue = "", decimal minPrice = 0, decimal maxPrice = 0, int page = 1)
        {
            var input = new ProductSearchInput()
            {
                Page = page,
                PageSize = PAGE_SIZE,
                SearchValue = searchValue ?? "",
                CategoryID = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            // Lấy danh sách sản phẩm
            var model = await CatalogDataService.ListProductsAsync(input);

            // Lấy danh sách loại hàng để làm bộ lọc (truyền qua ViewBag)
            var categorySearchInput = new PaginationSearchInput { Page = 1, PageSize = 0, SearchValue = "" };
            ViewBag.Categories = await CatalogDataService.ListCategoriesAsync(categorySearchInput);

            // Giữ lại input để hiển thị trên giao diện
            ViewBag.SearchInput = input;

            return View(model);
        }

        // CHỨC NĂNG 5: Xem thông tin chi tiết của mặt hàng
        public async Task<IActionResult> Detail(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null || !product.IsSelling)
                return RedirectToAction("Index"); // Nếu không tìm thấy hoặc ngừng bán thì quay về trang chủ

            // Lấy thêm danh sách ảnh và thuộc tính của sản phẩm
            ViewBag.Photos = await CatalogDataService.ListPhotosAsync(id);
            ViewBag.Attributes = await CatalogDataService.ListAttributesAsync(id);

            return View(product);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}