
using BilgeShop.Business.Managers;
using BilgeShop.Business.Services;
using BilgeShop.Data.Context;
using BilgeShop.Data.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<BilgeShopContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped(typeof(IRepository<>), typeof(SqlRepository<>));
// IRepository'e dependency injection yap�ld���nda, SqlRepository nesnesi g�nder.

builder.Services.AddScoped<IUserService, UserManager>();
// AddScoped -> Her istek i�in yeni bir kopya
builder.Services.AddScoped<ICategoryService, CategoryManager>();
builder.Services.AddScoped<IProductService, ProductManager>();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = new PathString("/");
    options.LogoutPath = new PathString("/");
    options.AccessDeniedPath = new PathString("/");
    // giri� - ��k�� - eri�im reddi durumlar�nda ana sayfaya y�nlendiriyorum.
});



var app = builder.Build();

app.UseStaticFiles(); // wwwroot kullan�lacak.

app.UseAuthentication();
app.UseAuthorization();
// Kimlik belirleme ve yetkilendirme i�lemleri i�in gerekli.

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=dashboard}/{action=index}/{id?}"
    );

// default route her zaman en altta.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=home}/{action=index}/{id?}"
    );



// app.MapDefaultControllerRoute(); �sttekinin k�sa yolu.


app.Run();
