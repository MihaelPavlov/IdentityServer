using BFF.Application.Application.Interfaces;
using IdentityModel;
using System.Security.Claims;

namespace BFF.Application.Application;

public class UserContextFactory : IUserContextFactory
{
    readonly IHttpContextAccessor _httpContextAccessor;
    readonly IUserContext _defaultUserContext;

    public UserContextFactory(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _defaultUserContext = new UserContext(null);
    }

    public IUserContext CreateUserContext()
    {
        if (_httpContextAccessor.HttpContext == null)
            return _defaultUserContext;

        var user = _httpContextAccessor.HttpContext.User;

        var userIdparsed = int.TryParse(user.FindFirstValue(JwtClaimTypes.Subject), out int userId);


        return new UserContext(userId: userIdparsed ? userId : null);
    }
}

