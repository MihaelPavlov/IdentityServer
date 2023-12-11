using Duende.IdentityServer.Models;

namespace ISD.IdentityServer.Models.Errors;

public class ErrorViewModel
{
    public ErrorViewModel()
    {
    }

    public ErrorViewModel(string error)
    {
        Error = new ErrorMessage { Error = error };
    }

    public ErrorMessage Error { get; set; } = null!;
}
