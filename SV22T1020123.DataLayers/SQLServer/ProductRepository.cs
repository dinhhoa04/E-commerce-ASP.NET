using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020123.DataLayers.Interfaces;
using SV22T1020123.Models.Catalog;
using SV22T1020123.Models.Common;

namespace SV22T1020123.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp thực hiện các thao tác truy xuất dữ liệu cho mặt hàng (Products)
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        public ProductRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Tìm kiếm mặt hàng và trả về dữ liệu phân trang
        /// </summary>
        public async Task<PagedResult<Product>> ListAsync(ProductSearchInput input)
        {
            var result = new PagedResult<Product>()
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            using var connection = new SqlConnection(_connectionString);

            string where = @"WHERE (ProductName LIKE '%' + @SearchValue + '%')";

            if (input.CategoryID > 0)
                where += " AND CategoryID = @CategoryID";

            if (input.SupplierID > 0)
                where += " AND SupplierID = @SupplierID";

            if (input.MinPrice > 0)
                where += " AND Price >= @MinPrice";

            if (input.MaxPrice > 0)
                where += " AND Price <= @MaxPrice";

            string countSql = $"SELECT COUNT(*) FROM Products {where}";

            result.RowCount = await connection.ExecuteScalarAsync<int>(countSql, input);

            string sql = $@"
                SELECT *
                FROM Products
                {where}
                ORDER BY ProductName
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            var data = await connection.QueryAsync<Product>(sql, input);

            result.DataItems = data.ToList();

            return result;
        }

        /// <summary>
        /// Lấy thông tin chi tiết 1 mặt hàng
        /// </summary>
        public async Task<Product?> GetAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT * 
                           FROM Products
                           WHERE ProductID = @productID";

            return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { productID });
        }

        /// <summary>
        /// Thêm mặt hàng mới
        /// </summary>
        public async Task<int> AddAsync(Product data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                INSERT INTO Products
                (ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo, IsSelling)
                VALUES
                (@ProductName, @ProductDescription, @SupplierID, @CategoryID, @Unit, @Price, @Photo, @IsSelling);

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        /// <summary>
        /// Cập nhật mặt hàng
        /// </summary>
        public async Task<bool> UpdateAsync(Product data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                UPDATE Products
                SET ProductName = @ProductName,
                    ProductDescription = @ProductDescription,
                    SupplierID = @SupplierID,
                    CategoryID = @CategoryID,
                    Unit = @Unit,
                    Price = @Price,
                    Photo = @Photo,
                    IsSelling = @IsSelling
                WHERE ProductID = @ProductID";

            int rows = await connection.ExecuteAsync(sql, data);

            return rows > 0;
        }

        /// <summary>
        /// Xóa mặt hàng
        /// </summary>
        public async Task<bool> DeleteAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"DELETE FROM Products WHERE ProductID = @productID";

            int rows = await connection.ExecuteAsync(sql, new { productID });

            return rows > 0;
        }

        /// <summary>
        /// Kiểm tra mặt hàng có đang được sử dụng trong OrderDetails không
        /// </summary>
        public async Task<bool> IsUsedAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT COUNT(*) 
                           FROM OrderDetails
                           WHERE ProductID = @productID";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { productID });

            return count > 0;
        }

        // ========================
        // PRODUCT ATTRIBUTES
        // ========================

        /// <summary>
        /// Lấy danh sách thuộc tính của mặt hàng
        /// </summary>
        public async Task<List<ProductAttribute>> ListAttributesAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT *
                           FROM ProductAttributes
                           WHERE ProductID = @productID
                           ORDER BY DisplayOrder";

            var data = await connection.QueryAsync<ProductAttribute>(sql, new { productID });

            return data.ToList();
        }

        /// <summary>
        /// Lấy thông tin 1 thuộc tính
        /// </summary>
        public async Task<ProductAttribute?> GetAttributeAsync(long attributeID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT *
                           FROM ProductAttributes
                           WHERE AttributeID = @attributeID";

            return await connection.QueryFirstOrDefaultAsync<ProductAttribute>(sql, new { attributeID });
        }

        /// <summary>
        /// Thêm thuộc tính
        /// </summary>
        public async Task<long> AddAttributeAsync(ProductAttribute data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                INSERT INTO ProductAttributes
                (ProductID, AttributeName, AttributeValue, DisplayOrder)
                VALUES
                (@ProductID, @AttributeName, @AttributeValue, @DisplayOrder);

                SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

            return await connection.ExecuteScalarAsync<long>(sql, data);
        }

        /// <summary>
        /// Cập nhật thuộc tính
        /// </summary>
        public async Task<bool> UpdateAttributeAsync(ProductAttribute data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                UPDATE ProductAttributes
                SET AttributeName = @AttributeName,
                    AttributeValue = @AttributeValue,
                    DisplayOrder = @DisplayOrder
                WHERE AttributeID = @AttributeID";

            int rows = await connection.ExecuteAsync(sql, data);

            return rows > 0;
        }

        /// <summary>
        /// Xóa thuộc tính
        /// </summary>
        public async Task<bool> DeleteAttributeAsync(long attributeID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"DELETE FROM ProductAttributes
                           WHERE AttributeID = @attributeID";

            int rows = await connection.ExecuteAsync(sql, new { attributeID });

            return rows > 0;
        }

        // ========================
        // PRODUCT PHOTOS
        // ========================

        /// <summary>
        /// Lấy danh sách ảnh của mặt hàng
        /// </summary>
        public async Task<List<ProductPhoto>> ListPhotosAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT *
                           FROM ProductPhotos
                           WHERE ProductID = @productID
                           ORDER BY DisplayOrder";

            var data = await connection.QueryAsync<ProductPhoto>(sql, new { productID });

            return data.ToList();
        }

        /// <summary>
        /// Lấy thông tin 1 ảnh
        /// </summary>
        public async Task<ProductPhoto?> GetPhotoAsync(long photoID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT *
                           FROM ProductPhotos
                           WHERE PhotoID = @photoID";

            return await connection.QueryFirstOrDefaultAsync<ProductPhoto>(sql, new { photoID });
        }

        /// <summary>
        /// Thêm ảnh
        /// </summary>
        public async Task<long> AddPhotoAsync(ProductPhoto data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                INSERT INTO ProductPhotos
                (ProductID, Photo, Description, DisplayOrder, IsHidden)
                VALUES
                (@ProductID, @Photo, @Description, @DisplayOrder, @IsHidden);

                SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

            return await connection.ExecuteScalarAsync<long>(sql, data);
        }

        /// <summary>
        /// Cập nhật ảnh
        /// </summary>
        public async Task<bool> UpdatePhotoAsync(ProductPhoto data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                UPDATE ProductPhotos
                SET Photo = @Photo,
                    Description = @Description,
                    DisplayOrder = @DisplayOrder,
                    IsHidden = @IsHidden
                WHERE PhotoID = @PhotoID";

            int rows = await connection.ExecuteAsync(sql, data);

            return rows > 0;
        }

        /// <summary>
        /// Xóa ảnh
        /// </summary>
        public async Task<bool> DeletePhotoAsync(long photoID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"DELETE FROM ProductPhotos
                           WHERE PhotoID = @photoID";

            int rows = await connection.ExecuteAsync(sql, new { photoID });

            return rows > 0;
        }
    }
}