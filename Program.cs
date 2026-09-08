using shift_pick_management.Filters;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// �R���e�i�ɃT�[�r�X��ǉ�
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(typeof(AccessControlFilter));
    options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
});

// �Z�b�V�����̒ǉ�
builder.Services.AddSession();

// �N�b�L�[�F�؂ɕK�v�ȃT�[�r�X��o�^
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

// �R���e�i�ɔF��ǉ�
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

// MVC�ŗ��p����T�[�r�X��o�^
builder.Services.AddMvc(options =>
{
    // �O���[�o���t�B���^�ɏ��F�t�B���^��ǉ�
    // �S�ẴR���g���[���[�Ń��O�C���K�{
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
// HTTPS�K�{
app.UseHttpsRedirection();
// �ÓI�t�@�C���̒�
app.UseStaticFiles();
//���[�e�B���O
app.UseRouting();
// Cookie�����@�\�L����
app.UseCookiePolicy();
// ID�L����
app.UseAuthentication();
// �F�؋@�\�L����
app.UseAuthorization();
// �Z�b�V������ԗL����
app.UseSession();
app.UseMvc();

// �K�����[�e�B���O
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Top}/{action=Index}/{id?}");

app.Run();
