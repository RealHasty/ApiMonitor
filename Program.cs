using ApiMonitor.Data;
using ApiMonitor.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------
// Services
// -------------------------------------------------------
builder.Services.AddControllersWithViews();

// EF Core → SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// HttpClient — used by ApiConnectionService (no separate service file needed)
builder.Services.AddHttpClient();

// App services
builder.Services.AddScoped<YamlLoaderService>();
builder.Services.AddScoped<ApiConnectionService>();
builder.Services.AddScoped<ApiSyncService>();

// -------------------------------------------------------
var app = builder.Build();
// -------------------------------------------------------

// Auto-migrate and sync YAML on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();   // applies any pending EF migrations

    var sync = scope.ServiceProvider.GetRequiredService<ApiSyncService>();
    await sync.SyncAllAsync();  // reads APIData/ and tests all connections
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
