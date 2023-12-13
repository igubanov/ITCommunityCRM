using ITCommunityCRM.Data;
using ITCommunityCRM.Data.Models;
using ITCommunityCRM.Models.Configuration;
using ITCommunityCRM.Services;
using ITCommunityCRM.Services.Bot;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using ITCommunityCRM.Services.Telegram;
using ITCommunityCRM.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ITCommunityCRMDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ITCommunityCRMDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection(nameof(AppSettings)));
builder.Services.Configure<TelegramWidgetSettings>(builder.Configuration.GetSection(nameof(TelegramWidgetSettings)));
builder.Services.Configure<EmailNotificationSettings>(builder.Configuration.GetSection(nameof(EmailNotificationSettings)));

builder.Services.AddHttpClient("telegram_bot_client")
                .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
                {
                    AppSettings? botConfig = sp.GetService<IOptions<AppSettings>>()?.Value;
                    TelegramBotClientOptions options = new(botConfig.TelegramToken);
                    return new TelegramBotClient(options, httpClient);
                });


builder.Services.AddTransient<EmailNotification>();
builder.Services.AddTransient<NotificationService>();
builder.Services.AddTransient<TelegramNotification>();
builder.Services.AddTransient<TemplateServise>();

builder.Services.AddScoped<UpdateHandler>();
builder.Services.AddScoped<ReceiverService>();
builder.Services.AddHostedService<PollingService>();
builder.Services.AddTransient<TelegramIncomingMessageService>();

var app = builder.Build();
UpdateDatabase<ITCommunityCRMDbContext>(app);

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseMigrationsEndPoint();
//}
//else
//{
//    app.UseExceptionHandler("/Home/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}

app.UseIncomingTelegramMessage();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();



static void UpdateDatabase<T>(IApplicationBuilder app)
    where T : DbContext
{
    using (var scope = app.ApplicationServices.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<T>().Database;
        db.SetCommandTimeout(160);
        db.Migrate();
    }
}