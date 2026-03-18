using Microsoft.EntityFrameworkCore;
using GymAppFresh.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using GymAppFresh.Hubs;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options=>{
    options.LoginPath="/Home/Login";
    options.LogoutPath="/Home/Logout";
    options.AccessDeniedPath="/Home/Denied";
    options.SlidingExpiration=true;
});
builder.Services.AddAuthorization(options=>
{
    options.AddPolicy("Admin", policy =>
        policy.RequireClaim("IsAdmin", "true"));
    options.AddPolicy("Staff", policy =>
        policy.RequireClaim("IsStaff", "true"));
});

builder.Services.AddSession();
builder.Services.AddSignalR();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
else
{
    app.UseDeveloperExceptionPage();
}

using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();

    if (!ctx.Memberships.Any())
    {
        var memberships = new List<Membership>
        {
            new Membership
            {
                Name = "Classic",
                Price = 500.00m ,
                PacketType = PacketType.Classic,
                CreatedAt = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddYears(1),
                IsActive = true
            },
            new Membership
            {
                Name = "Gold",
                Price = 800.00m,
                PacketType = PacketType.Gold,
                CreatedAt = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddYears(1),
                IsActive = true
            },
            new Membership
            {
                Name = "Platinum",
                Price = 1200.00m,
                PacketType = PacketType.Platinum,
                CreatedAt = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddYears(1),
                IsActive = true
            }
        };

        ctx.Memberships.AddRange(memberships);
        ctx.SaveChanges();
    }

}

app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<ChatHub>("/hubs/chat");

app.Run();
