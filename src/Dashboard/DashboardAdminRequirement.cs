using Microsoft.AspNetCore.Authorization;

namespace Dashboard
{
    public class DashboardAdminRequirement : IAuthorizationRequirement
    {
        public DashboardAdminRequirement(string dashboardAdmin)
        {
            this.AdminName = dashboardAdmin;
        }

        public string AdminName { get; }
    }
}