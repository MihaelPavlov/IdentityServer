using Duende.IdentityServer.Models;

namespace ISD.IdentityServer.Data.Seed;

internal static class PredefinedIdentities
{
    internal static List<IdentityResource> IdentityResources =>
        new()
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            //new IdentityResource()
            //{
            //    Name = "verification",
            //    UserClaims = new List<string>
            //    {
            //        JwtClaimTypes.Email,
            //        JwtClaimTypes.EmailVerified
            //    }
            //}
        };
}
