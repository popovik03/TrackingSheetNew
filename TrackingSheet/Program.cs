using Microsoft.EntityFrameworkCore;
using TrackingSheet.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using TrackingSheet.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//Мои сервисы
builder.Services.AddScoped<RemoteDataService>();
builder.Services.AddScoped<QuarterYearStatisticsService>();


string rootPath = builder.Configuration.GetValue<string>("FolderIndexing:RootPath");
string outputPath = builder.Configuration.GetValue<string>("FolderIndexing:OutputPath");
builder.Services.AddSingleton(new PassportFolderIndexerService(rootPath, outputPath));
builder.Services.AddSingleton(new PassportFolderSearchService(outputPath));
//builder.Services.AddSingleton<IConfiguration>(builder.Configuration); ;

builder.Services.AddSession();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(1000);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
//логирование
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});
//Сервис Entity
builder.Services.AddDbContext<MVCDbContext>(options => 
    options.UseSqlServer(builder.Configuration
    .GetConnectionString("MVCDbConnectionString")));
//Сервис для аутентификации
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option => {
    option.LoginPath = "/Access/Login";
    option.ExpireTimeSpan = TimeSpan.FromMinutes(120);
    }); 

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
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Access}/{action=Login}/{id?}");

app.MapControllers(); // Это нужно для маршрутизации API

app.Run();
