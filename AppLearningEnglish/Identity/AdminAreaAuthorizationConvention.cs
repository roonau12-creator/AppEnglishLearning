using AppLearningEnglish.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace AppLearningEnglish.Identity
{
    public class AdminAreaAuthorizationConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            var area = controller.Attributes
                .OfType<AreaAttribute>()
                .FirstOrDefault()
                ?.RouteValue;

            if (!string.Equals(area, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireRole(AppRoles.Admin)
                .Build();

            controller.Filters.Add(new AuthorizeFilter(policy));
        }
    }
}
