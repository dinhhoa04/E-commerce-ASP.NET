using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020123.DataLayers.Interfaces;
using SV22T1020123.Models.Common;
using SV22T1020123.Models.HR;

namespace SV22T1020123.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp thực hiện các thao tác truy xuất dữ liệu đối với bảng Employees
    /// trong cơ sở dữ liệu SQL Server.
    /// 
    /// Lớp này cài đặt interface IEmployeeRepository
    /// và sử dụng thư viện Dapper để thao tác dữ liệu.
    /// </summary>
    public class EmployeeRepository : IEmployeeRepository
    {
        /// <summary>
        /// Chuỗi kết nối đến cơ sở dữ liệu
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// Constructor khởi tạo repository
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối SQL Server</param>
        public EmployeeRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Truy vấn danh sách nhân viên theo điều kiện tìm kiếm
        /// và trả về kết quả dưới dạng phân trang
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang</param>
        /// <returns>Kết quả phân trang chứa danh sách Employee</returns>
        public async Task<PagedResult<Employee>> ListAsync(PaginationSearchInput input)
        {
            var result = new PagedResult<Employee>()
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
                                    FROM Employees
                                    WHERE FullName LIKE '%' + @searchValue + '%'
                                       OR Phone LIKE '%' + @searchValue + '%'";

                result.RowCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

                // Truy vấn dữ liệu
                string querySql = @"SELECT EmployeeID, FullName, BirthDate, Address,
                                           Phone, Email, Photo, IsWorking
                                    FROM Employees
                                    WHERE FullName LIKE '%' + @searchValue + '%'
                                       OR Phone LIKE '%' + @searchValue + '%'
                                    ORDER BY FullName
                                    OFFSET @offset ROWS
                                    FETCH NEXT @pageSize ROWS ONLY";

                if (input.PageSize == 0)
                {
                    querySql = @"SELECT EmployeeID, FullName, BirthDate, Address,
                                        Phone, Email, Photo, IsWorking
                                 FROM Employees
                                 WHERE FullName LIKE '%' + @searchValue + '%'
                                    OR Phone LIKE '%' + @searchValue + '%'
                                 ORDER BY FullName";
                }

                var data = await connection.QueryAsync<Employee>(querySql, parameters);
                result.DataItems = data.ToList();
            }

            return result;
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một nhân viên theo mã EmployeeID
        /// </summary>
        /// <param name="id">Mã nhân viên</param>
        /// <returns>Đối tượng Employee nếu tồn tại, ngược lại trả về null</returns>
        public async Task<Employee?> GetAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT EmployeeID, FullName, BirthDate, Address,
                                      Phone, Email, Photo, IsWorking
                               FROM Employees
                               WHERE EmployeeID = @id";

                return await connection.QueryFirstOrDefaultAsync<Employee>(sql, new { id });
            }
        }

        /// <summary>
        /// Thêm mới một nhân viên vào bảng Employees
        /// </summary>
        /// <param name="data">Thông tin nhân viên</param>
        /// <returns>Mã EmployeeID của bản ghi vừa được thêm</returns>
        public async Task<int> AddAsync(Employee data)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"INSERT INTO Employees
                               (FullName, BirthDate, Address, Phone, Email, Photo, IsWorking)
                               VALUES
                               (@FullName, @BirthDate, @Address, @Phone, @Email, @Photo, @IsWorking);
                               SELECT CAST(SCOPE_IDENTITY() AS INT);";

                int id = await connection.ExecuteScalarAsync<int>(sql, data);
                return id;
            }
        }

        /// <summary>
        /// Cập nhật thông tin của một nhân viên
        /// </summary>
        /// <param name="data">Dữ liệu cần cập nhật</param>
        /// <returns>True nếu cập nhật thành công, False nếu không tìm thấy bản ghi</returns>
        public async Task<bool> UpdateAsync(Employee data)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"UPDATE Employees
                               SET FullName = @FullName,
                                   BirthDate = @BirthDate,
                                   Address = @Address,
                                   Phone = @Phone,
                                   Email = @Email,
                                   Photo = @Photo,
                                   IsWorking = @IsWorking
                               WHERE EmployeeID = @EmployeeID";

                int rows = await connection.ExecuteAsync(sql, data);
                return rows > 0;
            }
        }

        /// <summary>
        /// Xóa một nhân viên theo mã EmployeeID
        /// </summary>
        /// <param name="id">Mã nhân viên</param>
        /// <returns>True nếu xóa thành công, False nếu không tồn tại</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"DELETE FROM Employees
                               WHERE EmployeeID = @id";

                int rows = await connection.ExecuteAsync(sql, new { id });
                return rows > 0;
            }
        }

        /// <summary>
        /// Kiểm tra nhân viên có đang được sử dụng trong bảng Orders hay không
        /// </summary>
        /// <param name="id">Mã nhân viên</param>
        /// <returns>True nếu đang được sử dụng, False nếu không</returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT COUNT(*)
                               FROM Orders
                               WHERE EmployeeID = @id";

                int count = await connection.ExecuteScalarAsync<int>(sql, new { id });
                return count > 0;
            }
        }

        /// <summary>
        /// Kiểm tra email của nhân viên có hợp lệ hay không (không bị trùng)
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
                            FROM Employees
                            WHERE Email = @email";
                }
                else
                {
                    sql = @"SELECT COUNT(*)
                            FROM Employees
                            WHERE Email = @email
                              AND EmployeeID <> @id";
                }

                int count = await connection.ExecuteScalarAsync<int>(sql, new { email, id });
                return count == 0;
            }
        }
    }
}