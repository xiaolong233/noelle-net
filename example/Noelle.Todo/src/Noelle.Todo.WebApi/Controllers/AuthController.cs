using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NoelleNet.Security.Claims;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using System.Text.Json;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Noelle.Todo.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly IOpenIddictAuthorizationManager _authorizationManager;
        private readonly IOpenIddictTokenManager _tokenManager;
        private readonly UserManager<IdentityUser<long>> _userManager;
        private readonly SignInManager<IdentityUser<long>> _signInManager;

        public AuthController(IOpenIddictApplicationManager applicationManager, IOpenIddictAuthorizationManager authorizationManager, UserManager<IdentityUser<long>> userManager, SignInManager<IdentityUser<long>> signInManager, IOpenIddictTokenManager tokenManager)
        {
            _applicationManager = applicationManager;
            _authorizationManager = authorizationManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenManager = tokenManager;
        }

        [HttpPost]
        [Route("token")]
        [Consumes("application/x-www-form-urlencoded")]
        [Produces("application/json")]
        public async Task<IActionResult> Token()
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            if (request == null)
            {
                return BadRequest(new OpenIddictResponse { Error = Errors.InvalidRequest, ErrorDescription = "无法检索 OpenID Connect 请求。" });
            }

            if (request.IsClientCredentialsGrantType())
            {
                var application = await _applicationManager.FindByClientIdAsync(request.ClientId ?? string.Empty);
                if (application == null)
                    return BadRequest(new OpenIddictResponse { Error = Errors.InvalidClient, ErrorDescription = "未找到该应用" });

                var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType, Claims.Name, Claims.Role);

                identity.SetClaim(Claims.Subject, await _applicationManager.GetClientIdAsync(application));
                identity.SetClaim(NoelleClaimTypes.ClientId, await _applicationManager.GetClientIdAsync(application));
                identity.SetClaim(Claims.Name, await _applicationManager.GetDisplayNameAsync(application));

                identity.SetScopes(request.GetScopes());
                identity.SetDestinations(GetDestinations);

                return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }
            else if (request.IsPasswordGrantType())
            {
                var user = await _userManager.FindByNameAsync(request.Username ?? string.Empty);
                if (user == null)
                    return BadRequest(new OpenIddictResponse() { Error = Errors.InvalidRequest, ErrorDescription = "用户名或密码错误" });

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password ?? string.Empty, true);
                if (!result.Succeeded)
                    return BadRequest(new OpenIddictResponse() { Error = Errors.InvalidRequest, ErrorDescription = "用户名或密码错误" });

                var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType, Claims.Name, Claims.Role);
                identity.SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user));
                identity.SetClaim(NoelleClaimTypes.ClientId, request.ClientId);
                identity.SetClaim(NoelleClaimTypes.UserId, await _userManager.GetUserIdAsync(user));
                identity.SetClaim(Claims.Email, await _userManager.GetEmailAsync(user));
                identity.SetClaim(Claims.Name, await _userManager.GetUserNameAsync(user));
                identity.SetClaim(Claims.PreferredUsername, await _userManager.GetUserNameAsync(user));
                identity.SetClaim(NoelleClaimTypes.Roles, JsonSerializer.Serialize(await _userManager.GetRolesAsync(user)));

                identity.SetScopes(request.GetScopes());
                identity.SetDestinations(GetDestinations);

                return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }
            else if (request.IsRefreshTokenGrantType())
            {
                var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                var user = await _userManager.FindByIdAsync(result.Principal?.GetClaim(Claims.Subject) ?? string.Empty);
                if (user == null)
                    return BadRequest(new OpenIddictResponse() { Error = Errors.ExpiredToken, ErrorDescription = "刷新令牌已失效" });

                if (!await _signInManager.CanSignInAsync(user))
                    return BadRequest(new OpenIddictResponse() { Error = Errors.AccessDenied, ErrorDescription = "该用户禁止登录" });

                var identity = new ClaimsIdentity(
                result.Principal?.Claims,
                    TokenValidationParameters.DefaultAuthenticationType,
                Claims.Name,
                Claims.Role);

                identity.SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user));
                identity.SetClaim(Claims.Email, await _userManager.GetEmailAsync(user));
                identity.SetClaim(Claims.Name, await _userManager.GetUserNameAsync(user));
                identity.SetClaim(Claims.PreferredUsername, await _userManager.GetUserNameAsync(user));
                identity.SetClaims(Claims.Role, [.. (await _userManager.GetRolesAsync(user))]);

                identity.SetScopes(request.GetScopes());
                identity.SetDestinations(GetDestinations);

                return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }
            else if (request.GrantType == "quick_login")
            {
                var identity = new ClaimsIdentity(
                    TokenValidationParameters.DefaultAuthenticationType,
                    Claims.Name,
                    Claims.Role);

                identity.SetClaim(Claims.Subject, Guid.NewGuid().ToString());

                identity.SetScopes(request.GetScopes());
                identity.SetDestinations(GetDestinations);

                return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            return BadRequest(new OpenIddictResponse { Error = Errors.InvalidGrant, ErrorDescription = "无效的GrantType" });
        }

        [HttpPost]
        [Route("logout")]
        [Consumes("application/x-www-form-urlencoded")]
        [Produces("application/json")]
        public async Task<IActionResult> Logout()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var token = await _tokenManager.FindByReferenceIdAsync(accessToken ?? string.Empty);
            if (token != null) await _tokenManager.TryRevokeAsync(token);

            await _signInManager.SignOutAsync();
            return SignOut(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        private static IEnumerable<string> GetDestinations(Claim claim)
        {
            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.

            switch (claim.Type)
            {
                case Claims.Name or Claims.PreferredUsername:
                    yield return Destinations.AccessToken;

                    if (claim.Subject.HasScope(Scopes.Profile))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Email:
                    yield return Destinations.AccessToken;

                    if (claim.Subject.HasScope(Scopes.Email))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Role:
                    yield return Destinations.AccessToken;

                    if (claim.Subject.HasScope(Scopes.Roles))
                        yield return Destinations.IdentityToken;

                    yield break;

                // Never include the security stamp in the access and identity tokens, as it's a secret value.
                case "AspNet.Identity.SecurityStamp": yield break;

                default:
                    yield return Destinations.AccessToken;
                    yield break;
            }
        }
    }
}
