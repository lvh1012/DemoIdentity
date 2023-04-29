using DemoIdentity.Data;
using DemoIdentity.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

#region identitysrever4
// add identitysrever4
var migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;
builder.Services.AddIdentityServer(options =>
{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;

    // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
    options.EmitStaticAudienceClaim = true;
})
    .AddConfigurationStore(options =>
    {
        options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
            sql => sql.MigrationsAssembly(migrationsAssembly));
    })
    .AddOperationalStore(options =>
    {
        options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
            sql => sql.MigrationsAssembly(migrationsAssembly));
    }).AddAspNetIdentity<IdentityUser>();

/* migrations identity server
 * dotnet ef migrations add InitialIdentityServerPersistedGrantDbMigration -c PersistedGrantDbContext -o Data/Migrations/IdentityServer/PersistedGrantDb
 * dotnet ef migrations add InitialIdentityServerConfigurationDbMigration -c ConfigurationDbContext -o Data/Migrations/IdentityServer/ConfigurationDb
 *
 *  dotnet ef database update --context PersistedGrantDbContext
 *  dotnet ef database update --context ConfigurationDbContext
 */
#endregion


builder.Services.AddTransient<IEmailSender, EmailSender>();

/*
 *Vì ClaimsPrincipal có thể re lại
 *khi add role hoặc đổi mật khẩu các thứ
 *nên cần phải check xem session hay cookie có hợp lệ k
 *
 *tracking changes made to the user profile
 */
// Force Identity's security stamp to be validated every minute.
builder.Services.Configure<SecurityStampValidatorOptions>(o =>
                   o.ValidationInterval = TimeSpan.FromMinutes(1));

#region Authentication Scheme
/* Authentication Scheme là 1 cái authentication handlers
 * có 1 số loại như cookie, jwt
 *
 * Phải đăng tên cho từng loại scheme
 * Nếu có nhiều scheme phải chỉ rõ loại nào là loại mặc định
 */
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; // scheme mặc định
//    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, // tên scheme
//    options => builder.Configuration.Bind("JwtSettings", options))
//.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, // tên scheme
//    options => builder.Configuration.Bind("CookieSettings", options))
builder.Services.AddAuthentication()
.AddOpenIdConnect(options =>
{
    // thông tin oidc client
    // Khi cần đăng nhập sẽ redirect đến server OIDC
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme; // nơi lưu token
    options.Authority = "http://127.0.0.1:5000"; // server OIDC
    options.RequireHttpsMetadata = false;
    options.ClientId = "mvc-client";
    options.ClientSecret = "secret";
    options.ResponseType = "code";
    options.UsePkce = true;
    options.Scope.Add("profile");
    options.SaveTokens = true; // Có lưu thông tin id token và refresh token ko?

    // chỉ có 1 số standard claim được lưu tự động nên phải mapping bằng tay
    options.GetClaimsFromUserInfoEndpoint = true;
    options.ClaimActions.MapUniqueJsonKey("preferred_username", "preferred_username");
    options.ClaimActions.MapUniqueJsonKey("gender", "gender");

    // hoặc 1 kiểu mapping khác
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "email",
        RoleClaimType = "roles"
    };
});
#endregion

builder.Services.AddControllersWithViews();

#region config identity

// must be called after calling AddIdentity or AddDefaultIdentity.
builder.Services.Configure<IdentityOptions>(options =>
{
    // config lock tài khoản nếu đăng nhập sai
    // Default Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // config password
    // Default Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // yêu cầu xác nhận tài khoản email/phonenumber khi đăng nhập
    // Default SignIn settings.
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;

    // config user
    // Default User settings.
    options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});

// config cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.Cookie.Name = "YourAppCookieName";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/Identity/Account/Login";
    // ReturnUrlParameter requires
    //using Microsoft.AspNetCore.Authentication.Cookies;
    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
    options.SlidingExpiration = true; // exprire time tự động tăng khi gần hết hạn
});

builder.Services.Configure<PasswordHasherOptions>(option =>
{
    // số lần hash passsword
    // làm chậm brushforce
    option.IterationCount = 12000;
});

#endregion config identity

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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseIdentityServer();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();