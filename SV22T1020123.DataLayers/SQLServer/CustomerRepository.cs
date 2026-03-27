using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020123.DataLayers.Interfaces;
using SV22T1020123.Models.Common;
using SV22T1020123.Models.Partner;

namespace SV22T1020123.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp thực hiện các thao tác truy xuất dữ liệu đối với bảng Customers
    /// trong cơ sở dữ liệu SQL Server.
    /// 
    /// Lớp này cài đặt interface ICustomerRepository
    /// và sử dụng thư viện Dapper để thao tác dữ liệu.
    /// </summary>
    public class CustomerRepository : ICustomerRepository
    {
        /// <summary>
        /// Chuỗi kết nối đến cơ sở dữ liệu
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// Constructor khởi tạo repository
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối đến SQL Server</param>
        public CustomerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Truy vấn danh sách khách hàng theo điều kiện tìm kiếm
        /// và trả về kết quả dưới dạng phân trang
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang</param>
        /// <returns>Kết quả phân trang chứa danh sách Customer</returns>
        public async Task<PagedResult<Customer>> ListAsync(PaginationSearchInput input)
        {
            var result = new PagedResult<Customer>()
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
                                    FROM Customers
                                    WHERE CustomerName LIKE '%' + @searchValue + '%'
                                       OR ContactName LIKE '%' + @searchValue + '%'
                                       OR Phone LIKE '%' + @searchValue + '%'";

                result.RowCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

                // Lấy dữ liệu
                string querySql = @"SELECT CustomerID, CustomerName, ContactName, Province,
                                           Address, Phone, Email, IsLocked
                                    FROM Customers
                                    WHERE CustomerName LIKE '%' + @searchValue + '%'
                                       OR ContactName LIKE '%' + @searchValue + '%'
                                       OR Phone LIKE '%' + @searchValue + '%'
                                    ORDER BY CustomerName
                                    OFFSET @offset ROWS
                                    FETCH NEXT @pageSize ROWS ONLY";

                if (input.PageSize == 0)
                {
                    querySql = @"SELECT CustomerID, CustomerName, ContactName, Province,
                                        Address, Phone, Email, IsLocked
                                 FROM Customers
                                 WHERE CustomerName LIKE '%' + @searchValue + '%'
                                    OR ContactName LIKE '%' + @searchValue + '%'
                                    OR Phone LIKE '%' + @searchValue + '%'
                                 ORDER BY CustomerName";
                }

                var data = await connection.QueryAsync<Customer>(querySql, parameters);
                result.DataItems = data.ToList();
            }

            return result;
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một khách hàng theo mã CustomerID
        /// </summary>
        /// <param name="id">Mã khách hàng</param>
        /// <returns>Đối tượng Customer nếu tồn tại, ngược lại trả về null</returns>
        public async Task<Customer?> GetAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT CustomerID, CustomerName, ContactName, Province,
                                      Address, Phone, Email, IsLocked
                               FROM Customers
                               WHERE CustomerID = @id";

                return await connection.QueryFirstOrDefaultAsync<Customer>(sql, new { id });
            }
        }

        /// <summary>
        /// Thêm mới một khách hàng vào bảng Customers
        /// </summary>
        /// <param name="data">Thông tin khách hàng</param>
        /// <returns>Mã CustomerID của bản ghi vừa được thêm</returns>
        public async Task<int> AddAsync(Customer data)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"INSERT INTO Customers
                               (CustomerName, ContactName, Province, Address, Phone, Email, IsLocked)
                               VALUES
                               (@CustomerName, @ContactName, @Province, @Address, @Phone, @Email, @IsLocked);
                               SELECT CAST(SCOPE_IDENTITY() AS INT);";

                int id = await connection.ExecuteScalarAsync<int>(sql, data);
                return id;
            }
        }

        /// <summary>
        /// Cập nhật thông tin khách hàng
        /// </summary>
        /// <param name="data">Dữ liệu cần cập nhật</param>
        /// <returns>True nếu cập nhật thành công, False nếu không tồn tại bản ghi</returns>
        public async Task<bool> UpdateAsync(Customer data)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"UPDATE Customers
                               SET CustomerName = @CustomerName,
                                   ContactName = @ContactName,
                                   Province = @Province,
                                   Address = @Address,
                                   Phone = @Phone,
                                   Email = @Email,
                                   IsLocked = @IsLocked
                               WHERE CustomerID = @CustomerID";

                int rows = await connection.ExecuteAsync(sql, data);
                return rows > 0;
            }
        }

        /// <summary>
        /// Xóa một khách hàng theo mã CustomerID
        /// </summary>
        /// <param name="id">Mã khách hàng</param>
        /// <returns>True nếu xóa thành công, False nếu không tồn tại</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"DELETE FROM Customers
                               WHERE CustomerID = @id";

                int rows = await connection.ExecuteAsync(sql, new { id });
                return rows > 0;
            }
        }

        /// <summary>
        /// Kiểm tra khách hàng có đang được sử dụng trong bảng Orders hay không
        /// </summary>
        /// <param name="id">Mã khách hàng</param>
        /// <returns>True nếu đang được sử dụng, False nếu không</returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT COUNT(*)
                               FROM Orders
                               WHERE CustomerID = @id";

                int count = await connection.ExecuteScalarAsync<int>(sql, new { id });
                return count > 0;
            }
        }

        /// <summary>
        /// Kiểm tra email có hợp lệ hay không (không trùng với email khác)
        /// </summary>
        /// <param name="email">Email cần kiểm tra</param>
        /// <param name="id">
        /// id = 0: kiểm tra email khi thêm mới
        /// id <> 0: kiểm tra email khi cập nhật
        /// </param>
        /// <returns>True nếu email hợp lệ, False nếu email đã tồn tại</returns>
        public async Task<bool> ValidateEmailAsync(string email, int id = 0)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql;

                if (id == 0)
                {
                    sql = @"SELECT COUNT(*)
                            FROM Customers
                            WHERE Email = @email";
                }
                else
                {
                    sql = @"SELECT COUNT(*)
                            FROM Customers
                            WHERE Email = @email
                              AND CustomerID <> @id";
                }

                int count = await connection.ExecuteScalarAsync<int>(sql, new { email, id });
                return count == 0;
            }
        }
    }
}