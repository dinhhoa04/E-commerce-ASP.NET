using Microsoft.AspNetCore.Mvc;
using SV22T1020123.Admin;
using SV22T1020123.BusinessLayers;
using SV22T1020123.Models.Catalog;

namespace SV22T1020123.Web.Controllers
{
    public class ProductController : Controller
    {
        private const string PRODUCT_SEARCH = "ProductSearchInput";

        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH);
            if (input == null)
                input = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = ApplicationContext.PageSize,
                    SearchValue = "",
                    CategoryID = 0,
                    SupplierID = 0,
                    MinPrice = 0,
                    MaxPrice = 0
                };
            return View(input);
        }

        public async Task<IActionResult> Search(ProductSearchInput input)
        {
            var result = await CatalogDataService.ListProductsAsync(input);
            ApplicationContext.SetSessionData(PRODUCT_SEARCH, input);
            return View(result);
        }

        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung mặt hàng";
            var model = new Product()
            {
                ProductID = 0,
                IsSelling = true
            };
            return View("Edit", model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật mặt hàng";
            var model = await CatalogDataService.GetProductAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveData(Product data, IFormFile? uploadPhoto)
        {
            try
            {
                ViewBag.Title = data.ProductID == 0 ? "Bổ sung mặt hàng" : "Cập nhật mặt hàng";

                if (string.IsNullOrWhiteSpace(data.ProductName))
                    ModelState.AddModelError(nameof(data.ProductName), "Vui lòng nhập tên mặt hàng");
                if (string.IsNullOrWhiteSpace(data.Unit))
                    ModelState.AddModelError(nameof(data.Unit), "Vui lòng nhập đơn vị tính");
                if (data.Price <= 0)
                    ModelState.AddModelError(nameof(data.Price), "Vui lòng nhập giá hợp lệ");
                if (data.CategoryID == null || data.CategoryID == 0)
                    ModelState.AddModelError(nameof(data.CategoryID), "Vui lòng chọn loại hàng");
                if (data.SupplierID == null || data.SupplierID == 0)
                    ModelState.AddModelError(nameof(data.SupplierID), "Vui lòng chọn nhà cung cấp");

                if (!ModelState.IsValid)
                    return View("Edit", data);

                if (uploadPhoto != null)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(uploadPhoto.FileName)}";
                    var filePath = Path.Combine(ApplicationContext.WWWRootPath, "images/products", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadPhoto.CopyToAsync(stream);
                    }
                    data.Photo = fileName;
                }

                if (string.IsNullOrEmpty(data.Photo)) data.Photo = "nophoto.png";
                if (string.IsNullOrEmpty(data.ProductDescription)) data.ProductDescription = "";

                if (data.ProductID == 0)
                    await CatalogDataService.AddProductAsync(data);
                else
                    await CatalogDataService.UpdateProductAsync(data);

                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang bận hoặc dữ liệu không hợp lệ");
                return View("Edit", data);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (Request.Method == "POST")
            {
                await CatalogDataService.DeleteProductAsync(id);
                return RedirectToAction("Index");
            }
            var model = await CatalogDataService.GetProductAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            ViewBag.CanDelete = !await CatalogDataService.IsUsedProductAsync(id);
            return View(model);
        }

        // ========== ATTRIBUTES ==========

        public async Task<IActionResult> EditAttributes(int id, long attributeID = 0)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null)
                return RedirectToAction("Index");

            ViewBag.Title = attributeID == 0 ? "Bổ sung thuộc tính" : "Cập nhật thuộc tính";

            ProductAttribute model;
            if (attributeID == 0)
            {
                model = new ProductAttribute()
                {
                    AttributeID = 0,
                    ProductID = id,
                    DisplayOrder = 1
                };
            }
            else
            {
                var attr = await CatalogDataService.GetAttributeAsync(attributeID);
                if (attr == null)
                    return RedirectToAction("Edit", new { id });
                model = attr;
            }

            ViewBag.Product = product;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveAttribute(ProductAttribute data)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(data.AttributeName))
                    ModelState.AddModelError(nameof(data.AttributeName), "Vui lòng nhập tên thuộc tính");
                if (string.IsNullOrWhiteSpace(data.AttributeValue))
                    ModelState.AddModelError(nameof(data.AttributeValue), "Vui lòng nhập giá trị thuộc tính");

                if (!ModelState.IsValid)
                {
                    ViewBag.Product = await CatalogDataService.GetProductAsync(data.ProductID);
                    return View("EditAttributes", data);
                }

                if (data.AttributeID == 0)
                    await CatalogDataService.AddAttributeAsync(data);
                else
                    await CatalogDataService.UpdateAttributeAsync(data);

                return RedirectToAction("Edit", new { id = data.ProductID });
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang bận hoặc dữ liệu không hợp lệ");
                ViewBag.Product = await CatalogDataService.GetProductAsync(data.ProductID);
                return View("EditAttributes", data);
            }
        }

        public async Task<IActionResult> DeleteAttributes(int id, long attributeID)
        {
            if (Request.Method == "POST")
            {
                await CatalogDataService.DeleteAttributeAsync(attributeID);
                return RedirectToAction("Edit", new { id });
            }
            var model = await CatalogDataService.GetAttributeAsync(attributeID);
            if (model == null)
                return RedirectToAction("Edit", new { id });
            ViewBag.ProductID = id;
            return View(model);
        }

        // ========== PHOTOS ==========

        public async Task<IActionResult> EditPhotos(int id, long photoID = 0)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null)
                return RedirectToAction("Index");

            ViewBag.Title = photoID == 0 ? "Bổ sung ảnh" : "Cập nhật ảnh";

            ProductPhoto model;
            if (photoID == 0)
            {
                model = new ProductPhoto()
                {
                    PhotoID = 0,
                    ProductID = id,
                    DisplayOrder = 1
                };
            }
            else
            {
                var photo = await CatalogDataService.GetPhotoAsync(photoID);
                if (photo == null)
                    return RedirectToAction("Edit", new { id });
                model = photo;
            }

            ViewBag.Product = product;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SavePhoto(ProductPhoto data, IFormFile? uploadPhoto)
        {
            try
            {
                if (uploadPhoto != null)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(uploadPhoto.FileName)}";
                    var filePath = Path.Combine(ApplicationContext.WWWRootPath, "images/products", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadPhoto.CopyToAsync(stream);
                    }
                    data.Photo = fileName;
                }

                if (string.IsNullOrEmpty(data.Photo))
                    ModelState.AddModelError(nameof(data.Photo), "Vui lòng chọn ảnh");

                if (!ModelState.IsValid)
                {
                    ViewBag.Product = await CatalogDataService.GetProductAsync(data.ProductID);
                    return View("EditPhotos", data);
                }

                if (string.IsNullOrEmpty(data.Description)) data.Description = "";

                if (data.PhotoID == 0)
                    await CatalogDataService.AddPhotoAsync(data);
                else
                    await CatalogDataService.UpdatePhotoAsync(data);

                return RedirectToAction("Edit", new { id = data.ProductID });
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang bận hoặc dữ liệu không hợp lệ");
                ViewBag.Product = await CatalogDataService.GetProductAsync(data.ProductID);
                return View("EditPhotos", data);
            }
        }

        public async Task<IActionResult> DeletePhotos(int id, long photoID)
        {
            if (Request.Method == "POST")
            {
                await CatalogDataService.DeletePhotoAsync(photoID);
                return RedirectToAction("Edit", new { id });
            }
            var model = await CatalogDataService.GetPhotoAsync(photoID);
            if (model == null)
                return RedirectToAction("Edit", new { id });
            ViewBag.ProductID = id;
            return View(model);
        }
    }
}