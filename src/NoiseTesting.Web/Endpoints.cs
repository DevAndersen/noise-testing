using NoiseTesting.Web.Services;

namespace NoiseTesting.Web;

public static class Endpoints
{
    extension(WebApplication app)
    {
        public WebApplication MapEndpoints()
        {
            app.MapGet("/api/tile", (int posX, int posY, int seed, TileService tileService) =>
            {
                byte[] imageBytes = tileService.GetImageBytes(posX, posY, seed);
                return Results.File(imageBytes, "image/png");
            });

            return app;
        }
    }
}
