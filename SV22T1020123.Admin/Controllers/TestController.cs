using SV22T1020123.DataLayers.SQLServer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SV22T1020123.Models.Catalog;
using SV22T1020123.Models.Common;

namespace SV22T1020123.Admin.Controllers
{
    public class TestController : Controller
    {
        private readonly IConfiguration _configuration;

        public TestController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> TestProduct()
        {
            var repo = new SupplierRepository(
                _configuration.GetConnectionString("LiteCommerceDB")
            );

            var input = new ProductSearchInput()
            {
                Page = 1,
                PageSize = 10
            };

            var result = await repo.ListAsync(input);

            return Json(result);
        }
    }
}