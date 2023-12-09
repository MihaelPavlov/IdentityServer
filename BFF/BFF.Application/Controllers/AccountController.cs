using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace BFF.Application.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController : Controller
{
    /// <summary>
    /// Handling authorization code flow in authentication processes.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="codeVerifier"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The resulting model includes details such as identity token, access token, refresh token, and user information.</returns>
    [HttpGet]
    [AllowAnonymous]
    [Route("token")]
    public async Task<IActionResult> AuthorizationCodeToken([FromHeader(Name = "code")] string code, [FromHeader(Name = "code_verifier")] string codeVerifier, CancellationToken cancellationToken)
    {
        // discover endpoints from metadata
        var client = new HttpClient();
        var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5001", cancellationToken);
        if (disco.IsError)
            throw new BadHttpRequestException("Discovery document not found!");

        // request token
        var tokenResponse = await client.RequestAuthorizationCodeTokenAsync(new AuthorizationCodeTokenRequest
        {
            GrantType = "authorization_code",
            Address = disco.TokenEndpoint,
            ClientId = "WebClient_ID",
            Code = code,
            CodeVerifier = codeVerifier,
            RedirectUri = "http://localhost:4200/sign-in-callback"
        }, cancellationToken);

        if (tokenResponse.IsError)
            throw new BadHttpRequestException(tokenResponse.Error + " :: " + tokenResponse.ErrorDescription);

        ArgumentNullException.ThrowIfNull(tokenResponse.AccessToken);

        // request user info
        var apiClient = new HttpClient();
        apiClient.SetBearerToken(tokenResponse.AccessToken);

        var response = await apiClient.GetAsync(disco.UserInfoEndpoint, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new BadHttpRequestException(response.ToString());

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var parsed = JsonDocument.Parse(content);

        var userResponse = parsed.Deserialize<object>();
        /*
          "userResponse": {
            "sub": "1",
            "preferred_username": "admin@trinv.com",
            "name": "admin@trinv.com"
          }
         */

        return Ok(new
        {
            TokenResponse = new
            {
                tokenResponse.IdentityToken,
                tokenResponse.AccessToken,
                tokenResponse.RefreshToken,
                tokenResponse.ExpiresIn
            },
            UserResponse = userResponse
        });
    }

    /// <summary>
    /// This method handles the authorization process for a web client using the Authorization Code flow.
    /// It obtains an access token, retrieves user information, sets up claims for authentication, signs in the user, and returns the parsed user information as an object.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="codeVerifier"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns the parsed user information as an object</returns>
    [HttpGet]
    [AllowAnonymous]
    [Route("authorize")]
    public async Task<IActionResult> AuthorizeByCode([FromHeader(Name = "code")] string code, [FromHeader(Name = "code_verifier")] string codeVerifier, CancellationToken cancellationToken)
    {
        // discover endpoints from metadata
        var client = new HttpClient();
        var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5001");
        if (disco.IsError)
            throw new BadHttpRequestException("Discovery document not found!");

        // request token
        var tokenResponse = await client.RequestAuthorizationCodeTokenAsync(new AuthorizationCodeTokenRequest
        {
            Address = disco.TokenEndpoint,
            ClientId = "WebClient_ID",
            Code = code,
            CodeVerifier = codeVerifier,
            RedirectUri = "http://localhost:4200/sign-in-callback"
        });

        if (tokenResponse.IsError)
            throw new BadHttpRequestException(tokenResponse.Error + " :: " + tokenResponse.ErrorDescription);

        ArgumentNullException.ThrowIfNull(tokenResponse.AccessToken);

        // request user info
        var apiClient = new HttpClient();
        apiClient.SetBearerToken(tokenResponse.AccessToken);

        var response = await apiClient.GetAsync(disco.UserInfoEndpoint);
        if (!response.IsSuccessStatusCode)
            throw new BadHttpRequestException(response.ToString());

        HttpContext.Session.SetString("AccessToken", tokenResponse.AccessToken);
        //HttpContext.Session.SetString("RefreshToken", tokenResponse.RefreshToken);

        var content = await response.Content.ReadAsStringAsync();
        var parsed = JsonDocument.Parse(content);
        var sub = string.Empty;
        var email = string.Empty;
        foreach (var el in parsed.RootElement.EnumerateObject())
        {
            var name = el.Name;
            var value = el.Value.GetString() ?? "";
            HttpContext.Session.SetString(name, value);

            if (name == "sub" && !string.IsNullOrWhiteSpace(value))
                sub = value;

            if (name == "name" && !string.IsNullOrWhiteSpace(value))
                email = value;
        }

        // sign in
        var claims = new List<Claim>
        {
            new Claim("sub", sub),
            new Claim("scope","main_api"),
            new Claim("email",email)
        };

        var claimsPrincipal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        var authProperties = new AuthenticationProperties
        {
            AllowRefresh = true,
            // Refreshing the authentication session should be allowed.

            //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
            // The time at which the authentication ticket expires. A 
            // value set here overrides the ExpireTimeSpan option of 
            // CookieAuthenticationOptions set with AddCookie.

            //IsPersistent = true,
            // Whether the authentication session is persisted across 
            // multiple requests. When used with cookies, controls
            // whether the cookie's lifetime is absolute (matching the
            // lifetime of the authentication ticket) or session-based.

            //IssuedUtc = <DateTimeOffset>,
            // The time at which the authentication ticket was issued.

            //RedirectUri = <string>
            // The full path or absolute URI to be used as an http 
            // redirect response value.
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            claimsPrincipal,
            authProperties);

        var result = parsed.Deserialize<object>();

        ArgumentNullException.ThrowIfNull(result);

        return Ok(result);
    }


    [HttpGet]
    [Authorize(Policy = "ApiScope")]
    [Route("user-info")]
    public IActionResult UserInformation()
    {
        var subFromClaim = User.FindFirstValue("sub");
        var email = User.FindFirstValue("email");

        // For future use of the session
        var subFromSession = HttpContext.Session.GetString("sub");

        return Ok(new { subFromClaim, email });
    }

    [HttpGet]
    [Authorize(Policy = "ApiScope")]
    [Route("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        // Clear the existing external cookie
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var client = new HttpClient();
        var result = await client.GetAsync("https://localhost:5001/account/logout", cancellationToken);

        if (!result.IsSuccessStatusCode)
            throw new BadHttpRequestException(result.ToString());

        return Ok();
    }
}
