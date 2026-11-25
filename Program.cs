using Microsoft.AspNetCore.Authentication.Cookies;
using RepairLab.Data;

var b = WebApplication.CreateBuilder(args);
b.Services.AddControllersWithViews();
b.Services.AddSingleton<Db>();
b.Services.AddScoped<UserRepository>();
b.Services.AddScoped<CaseRepository>();
b.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o => { o.LoginPath = "/Account/Login"; o.AccessDeniedPath = "/Account/Login"; });

var app = b.Build();
app.Services.GetRequiredService<Db>().Init();   // создаёт БД, таблицы и демо-данные
app.UseStatusCodePagesWithReExecute("/Home/Error404");  // страница 404
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();
