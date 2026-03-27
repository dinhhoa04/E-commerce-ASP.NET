using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020123.DataLayers.Interfaces;
using SV22T1020123.Models.Common;
using SV22T1020123.Models.Partner;

namespace SV22T1020123.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp cài đặt các chức năng truy xuất dữ liệu đối với bảng Suppliers
    /// trong SQL Server.
    /// 
    /// Lớp này sử dụng thư viện Dapper để thực hiện các thao tác CRUD
    /// và cài đặt interface IGenericRepository<Supplier>.
    /// </summary>
    public class SupplierRepository : IGenericRepository<Supplier>
    {
        /// <summary>
        /// Chuỗi kết nối đến CSDL
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// Constructor của lớp SupplierRepository
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối đến SQL Server</param>
        public SupplierRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Truy vấn danh sách nhà cung cấp theo điều kiện tìm kiếm
        /// và trả về kết quả dưới dạng phân trang
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang</param>
        /// <returns>Kết quả phân trang chứa danh sách Supplier</returns>
        public async Task<PagedResult<Supplier>> ListAsync(PaginationSearchInput input)
        {
            var result = new PagedResult<Supplier>()
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
                                    FROM Suppliers
                                    WHERE SupplierName LIKE '%' + @searchValue + '%'
                                       OR ContactName LIKE '%' + @searchValue + '%'";

                result.RowCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

                // Lấy dữ liệu
                string querySql = @"SELECT *
                                    FROM Suppliers
                                    WHERE SupplierName LIKE '%' + @searchValue + '%'
                                       OR ContactName LIKE '%' + @searchValue + '%'
                                    ORDER BY SupplierName
                                    OFFSET @offset ROWS
                                    FETCH NEXT @pageSize ROWS ONLY";

                if (input.PageSize == 0)
                {
                    querySql = @"SELECT *
                                 FROM Suppliers
                                 WHERE SupplierName LIKE '%' + @searchValue + '%'
                                    OR ContactName LIKE '%' + @searchValue + '%'
                                 ORDER BY SupplierName";
                }

                var data = await connection.QueryAsync<Supplier>(querySql, parameters);

                result.DataItems = data.ToList();
            }

            return result;
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một nhà cung cấp theo mã SupplierID
        /// </summary>
        /// <param name="id">Mã nhà cung cấp cần tìm</param>
        /// <returns>Đối tượng Supplier nếu tồn tại, ngược lại trả về null</returns>
        public async Task<Supplier?> GetAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT *
                               FROM Suppliers
                               WHERE SupplierID = @id";

                return await connection.QueryFirstOrDefaultAsync<Supplier>(sql, new { id });
            }
        }

        /// <summary>
        /// Thêm mới một nhà cung cấp vào bảng Suppliers
        /// </summary>
        /// <param name="data">Thông tin nhà cung cấp cần thêm</param>
        /// <returns>Mã SupplierID của bản ghi vừa được thêm</returns>
        public async Task<int> AddAsync(Supplier data)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"INSERT INTO Suppliers
                               (SupplierName, ContactName, Province, Address, Phone, Email)
                               VALUES
                               (@SupplierName, @ContactName, @Province, @Address, @Phone, @Email);
                               SELECT CAST(SCOPE_IDENTITY() AS INT);";

                int id = await connection.ExecuteScalarAsync<int>(sql, data);
                return id;
            }
        }

        /// <summary>
        /// Cập nhật thông tin của một nhà cung cấp
        /// </summary>
        /// <param name="data">Dữ liệu cần cập nhật</param>
        /// <returns>True nếu cập nhật thành công, False nếu không tìm thấy bản ghi</returns>
        public async Task<bool> UpdateAsync(Supplier data)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"UPDATE Suppliers
                               SET SupplierName = @SupplierName,
                                   ContactName = @ContactName,
                                   Province = @Province,
                                   Address = @Address,
                                   Phone = @Phone,
                                   Email = @Email
                               WHERE SupplierID = @SupplierID";

                int rows = await connection.ExecuteAsync(sql, data);
                return rows > 0;
            }
        }

        /// <summary>
        /// Xóa một nhà cung cấp theo mã SupplierID
        /// </summary>
        /// <param name="id">Mã nhà cung cấp cần xóa</param>
        /// <returns>True nếu xóa thành công, False nếu không tồn tại</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"DELETE FROM Suppliers
                               WHERE SupplierID = @id";

                int rows = await connection.ExecuteAsync(sql, new { id });
                return rows > 0;
            }
        }

        /// <summary>
        /// Kiểm tra xem nhà cung cấp có đang được sử dụng
        /// trong bảng Products hay không
        /// </summary>
        /// <param name="id">Mã nhà cung cấp</param>
        /// <returns>True nếu đang được sử dụng, False nếu không</returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT COUNT(*)
                               FROM Products
                               WHERE SupplierID = @id";

                int count = await connection.ExecuteScalarAsync<int>(sql, new { id });
                return count > 0;
            }
        }
    }
}