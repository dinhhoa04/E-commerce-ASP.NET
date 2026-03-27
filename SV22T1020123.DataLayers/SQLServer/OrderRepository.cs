using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020123.DataLayers.Interfaces;
using SV22T1020123.Models.Common;
using SV22T1020123.Models.Sales;

namespace SV22T1020123.DataLayers.SQLServer
{
    public class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString;

        public OrderRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // =====================================================
        // LẤY DANH SÁCH ĐƠN HÀNG (PHÂN TRANG)
        // =====================================================
        public async Task<PagedResult<OrderViewInfo>> ListAsync(OrderSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new
            {
                Page = input.Page,
                PageSize = input.PageSize,
                SearchValue = input.SearchValue ?? "",
                Status = (int)input.Status,
                DateFrom = input.DateFrom,
                DateTo = input.DateTo
            };

            var sql = @"
SELECT COUNT(*) 
FROM Orders
WHERE (@SearchValue = '' OR OrderID LIKE '%' + @SearchValue + '%')

SELECT o.*, 
       c.CustomerName, c.ContactName AS CustomerContactName,
       c.Email AS CustomerEmail, c.Phone AS CustomerPhone,
       c.Address AS CustomerAddress,
       e.FullName AS EmployeeName,
       s.ShipperName, s.Phone AS ShipperPhone
FROM Orders o
LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
ORDER BY o.OrderID DESC
OFFSET (@Page - 1) * @PageSize ROWS
FETCH NEXT @PageSize ROWS ONLY";

            using var multi = await connection.QueryMultipleAsync(sql, parameters);

            int rowCount = await multi.ReadSingleAsync<int>();
            var data = (await multi.ReadAsync<OrderViewInfo>()).ToList();

            return new PagedResult<OrderViewInfo>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data
            };
        }

        // =====================================================
        // LẤY THÔNG TIN 1 ĐƠN HÀNG
        // =====================================================
        public async Task<OrderViewInfo?> GetAsync(int orderID)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
SELECT o.*, 
       c.CustomerName, c.ContactName AS CustomerContactName,
       c.Email AS CustomerEmail, c.Phone AS CustomerPhone,
       c.Address AS CustomerAddress,
       e.FullName AS EmployeeName,
       s.ShipperName, s.Phone AS ShipperPhone
FROM Orders o
LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
WHERE o.OrderID = @orderID";

            return await connection.QueryFirstOrDefaultAsync<OrderViewInfo>(sql, new { orderID });
        }

        // =====================================================
        // THÊM ĐƠN HÀNG
        // =====================================================
        public async Task<int> AddAsync(Order data)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
INSERT INTO Orders(CustomerID, OrderTime, DeliveryProvince, DeliveryAddress, Status)
VALUES(@CustomerID, @OrderTime, @DeliveryProvince, @DeliveryAddress, @Status);

SELECT CAST(SCOPE_IDENTITY() AS INT)";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        // =====================================================
        // CẬP NHẬT ĐƠN HÀNG
        // =====================================================
        public async Task<bool> UpdateAsync(Order data)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
UPDATE Orders
SET CustomerID = @CustomerID,
    DeliveryProvince = @DeliveryProvince,
    DeliveryAddress = @DeliveryAddress,
    EmployeeID = @EmployeeID,
    AcceptTime = @AcceptTime,
    ShipperID = @ShipperID,
    ShippedTime = @ShippedTime,
    FinishedTime = @FinishedTime,
    Status = @Status
WHERE OrderID = @OrderID";

            return await connection.ExecuteAsync(sql, data) > 0;
        }

        // =====================================================
        // XÓA ĐƠN HÀNG
        // =====================================================
        public async Task<bool> DeleteAsync(int orderID)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = "DELETE FROM Orders WHERE OrderID = @orderID";

            return await connection.ExecuteAsync(sql, new { orderID }) > 0;
        }

        // =====================================================
        // LẤY DANH SÁCH CHI TIẾT ĐƠN HÀNG
        // =====================================================
        public async Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
SELECT d.*, p.ProductName, p.Unit, p.Photo
FROM OrderDetails d
JOIN Products p ON d.ProductID = p.ProductID
WHERE d.OrderID = @orderID";

            var data = await connection.QueryAsync<OrderDetailViewInfo>(sql, new { orderID });
            return data.ToList();
        }

        // =====================================================
        // LẤY 1 CHI TIẾT ĐƠN HÀNG
        // =====================================================
        public async Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
SELECT d.*, p.ProductName, p.Unit, p.Photo
FROM OrderDetails d
JOIN Products p ON d.ProductID = p.ProductID
WHERE d.OrderID = @orderID AND d.ProductID = @productID";

            return await connection.QueryFirstOrDefaultAsync<OrderDetailViewInfo>(sql, new { orderID, productID });
        }

        // =====================================================
        // THÊM CHI TIẾT ĐƠN HÀNG
        // =====================================================
        public async Task<bool> AddDetailAsync(OrderDetail data)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
INSERT INTO OrderDetails(OrderID, ProductID, Quantity, SalePrice)
VALUES(@OrderID, @ProductID, @Quantity, @SalePrice)";

            return await connection.ExecuteAsync(sql, data) > 0;
        }

        // =====================================================
        // CẬP NHẬT CHI TIẾT ĐƠN HÀNG
        // =====================================================
        public async Task<bool> UpdateDetailAsync(OrderDetail data)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
UPDATE OrderDetails
SET Quantity = @Quantity,
    SalePrice = @SalePrice
WHERE OrderID = @OrderID
AND ProductID = @ProductID";

            return await connection.ExecuteAsync(sql, data) > 0;
        }

        // =====================================================
        // XÓA CHI TIẾT ĐƠN HÀNG
        // =====================================================
        public async Task<bool> DeleteDetailAsync(int orderID, int productID)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
DELETE FROM OrderDetails
WHERE OrderID = @orderID
AND ProductID = @productID";

            return await connection.ExecuteAsync(sql, new { orderID, productID }) > 0;
        }
    }
}