using GestionRH.Data;
using GestionRH.Models;
using GestionRH.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<Utilisateur, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultUI()
.AddDefaultTokenProviders();
builder.Services.AddControllersWithViews();

// Enregistrer les services
builder.Services.AddScoped<PdfService>();
builder.Services.AddScoped<GestionRH.Services.NotificationService>();

// Ajouter SignalR pour les notifications en temps réel
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

var supportedCultures = new[] { "fr-MA" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Mapper le hub SignalR
app.MapHub<GestionRH.Hubs.NotificationHub>("/notificationHub");

// FIX DB CONSTRAINTS ON STARTUP
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try 
    {
        // Allow NULL for Poste and Salaire to support Responsable type
        context.Database.ExecuteSqlRaw("ALTER TABLE AspNetUsers MODIFY COLUMN Poste VARCHAR(100) NULL;");
        context.Database.ExecuteSqlRaw("ALTER TABLE AspNetUsers MODIFY COLUMN Salaire DECIMAL(18,2) NULL;");
    }
    catch (Exception ex)
    {
        // Ignore if already done or fails
        Console.WriteLine("DB Fix Warning: " + ex.Message);
    }
}

app.Run();
