using Microsoft.EntityFrameworkCore;
using EventManagementSystem;

partial class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container
        builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
        builder.Services.AddSession();

        // Add Database Context
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

        var app = builder.Build();

        // Configure the HTTP request pipeline
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseSession();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Event}/{action=Index}/{id?}");

        app.Run();
    }
}
