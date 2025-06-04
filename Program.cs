using ContentCraft_studio.Models;
using ContentCraft_studio.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Auth0.AspNetCore.Authentication;
using System.Net;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;

using Microsoft.AspNetCore.DataProtection;
using System.IO;
using DotNetEnv;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add configuration sources
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Configure data protection with a fallback mechanism
if (OperatingSystem.IsWindows())
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "keys")))
        .ProtectKeysWithDpapi();
}
else
{
    // For Linux/Docker environment
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "keys")))
        .SetApplicationName("ContentCraft_studio");
}

builder.Services.AddControllersWithViews();


builder.Services.AddAuth0WebAppAuthentication(options => {
    options.Domain = Environment.GetEnvironmentVariable("Auth0__Domain") ?? 
        throw new InvalidOperationException("Auth0:Domain environment variable is missing");
    options.ClientId = Environment.GetEnvironmentVariable("Auth0__ClientId") ?? 
        throw new InvalidOperationException("Auth0:ClientId environment variable is missing");
    options.ClientSecret = Environment.GetEnvironmentVariable("Auth0__ClientSecret") ?? 
        throw new InvalidOperationException("Auth0:ClientSecret environment variable is missing");
});

// Configure OpenID Connect callback path
builder.Services.Configure<OpenIdConnectOptions>("Auth0", options => {
    options.CallbackPath = new PathString("/callback");
});

builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/Account/SignIn";
    options.LogoutPath = "/Account/SignOut";
    options.ReturnUrlParameter = "returnUrl";
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.Configure<GeminiOptions>(options =>
{
    options.ApiKey = Environment.GetEnvironmentVariable("Gemini__ApiKey") ??
        throw new InvalidOperationException("Gemini:ApiKey environment variable is missing");
});
builder.Services.AddHttpClient();
builder.Services.AddScoped<IGeminiService, GeminiService>();
builder.Services.AddScoped<IMongoDbService, MongoDbService>();

// Configure MongoDB settings
builder.Services.Configure<MongoDbOptions>(options =>
{
    options.ConnectionString = Environment.GetEnvironmentVariable("MongoDb__ConnectionString") ??
        throw new InvalidOperationException("MongoDb:ConnectionString environment variable is missing");
    options.DatabaseName = builder.Configuration["MongoDb:DatabaseName"] ??
        throw new InvalidOperationException("MongoDb:DatabaseName configuration is missing");
    options.ImageDescriptionsCollectionName = builder.Configuration["MongoDb:ImageDescriptionsCollectionName"] ??
        throw new InvalidOperationException("MongoDb:ImageDescriptionsCollectionName configuration is missing");
    options.UsersCollectionName = builder.Configuration["MongoDb:UsersCollectionName"] ??
        throw new InvalidOperationException("MongoDb:UsersCollectionName configuration is missing");
    options.BusinessNamesCollectionName = builder.Configuration["MongoDb:BusinessNamesCollectionName"] ??
        throw new InvalidOperationException("MongoDb:BusinessNamesCollectionName configuration is missing");
    options.BlogPostsCollectionName = builder.Configuration["MongoDb:BlogPostsCollectionName"] ??
        throw new InvalidOperationException("MongoDb:BlogPostsCollectionName configuration is missing");
    options.StoriesCollectionName = builder.Configuration["MongoDb:StoriesCollectionName"] ??
        throw new InvalidOperationException("MongoDb:StoriesCollectionName configuration is missing");
});

// Add session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.Name = ".ContentCraft.Session";
});

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
app.UseSession();

// Add health check endpoint
app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
