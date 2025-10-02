using DotNetEnv;
using DotNetEnv.Configuration;
using ImageManager;
using ImageManager.Data;
using ImageManager.Extensions;
using ImageManager.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PixivCS.Api;
using User = ImageManager.Data.Models.User;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);



#region Authentication Setup

// Gets the SQL Connection string from the enviroment variables
var connectionString = builder.Configuration["SQL_CONNECTION_STRING"] ?? throw new Exception("SQL_CONNECTION_STRING is required");
// Adds this DB to the DB context
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

// Set up identity service
builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.User.RequireUniqueEmail = true;
    }).AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure cookie Authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    //options.LoginPath = "/Account/Login";
    //options.LogoutPath = "/Account/Logout";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

#endregion

builder.Services.AddSingleton<IPixivService>(sp => new PixivService(builder.Configuration["PIXIV_TOKEN"] ?? throw new Exception("PIXIV_TOKEN is required")));
builder.Services.AddSingleton<ITaggerService>(sp => new TaggerService(builder.Configuration["ANIMETAGGER_URL"] ?? throw new Exception("ANIMETAGGER_URL is required")));
builder.Services.AddSingleton<IFileService>(sp =>
    new FileService(builder.Configuration["FILE_DIRECTORY"] ?? throw new Exception("FILE_DIRECTORY is required")));
builder.Services.AddScoped<IDatabaseService, DatabaseService>();
builder.Services.AddScoped<IPixivImageImportManager, PixivImportManager>();
builder.Services.AddScoped<IImageImportService, ImageImportService>();

builder.Services.AddHostedService<PixivSyncService>();

#region API Setup

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(configure =>
    {
        configure.AllowAnyHeader();
        configure.AllowAnyMethod();
        configure.AllowAnyOrigin();
    });
});
builder.Services.AddLogging();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "www", Description = "wwwww", Version = "v1" });
});

builder.Services.AddControllers();

#endregion

var app = builder.Build();

#region Development Setup
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UUAI API V1");
    });

    using var scope = app.Services.CreateScope();
    DatabaseSetup.ConfigureIdentityDatabase(scope);
}
#endregion

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseExceptionHandler("/error");

app.UseCors();
app.Run();