using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020123.DataLayers.Interfaces;
using SV22T1020123.Models.Security;

namespace SV22T1020123.DataLayers.SQLServer
{
    /// <summary>
    /// Thực hiện các thao tác truy xuất dữ liệu liên quan đến
    /// tài khoản của khách hàng
    /// </summary>
    public class CustomerAccountRepository : IUserAccountRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo CustomerAccountRepository
        /// </summary>
        /// <param name="connectionString"></param>
        public CustomerAccountRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Kiểm tra đăng nhập của khách hàng
        /// </summary>
        public async Task<UserAccount?> AuthorizeAsync(string userName, string password)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT 
                                CustomerID AS UserId,
                                Email AS UserName,
                                CustomerName AS DisplayName,
                                Email,
                                '' AS Photo,
                                'Customer' AS RoleNames
                           FROM Customers
                           WHERE Email = @userName
                           AND Password = @password";

            return await connection.QueryFirstOrDefaultAsync<UserAccount>(sql, new
            {
                userName,
                password
            });
        }

        /// <summary>
        /// Đổi mật khẩu của khách hàng
        /// </summary>
        public async Task<bool> ChangePasswordAsync(string userName, string password)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"UPDATE Customers
                           SET Password = @password
                           WHERE Email = @userName";

            return await connection.ExecuteAsync(sql, new
            {
                userName,
                password
            }) > 0;
        }
    }
}