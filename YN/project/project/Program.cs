using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using project.Models;
using project.Models.Services;

var builder = WebApplication.CreateBuilder(args);

// 加入 MVC、Session 和 DI
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// 註冊 Session（只要寫一次就好）
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 登入逾時時間
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 註冊依賴注入（你的 ITeacherService）
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<TeacherService>();
builder.Services.AddScoped<OrderService>();

// Model層
builder.Services.AddScoped<OrderModel>();
builder.Services.AddScoped<TeacherModel>();
builder.Services.AddScoped<StudentModel>();
builder.Services.AddScoped<adminLoginModel>();
builder.Services.AddScoped<SubjectModel>();
builder.Services.AddScoped<TeacherProfile>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<AdminOrderService>();
builder.Services.AddScoped<AdminOrderDetailService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<FeedbackModel>(); //意見回饋0603
builder.Services.AddScoped<StudentProfile>();
builder.Services.AddScoped<NewsletterModel>();



var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); //放在 UseRouting 之後，UseEndpoints / MapControllerRoute 之前最穩

// ↓ 如果之後你用身份驗證才會需要這個
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllerRoute(
    name: "adminLogin",
    pattern: "admin/{action=Login}/{id?}",
    defaults: new { controller = "adminLogin" }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
