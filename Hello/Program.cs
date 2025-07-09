using Hello.Contract.Interfaces;
using Hello.Contract.Services;
using Hello.Domain.Seed;
using Hello.Domain.Users;
using Hello.Entityframework.Core.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PermissionBasedAuthorizationIntDotNet5.Contants;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<HelloDbContext>(options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("Hello"))
);



builder.Services.AddIdentity<User,IdentityRole>()
    .AddEntityFrameworkStores<HelloDbContext>()
    .AddDefaultTokenProviders();





var jwtSettings = builder.Configuration.GetSection("JWT");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}
)
    .AddJwtBearer(Options =>
    {
        Options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]))

        };
    });

builder.Services.AddScoped<IJwt, Jwt>();




builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
});





builder.Services.AddAuthorization(options =>
{
    var allPermissions = Permissions.GenerateAllPermissions();

    foreach (var permission in allPermissions)
    {
        options.AddPolicy(permission, policy =>
            policy.RequireClaim("Permission", permission));
    }
});




var app = builder.Build();



// Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<User>>();

    await DefaultRoles.SeedAsync(roleManager);
    await DefaultUsers.SeedBasicUserAsync(userManager);
    await DefaultUsers.SeedSuperAdminAsync(userManager, roleManager);
}






// Jwt 
app.Use(async (context, next) =>
{
    var token = context.Request.Cookies["jwt"];

    if (!string.IsNullOrEmpty(token))
    {
        context.Request.Headers["Authorization"] = "Bearer " + token;
    }

    await next();
});








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

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=User}/{action=Login}/{id?}")
//    .WithStaticAssets();


app.Run();
