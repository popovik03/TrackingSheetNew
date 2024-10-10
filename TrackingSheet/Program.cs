using Microsoft.EntityFrameworkCore;
using TrackingSheet.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using TrackingSheet.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.Exchange.WebServices.Data;
using TrackingSheet.Services.TelegramService;
using Telegram.Bot;
//using TrackingSheet.Services.WellInspectorServices;

var builder = WebApplication.CreateBuilder(args);


// Настройка MVC для использования Newtonsoft.Json с игнорированием циклических ссылок
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    );
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken"; // Устанавливаем имя заголовка
});

//Мои сервисы
builder.Services.AddScoped<RemoteDataService>();
//builder.Services.AddScoped<WellInspectorService>();
builder.Services.AddScoped<QuarterYearStatisticsService>();
builder.Services.AddScoped<IKanbanService, KanbanService>();

// Регистрация TelegramBotClient
var botToken = builder.Configuration["TelegramBot:Token"];
builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));

// Регистрация службы для Long Polling
builder.Services.AddHostedService<TelegramLongPollingService>();



string rootPath = builder.Configuration.GetValue<string>("FolderIndexing:RootPath");
string outputPath = builder.Configuration.GetValue<string>("FolderIndexing:OutputPath");
builder.Services.AddSingleton(new PassportFolderIndexerService(rootPath, outputPath));
builder.Services.AddSingleton(new PassportFolderSearchService(outputPath));
builder.Services.AddSingleton<EwsService>();
//builder.Services.AddSingleton<IConfiguration>(builder.Configuration); ;


builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(1000);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//Логирование
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
    option.ExpireTimeSpan = TimeSpan.FromMinutes(720);
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
