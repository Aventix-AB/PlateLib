using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace API.Auth;

/// <summary>
/// Development-only authentication scheme that accepts a static API key
/// from the Authorization header ("Bearer {DevApiKey}").
/// Only active when <c>Auth:AllowDevKey</c> is <c>true</c>.
/// Never enable in production.
/// </summary>
public class DevApiKeyAuthHandler(
    IOptionsMonitor<DevApiKeyAuthOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<DevApiKeyAuthOptions>(options, logger, encoder)
{
    public const string SchemeName = "DevApiKey";
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var token = Request.Headers.Authorization.FirstOrDefault()?.Replace("Bearer ", "");

        if (string.IsNullOrEmpty(token) || token != Options.ApiKey)
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new[] { new Claim(ClaimTypes.Name, "dev-maintainer"), new Claim("role", "maintainer") };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public class DevApiKeyAuthOptions : AuthenticationSchemeOptions
{
    public string ApiKey { get; set; } = string.Empty;
}
