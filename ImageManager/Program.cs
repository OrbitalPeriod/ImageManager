#region Usings

using System.Threading.Channels;
using ImageManager.Data;
using ImageManager.Extensions;
using ImageManager.Repositories;
using ImageManager.Services;
using ImageManager.Services.PlatformTokens;
using ImageManager.Services.Query;
using ImageManager.Services.Tags;
using ImageManager.Services.UserInfo;
using ImageManager.Workers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using User = ImageManager.Data.Models.User;
#endregion

DotNetEnv.Env.Load("../.env");
DotNetEnv.Env.Load("../.secrets.env");

var builder = WebApplication.CreateBuilder(args);


#region Database & Identity Configuration
var connectionString = builder.Configuration["SQL_CONNECTION_STRING"]
                       ?? throw new Exception("SQL_CONNECTION_STRING is required");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});
#endregion

#region Channels

builder.Services.AddSingleton(Channel.CreateBounded<PlatformSyncRequest>(new BoundedChannelOptions(1000)
{
    FullMode = BoundedChannelFullMode.Wait
}));

#endregion


#region Repository Registrations
builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();
builder.Services.AddScoped<IDownloadedImageRepository, DownloadedImageRepository>();
builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<IPlatformTokenRepository, PlatformTokenRepository>();
builder.Services.AddScoped<IShareTokenRepository, ShareTokenRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IUserOwnedImageRepository, UserOwnedImageRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPlatformSyncLogRepository, PlatformSyncLogRepository>();

builder.Services.AddScoped<IDeleteImageService, DeleteImageService>();
builder.Services.AddScoped<IImageDetailService, ImageDetailService>();
builder.Services.AddScoped<IImageQueryService, ImageQueryService>();
builder.Services.AddScoped<IUploadImageService, UploadImageService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ICharacterQueryService, CharacterQueryService>();
builder.Services.AddScoped<IInitialUserCreationService, InitialUserCreationService>();

builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IUserInfoService, UserInfoService>();
#endregion

#region External API Clients
builder.Services.AddSingleton<IPixivService>(_ => new PixivService(
    builder.Configuration["PIXIV_TOKEN"] ?? throw new Exception("PIXIV_TOKEN is required")));

builder.Services.AddScoped<ITaggerService, TaggerService>();

builder.Services.AddSingleton<IFileService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<FileService>>();
    return new FileService(config, logger);
});
#endregion

#region Hosted Services
builder.Services.AddScoped<IPixivImageImportManager, PixivImportManager>();
builder.Services.AddScoped<IImageImportService, ImageImportService>();
builder.Services.AddScoped<IPlatformTokenService, PlatformTokenService>();

// Runs the RemoteSync loop in the background
builder.Services.AddHostedService<RemoteSyncService>();
builder.Services.AddHostedService<RemoteSyncQueuingService>();
#endregion

#region Middleware & API Setup
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin();
    });
});

builder.Services.AddLogging();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Anime Image Manager",
        Description = "Backend to manage and access images",
        Version = "v1"
    });
});

builder.Services.AddControllers();
#endregion

var app = builder.Build();

#region Development Environment Configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Anime Image Manager");
    });


    // Seed the database on startup when in development mode
    using var scope = app.Services.CreateScope();
    DatabaseSetup.ConfigureIdentityDatabase(scope);

    var initialUserService = scope.ServiceProvider.GetRequiredService<IInitialUserCreationService>();
    await initialUserService.AddDefaultUser();
}
#endregion

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseCors();

app.Run();
