namespace Dashboard
{
    using Microsoft.AspNetCore.Authorization;

    public class DashboardAdminRequirement : IAuthorizationRequirement
    {
        public DashboardAdminRequirement(string dashboardAdmin)
        {
            this.AdminName = dashboardAdmin;
        }

        public string AdminName { get; }
    }
}