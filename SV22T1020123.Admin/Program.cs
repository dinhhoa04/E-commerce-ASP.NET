using Microsoft.AspNetCore.Authentication.Cookies;
using SV22T1020123.Admin;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// 1. CẤU HÌNH CÁC DỊCH VỤ (SERVICES) VÀO CONTAINER
// Mọi lệnh builder.Services... ĐỀU PHẢI ĐẶT TRƯỚC builder.Build()
// =========================================================

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews()
    .AddMvcOptions(option =>
    {
        option.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    });

// Cấu hình Session
builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromHours(2);
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
});

// Cấu hình Authentication (Bật cơ chế Đăng nhập Admin)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.Cookie.Name = "LiteCommerce.Admin"; // Tên cookie riêng cho Admin
        option.LoginPath = "/Account/Login";       // Trang đẩy về nếu chưa đăng nhập
        option.AccessDeniedPath = "/Account/AccessDenied";
        option.ExpireTimeSpan = TimeSpan.FromDays(7);
        option.SlidingExpiration = true;
        option.Cookie.HttpOnly = true;
        option.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

// =========================================================
// 2. BIÊN DỊCH VÀ XÂY DỰNG ỨNG DỤNG
// =========================================================
var app = builder.Build();

// =========================================================
// 3. KHỞI TẠO CÁC CẤU HÌNH VÀ KẾT NỐI (DATA, CULTURE, CONTEXT)
// =========================================================

// Cấu hình định dạng ngôn ngữ (Tiếng Việt)
var cultureInfo = new CultureInfo("vi-VN");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Khởi tạo ApplicationContext
ApplicationContext.Configure(
    httpContextAccessor: app.Services.GetRequiredService<IHttpContextAccessor>(),
    webHostEnvironment: app.Services.GetRequiredService<IWebHostEnvironment>(),
    configuration: app.Configuration
);

// Khởi tạo kết nối CSDL cho tầng BusinessLayer
string connectionString = app.Configuration.GetConnectionString("LiteCommerceDB")
    ?? throw new InvalidOperationException("ConnectionString 'LiteCommerceDB' not found.");
SV22T1020123.BusinessLayers.Configuration.Initialize(connectionString);

// =========================================================
// 4. CẤU HÌNH MIDDLEWARE (PIPELINE XỬ LÝ REQUEST)
// Lưu ý: Thứ tự các lệnh app.Use... là cực kỳ quan trọng!
// =========================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

// Đảm bảo đúng thứ tự: Session -> Authentication -> Authorization
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Cấu hình đường dẫn mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();