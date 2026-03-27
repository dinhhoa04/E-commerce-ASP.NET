using Microsoft.AspNetCore.Mvc;

namespace SV22T1020123.Web.Controllers
{
    public class ProductController : Controller
    {
        // GET: /Product
        public IActionResult Index()
        {
            ViewBag.Title = "Danh sách mặt hàng";
            return View();
        }

        // GET: /Product/Detail/5
        public IActionResult Detail(int id)
        {
            ViewBag.Title = "Chi tiết mặt hàng";
            ViewBag.ProductId = id;
            return View();
        }

        // GET: /Product/Create
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung mặt hàng";
            return View("Edit"); // dùng chung view Edit
        }

        // GET: /Product/Edit/5
        public IActionResult Edit(int id)
        {
            ViewBag.Title = "Cập nhật mặt hàng";
            ViewBag.ProductId = id;
            return View();
        }

        // GET: /Product/Delete/5
        public IActionResult Delete(int id)
        {
            ViewBag.Title = "Xóa mặt hàng";
            ViewBag.ProductId = id;
            return View();
        }

        // ================== ATTRIBUTES ==================

        // GET: /Product/ListAttributes/5
        public IActionResult ListAttributes(int id)
        {
            ViewBag.Title = "Danh sách thuộc tính";
            ViewBag.ProductId = id;
            return View();
        }

        // GET: /Product/CreateAttributes/5
        public IActionResult CreateAttributes(int id)
        {
            ViewBag.Title = "Thêm thuộc tính";
            ViewBag.ProductId = id;
            return View("EditAttributes");
        }

        // GET: /Product/EditAttributes/5?attributeId=3
        public IActionResult EditAttributes(int id, int attributeId)
        {
            ViewBag.Title = "Cập nhật thuộc tính";
            ViewBag.ProductId = id;
            ViewBag.AttributeId = attributeId;
            return View();
        }

        // GET: /Product/DeleteAttributes/5?attributeId=3
        public IActionResult DeleteAttributes(int id, int attributeId)
        {
            ViewBag.Title = "Xóa thuộc tính";
            ViewBag.ProductId = id;
            ViewBag.AttributeId = attributeId;
            return View();
        }

        // ================== PHOTOS ==================

        // GET: /Product/ListPhotos/5
        public IActionResult ListPhotos(int id)
        {
            ViewBag.Title = "Danh sách hình ảnh";
            ViewBag.ProductId = id;
            return View();
        }

        // GET: /Product/CreatePhotos/5
        public IActionResult CreatePhotos(int id)
        {
            ViewBag.Title = "Thêm hình ảnh";
            ViewBag.ProductId = id;
            return View("EditPhotos");
        }

        // GET: /Product/EditPhotos/5?photoId=2
        public IActionResult EditPhotos(int id, int photoId)
        {
            ViewBag.Title = "Cập nhật hình ảnh";
            ViewBag.ProductId = id;
            ViewBag.PhotoId = photoId;
            return View();
        }

        // GET: /Product/DeletePhotos/5?photoId=2
        public IActionResult DeletePhotos(int id, int photoId)
        {
            ViewBag.Title = "Xóa hình ảnh";
            ViewBag.ProductId = id;
            ViewBag.PhotoId = photoId;
            return View();
        }
    }
}
