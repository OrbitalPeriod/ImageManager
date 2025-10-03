using ImageManager.Data;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Extensions;

public class DatabaseSetup
{
    /// <summary>
    /// Configures database migrations.
    /// </summary>
    /// <param name="scope"></param>
    public static void ConfigureIdentityDatabase(IServiceScope scope)
    {

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();

    }
}