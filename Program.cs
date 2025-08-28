using ABCRetailer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ABCRetailer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Load configuration from appsettings.json
            builder.Services.Configure<AzureStorageOptions>(
                builder.Configuration.GetSection("AzureStorage"));

            // Register Azure Storage service
            builder.Services.AddScoped<IAzureStorageService, AzureStorageService>();

            // Add MVC services
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Initialize Azure resources
            using (var scope = app.Services.CreateScope())
            {
                var storage = scope.ServiceProvider.GetRequiredService<IAzureStorageService>();
                await storage.InitializeAsync();
            }

            // Configure middleware
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            // Map default route
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
