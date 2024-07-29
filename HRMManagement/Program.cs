using HRMManagement.Models;
using HRMManagement.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<HrmContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<INhanVien, EFNhanVien>();
builder.Services.AddScoped<IChucVu, EFChucVu>();
builder.Services.AddScoped<IPhongBan, EFPhongBan>();
builder.Services.AddScoped<IViTricv, EFViTricv>();


builder.Services.AddAuthentication()
	.AddFacebook(facebookOptions =>
	{
		facebookOptions.AppId = "1466794167575859";
		facebookOptions.AppSecret = "6e3dd7071e19abe3b2b4e393eaf33044";
	})
	.AddGoogle(googleOptions =>
	{
		googleOptions.ClientId = "";
		googleOptions.ClientSecret = "";
	});

//builder.Services.AddIdentity<Taikhoan, IdentityRole>(options =>
//{
//	// Set password requirements
//	options.Password.RequireDigit = true;
//	options.Password.RequireLowercase = true;
//	options.Password.RequireUppercase = true;
//	options.Password.RequireNonAlphanumeric = true;
//	options.Password.RequiredLength = 6;

//	// Set lockout settings
//	options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
//	options.Lockout.MaxFailedAccessAttempts = 5;
//	options.Lockout.AllowedForNewUsers = true;

//	//// Set user options
//	//options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
//	//options.User.RequireUniqueEmail = true;
//})
//.AddEntityFrameworkStores<HrmContext>()
//.AddSignInManager<SignInManager<Taikhoan>>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}");

app.Use(async (ctx, next) =>
{
	bool isExistCookies = ctx.Request.Cookies.ContainsKey("UserId") && ctx.Request.Cookies.ContainsKey("UserName");
	bool isPublicRoute = ctx.Request.Path == "/" || ctx.Request.Path == "/OAuth/Login" || ctx.Request.Path == "/OAuth/Register";

	if (!isExistCookies && !isPublicRoute && ctx.Request.Path != "/404")
	{
		ctx.Response.Redirect("/OAuth/Login");
		return;
	}

	if (isExistCookies)
	{
		if (isPublicRoute) {
			ctx.Response.Redirect("/dashboard");
			return;
		}
	}

	if (ctx.Response.StatusCode == 404)
	{
		ctx.Request.Path = "/404";
	}

	await next();
});

app.Run();
