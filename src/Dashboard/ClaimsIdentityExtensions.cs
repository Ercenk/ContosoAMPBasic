namespace Dashboard
{
    using System;
    using System.Security.Claims;
    using System.Security.Principal;

    public static class ClaimsIdentityExtensions
    {
        public static string GetUserEmail(this IIdentity principal)
        {
            if (!(principal is ClaimsIdentity identity)) throw new ApplicationException("Not of ClaimsIdentity type");

            return string.IsNullOrEmpty(identity.Name)
                       ? identity.FindFirst("preferred_username")?.Value
                       : identity.Name;
        }
    }
}