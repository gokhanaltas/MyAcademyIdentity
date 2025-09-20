using EmailApp.Context;
using EmailApp.Entities;
using EmailApp.Validations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options =>

{
    var connectionString = builder.Configuration.GetConnectionString
    ("SqlConnection");

    options.UseSqlServer(connectionString);
});

builder.Services.AddIdentity<AppUser,AppRole>(config =>
{
    config.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddErrorDescriber<CustomErrorDescriber>();



builder.Services.AddControllersWithViews();

builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.Cookie.Name = "IdentityCookie";
    opt.LoginPath = "/Login/Index";
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
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
