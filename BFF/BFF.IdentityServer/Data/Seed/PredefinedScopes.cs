using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace BFF.IdentityServer.Data.Seed;

internal static class PredefinedScopes
{
    internal static List<ApiScope> ApiScopes =>
        new()
        {
            new ApiScope(IdentityServerConstants.LocalApi.ScopeName),
            new ApiScope("main_api", "MainApi"),
        };
}
