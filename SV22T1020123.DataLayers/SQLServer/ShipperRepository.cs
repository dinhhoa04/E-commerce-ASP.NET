using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020123.DataLayers.Interfaces;
using SV22T1020123.Models.Common;
using SV22T1020123.Models.Partner;

namespace SV22T1020123.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp thực hiện các thao tác truy xuất dữ liệu đối với bảng Shippers
    /// trong cơ sở dữ liệu SQL Server.
    /// 
    /// Lớp này cài đặt interface IGenericRepository<Shipper>
    /// và sử dụng thư viện Dapper để thao tác với dữ liệu.
    /// </summary>
    public class ShipperRepository : IGenericRepository<Shipper>
    {
        /// <summary>
        /// Chuỗi kết nối đến cơ sở dữ liệu
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// Constructor khởi tạo repository
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối đến SQL Server</param>
        public ShipperRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Truy vấn danh sách người giao hàng theo điều kiện tìm kiếm
        /// và trả về kết quả dưới dạng phân trang
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang</param>
        /// <returns>Kết quả phân trang chứa danh sách Shipper</returns>
        public async Task<PagedResult<Shipper>> ListAsync(PaginationSearchInput input)
        {
            var result = new PagedResult<Shipper>()
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
                                    FROM Shippers
                                    WHERE ShipperName LIKE '%' + @searchValue + '%'";

                result.RowCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

                // Truy vấn dữ liệu
                string querySql = @"SELECT *
                                    FROM Shippers
                                    WHERE ShipperName LIKE '%' + @searchValue + '%'
                                    ORDER BY ShipperName
                                    OFFSET @offset ROWS
                                    FETCH NEXT @pageSize ROWS ONLY";

                if (input.PageSize == 0)
                {
                    querySql = @"SELECT *
                                 FROM Shippers
                                 WHERE ShipperName LIKE '%' + @searchValue + '%'
                                 ORDER BY ShipperName";
                }

                var data = await connection.QueryAsync<Shipper>(querySql, parameters);
                result.DataItems = data.ToList();
            }

            return result;
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một người giao hàng theo mã ShipperID
        /// </summary>
        /// <param name="id">Mã người giao hàng</param>
        /// <returns>Đối tượng Shipper nếu tồn tại, ngược lại trả về null</returns>
        public async Task<Shipper?> GetAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT *
                               FROM Shippers
                               WHERE ShipperID = @id";

                return await connection.QueryFirstOrDefaultAsync<Shipper>(sql, new { id });
            }
        }

        /// <summary>
        /// Thêm mới một người giao hàng vào bảng Shippers
        /// </summary>
        /// <param name="data">Thông tin người giao hàng</param>
        /// <returns>Mã ShipperID của bản ghi vừa được thêm</returns>
        public async Task<int> AddAsync(Shipper data)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"INSERT INTO Shippers(ShipperName, Phone)
                               VALUES(@ShipperName, @Phone);
                               SELECT CAST(SCOPE_IDENTITY() AS INT);";

                int id = await connection.ExecuteScalarAsync<int>(sql, data);
                return id;
            }
        }

        /// <summary>
        /// Cập nhật thông tin người giao hàng
        /// </summary>
        /// <param name="data">Dữ liệu cần cập nhật</param>
        /// <returns>True nếu cập nhật thành công, False nếu không tìm thấy dữ liệu</returns>
        public async Task<bool> UpdateAsync(Shipper data)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"UPDATE Shippers
                               SET ShipperName = @ShipperName,
                                   Phone = @Phone
                               WHERE ShipperID = @ShipperID";

                int rows = await connection.ExecuteAsync(sql, data);
                return rows > 0;
            }
        }

        /// <summary>
        /// Xóa một người giao hàng theo mã ShipperID
        /// </summary>
        /// <param name="id">Mã người giao hàng</param>
        /// <returns>True nếu xóa thành công, False nếu không tồn tại</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"DELETE FROM Shippers
                               WHERE ShipperID = @id";

                int rows = await connection.ExecuteAsync(sql, new { id });
                return rows > 0;
            }
        }

        /// <summary>
        /// Kiểm tra người giao hàng có đang được sử dụng trong bảng Orders hay không
        /// </summary>
        /// <param name="id">Mã người giao hàng</param>
        /// <returns>True nếu đang được sử dụng, False nếu không</returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT COUNT(*)
                               FROM Orders
                               WHERE ShipperID = @id";

                int count = await connection.ExecuteScalarAsync<int>(sql, new { id });
                return count > 0;
            }
        }
    }
}