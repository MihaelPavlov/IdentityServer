using BFF.Application.Application.Interfaces;

namespace BFF.Application.Application;

public class UserContext : IUserContext
{
    readonly int? _userId;

    public UserContext(int? userId)
    {
        _userId = userId;
    }

    public int UserId
    {
        get
        {
            if (_userId.HasValue)
                return _userId.Value;
            throw new ArgumentException("Can not find current user.");
        }
    }

    public bool IsAuthenticated => _userId.HasValue;
}

