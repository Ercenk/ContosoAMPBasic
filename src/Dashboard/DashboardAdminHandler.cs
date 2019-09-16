namespace Dashboard
{
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;

    public class DashboardAdminHandler : AuthorizationHandler<DashboardAdminRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            DashboardAdminRequirement requirement)
        {
            if (context.User == null) return Task.CompletedTask;

            if (context.User.Identity.GetUserEmail() == requirement.AdminName)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}