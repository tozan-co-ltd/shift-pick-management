using ai_truck_load_measurement.Filters;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// コンテナにサービスを追加
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(typeof(AccessControlFilter));
    options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
});

// セッションの追加
builder.Services.AddSession();

// クッキー認証に必要なサービスを登録
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultForbidScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    //options.Cookie.IsEssential = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.Name = CookieAuthenticationDefaults.AuthenticationScheme;
    //options.Cookie.MaxAge = TimeSpan.FromMinutes(1440);
    options.LoginPath = "/Login";
    options.SlidingExpiration = false;
    //options.ExpireTimeSpan = TimeSpan.FromMinutes(1440);
});

// コンテナに認可を追加
builder.Services.AddAuthorization(options =>
{
    //options.AddPolicy("1", policy =>
    //{
    //    //policy.RequireClaim(CustomClaimTypes.ClaimType_Role, "1");
    //});
    //options.AddPolicy("test", policy =>
    //{
    //    //policy.RequireClaim(CustomClaimTypes.ClaimType_Role, "test");
    //});
});

// MVCで利用するサービスを登録
builder.Services.AddMvc(options =>
{
    // グローバルフィルタに承認フィルタを追加
    // 全てのコントローラーでログイン必須
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.Filters.Add(new AuthorizeFilter(policy));
    options.EnableEndpointRouting = false;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
// HTTPS必須
app.UseHttpsRedirection();
// 静的ファイルの提供
app.UseStaticFiles();
//ルーティング
app.UseRouting();
// Cookie原則機能有効化
app.UseCookiePolicy();
// ID有効化
app.UseAuthentication();
// 認証機能有効化
app.UseAuthorization();
// セッション状態有効化
app.UseSession();
app.UseMvc();

// 規則ルーティング
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Top}/{action=Index}/{id?}");

app.Run();
