using Microsoft.AspNetCore.Routing;

namespace AppLearningEnglish.Routing;

public static class AppRouteExtensions
{
    public static WebApplication MapAppRoutes(this WebApplication app)
    {
        foreach (var spec in AppRoutes.All())
        {
            app.MapControllerRoute(spec.Name, spec.Pattern, spec.ToDefaults())
                .WithStaticAssets();
        }

        return app;
    }

    private static RouteValueDictionary ToDefaults(this AppRoutes.AppRoute spec)
    {
        var defaults = new RouteValueDictionary();
        if (spec.Area is not null)
        {
            defaults["area"] = spec.Area;
        }

        if (spec.Controller is not null)
        {
            defaults["controller"] = spec.Controller;
        }

        if (spec.Action is not null)
        {
            defaults["action"] = spec.Action;
        }

        return defaults;
    }
}
