using GeminiAspNetDemo.Models;
using GeminiAspNetDemo.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Auth0 Authentication
var auth0Settings = builder.Configuration.GetSection("Auth0").Get<Auth0Settings>();
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Auth0Constants.AuthenticationScheme;
}).AddAuth0WebAppAuthentication(options => {
    options.Domain = auth0Settings.Domain;
    options.ClientId = auth0Settings.ClientId;
    options.ClientSecret = auth0Settings.ClientSecret;
    options.CallbackPath = new PathString("/callback");
    options.ResponseType = "code";
    options.Scope = "openid profile email";
    options.SaveTokens = true;
    options.UseRefreshTokens = true;
});


builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});


builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.Name = "auth_cookie";
    options.Cookie.IsEssential = true;
    options.Events = new CookieAuthenticationEvents
    {
        OnSigningOut = async context =>
        {
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await context.HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme);
            context.CookieOptions.Expires = DateTimeOffset.Now.AddDays(-1);
            foreach (var cookie in context.HttpContext.Request.Cookies.Keys)
            {
                context.HttpContext.Response.Cookies.Delete(cookie);
            }
        }
    };
});

builder.Services.Configure<Auth0Settings>(builder.Configuration.GetSection("Auth0"));
builder.Services.Configure<GeminiOptions>(
    builder.Configuration.GetSection("Gemini"));
builder.Services.AddHttpClient();
builder.Services.AddScoped<IGeminiService, GeminiService>();

// Add HTTPS configuration
builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 7098;
});

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
