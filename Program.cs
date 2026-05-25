using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/karlhotel-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute("chat",       "api/chat",   new { controller = "Home", action = "Chat" });
app.MapControllerRoute("rezervace",  "rezervace",  new { controller = "Home", action = "Rezervace" });
app.MapControllerRoute("restaurace", "restaurace", new { controller = "Home", action = "Restaurace" });
app.MapControllerRoute("vylety",     "vylety",     new { controller = "Home", action = "Vylety" });
app.MapControllerRoute("galerie",    "galerie",    new { controller = "Home", action = "Galerie" });
app.MapControllerRoute("default",   "{controller=Home}/{action=Index}/{id?}").WithStaticAssets();

app.Run();
