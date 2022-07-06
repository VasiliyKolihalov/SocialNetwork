using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SocialNetwork.Constants;
using SocialNetwork.Middlewares;
using SocialNetwork.Models;
using SocialNetwork.Models.Roles;
using SocialNetwork.Models.Users;
using SocialNetwork.Repositories;
using SocialNetwork.Services;

var builder = WebApplication.CreateBuilder(args);

#region Configuration

builder.Configuration.AddJsonFile("jwtauthsettings.json");
builder.Configuration.AddJsonFile("admindata.json");
builder.Configuration.AddJsonFile("companysettings.json");

#endregion

#region Services

string connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddScoped<IApplicationContext>(_ => new ApplicationContext(connection));

var jwtAuthOptions = builder.Configuration.GetSection("JwtAuthenticationData");
builder.Services.Configure<JwtAuthenticationOptions>(jwtAuthOptions);

JwtAuthenticationOptions jwtOptions = new JwtAuthenticationOptions()
{
    Issuer = jwtAuthOptions["Issuer"],
    Audience = jwtAuthOptions["Audience"],
    Secret = jwtAuthOptions["Secret"],
    TokenMinuteLifetime = Convert.ToInt32(jwtAuthOptions["TokenMinuteLifetime"])
};

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,

            IssuerSigningKey = jwtOptions.GetSymmetricSecurityKey(),
            ValidateIssuerSigningKey = true,
        };
    });

builder.Services.AddControllers();
builder.Services.AddTransient<UsersService>();
builder.Services.AddTransient<AccountService>();
builder.Services.AddTransient<CorrespondencesService>();
builder.Services.AddTransient<RolesService>();
builder.Services.AddTransient<CommunitiesService>();
builder.Services.AddScoped<EmailService>();

builder.Services.AddAutoMapper(typeof(Program));

#endregion

var app = builder.Build();

#region Middlewares

app.UseHttpLogging();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ErrorHandlerMiddleware>();

app.MapControllers();

#endregion

InitializeRolesAndAdministrator(app.Services.CreateScope().ServiceProvider.GetRequiredService<IApplicationContext>(), app.Configuration);

app.Run();

static void InitializeRolesAndAdministrator(IApplicationContext applicationContext, IConfiguration configuration)
{
    var adminData = configuration.GetSection("AdminData");

    User admin;
    try
    {
        admin = applicationContext.Users.GetFromEmail(adminData["Email"]);
    }
    catch
    {
        admin = new User()
        {
            FirstName = adminData["FirstName"], SecondName = adminData["SecondName"], Email = adminData["Email"],
            PasswordHash = PasswordHasher.HashPassword(adminData["Password"])
        };
        applicationContext.Users.Add(admin);
        applicationContext.Images.SetDefaultValueForUserAvatar(admin.Id);
    }

    Role adminRole;
    try
    {
        adminRole = applicationContext.Roles.GetFromName(RolesNameConstants.AdminRole);
    }
    catch
    {
        adminRole = new Role() {Name = RolesNameConstants.AdminRole};
        applicationContext.Roles.Add(adminRole);
    }


    IEnumerable<Role> roles = applicationContext.Roles.GetFromUserId(admin.Id);

    if (!roles.Any(x => x.Name == RolesNameConstants.AdminRole))
        applicationContext.Roles.AddRoleToUser(admin.Id, adminRole.Id);
}