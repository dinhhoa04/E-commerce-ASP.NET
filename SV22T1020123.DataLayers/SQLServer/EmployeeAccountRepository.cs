using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020123.DataLayers.Interfaces;
using SV22T1020123.Models.Security;
using System.Threading.Tasks;

namespace SV22T1020123.DataLayers.SQLServer
{
    /// <summary>
    /// Thực hiện xử lý đăng nhập và quản lý tài khoản của nhân viên
    /// </summary>
    public class EmployeeAccountRepository : IUserAccountRepository
    {
        private readonly string _connectionString;

        public EmployeeAccountRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Kiểm tra thông tin đăng nhập của nhân viên
        /// </summary>
        public async Task<UserAccount?> AuthorizeAsync(string userName, string password)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<UserAccount>(
                @"SELECT 
                        CAST(EmployeeID AS NVARCHAR(50)) AS UserId,
                        Email AS UserName,
                        FullName AS DisplayName,
                        Email,
                        Photo,
                        ISNULL(RoleNames, '') AS RoleNames
                  FROM Employees
                  WHERE Email = @userName
                        AND Password = @password
                        AND IsWorking = 1",
                new { userName, password });
        }

        /// <summary>
        /// Đổi mật khẩu nhân viên
        /// </summary>
        public async Task<bool> ChangePasswordAsync(string userName, string password)
        {
            using var connection = new SqlConnection(_connectionString);

            int result = await connection.ExecuteAsync(
                @"UPDATE Employees
                  SET Password = @password
                  WHERE Email = @userName",
                new { userName, password });

            return result > 0;
        }
    }
}