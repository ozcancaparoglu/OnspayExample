using Infrastructure.Database;

namespace Web.Api.Extensions;

public static class SeedDataExtensions
{
    public static async Task SeedData(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Add your data seeding logic here
        await dbContext.SaveChangesAsync();
    }
}