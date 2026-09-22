using Microsoft.EntityFrameworkCore;
using OrgMgmt;
using OrgMgmt.Models;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<OrgDbContext>(options => options.UseSqlite(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ));
var app = builder.Build();
using (var scope = app.Services.CreateScope())
    {
    var dbContext = scope.ServiceProvider.GetRequiredService<OrgDbContext>();
    dbContext.Database.EnsureCreated();
    }
if (!app.Environment.IsDevelopment())
    {
    app.UseExceptionHandler("/Home/Error");
    }
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();