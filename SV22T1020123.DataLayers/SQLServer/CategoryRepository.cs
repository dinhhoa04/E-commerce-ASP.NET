using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020123.DataLayers.Interfaces;
using SV22T1020123.Models.Common;
using SV22T1020123.Models.Catalog;

namespace SV22T1020123.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp thực hiện các thao tác truy xuất dữ liệu đối với bảng Categories
    /// trong cơ sở dữ liệu SQL Server.
    /// 
    /// Lớp này cài đặt interface IGenericRepository<Category>
    /// và sử dụng thư viện Dapper để thao tác dữ liệu.
    /// </summary>
    public class CategoryRepository : IGenericRepository<Category>
    {
        /// <summary>
        /// Chuỗi kết nối đến cơ sở dữ liệu
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// Constructor khởi tạo repository
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối đến SQL Server</param>
        public CategoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Truy vấn danh sách loại hàng theo điều kiện tìm kiếm
        /// và trả về kết quả dưới dạng phân trang
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang</param>
        /// <returns>Kết quả phân trang chứa danh sách Category</returns>
        public async Task<PagedResult<Category>> ListAsync(PaginationSearchInput input)
        {
            var result = new PagedResult<Category>()
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var parameters = new
                {
                    searchValue = input.SearchValue,
                    offset = input.Offset,
                    pageSize = input.PageSize
                };

                // Đếm số dòng
                string countSql = @"SELECT COUNT(*)
                                    FROM Categories
                                    WHERE CategoryName LIKE '%' + @searchValue + '%'";

                result.RowCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

                // Truy vấn dữ liệu
                string querySql = @"SELECT *
                                    FROM Categories
                                    WHERE CategoryName LIKE '%' + @searchValue + '%'
                                    ORDER BY CategoryName
                                    OFFSET @offset ROWS
                                    FETCH NEXT @pageSize ROWS ONLY";

                if (input.PageSize == 0)
                {
                    querySql = @"SELECT *
                                 FROM Categories
                                 WHERE CategoryName LIKE '%' + @searchValue + '%'
                                 ORDER BY CategoryName";
                }

                var data = await connection.QueryAsync<Category>(querySql, parameters);
                result.DataItems = data.ToList();
            }

            return result;
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một loại hàng theo mã CategoryID
        /// </summary>
        /// <param name="id">Mã loại hàng</param>
        /// <returns>Đối tượng Category nếu tồn tại, ngược lại trả về null</returns>
        public async Task<Category?> GetAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT *
                               FROM Categories
                               WHERE CategoryID = @id";

                return await connection.QueryFirstOrDefaultAsync<Category>(sql, new { id });
            }
        }

        /// <summary>
        /// Thêm mới một loại hàng vào bảng Categories
        /// </summary>
        /// <param name="data">Thông tin loại hàng</param>
        /// <returns>Mã CategoryID của bản ghi vừa được thêm</returns>
        public async Task<int> AddAsync(Category data)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"INSERT INTO Categories(CategoryName, Description)
                               VALUES(@CategoryName, @Description);
                               SELECT CAST(SCOPE_IDENTITY() AS INT);";

                int id = await connection.ExecuteScalarAsync<int>(sql, data);
                return id;
            }
        }

        /// <summary>
        /// Cập nhật thông tin của một loại hàng
        /// </summary>
        /// <param name="data">Dữ liệu cần cập nhật</param>
        /// <returns>True nếu cập nhật thành công, False nếu không tìm thấy bản ghi</returns>
        public async Task<bool> UpdateAsync(Category data)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"UPDATE Categories
                               SET CategoryName = @CategoryName,
                                   Description = @Description
                               WHERE CategoryID = @CategoryID";

                int rows = await connection.ExecuteAsync(sql, data);
                return rows > 0;
            }
        }

        /// <summary>
        /// Xóa một loại hàng theo mã CategoryID
        /// </summary>
        /// <param name="id">Mã loại hàng</param>
        /// <returns>True nếu xóa thành công, False nếu không tồn tại</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"DELETE FROM Categories
                               WHERE CategoryID = @id";

                int rows = await connection.ExecuteAsync(sql, new { id });
                return rows > 0;
            }
        }

        /// <summary>
        /// Kiểm tra loại hàng có đang được sử dụng trong bảng Products hay không
        /// </summary>
        /// <param name="id">Mã loại hàng</param>
        /// <returns>True nếu đang được sử dụng, False nếu không</returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT COUNT(*)
                               FROM Products
                               WHERE CategoryID = @id";

                int count = await connection.ExecuteScalarAsync<int>(sql, new { id });
                return count > 0;
            }
        }
    }
}