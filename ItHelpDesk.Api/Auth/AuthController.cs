using ItHelpDesk.Api.Data.Dtos.Auth;
using ItHelpDesk.Api.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ItHelpDesk.Api.Auth
{
    [ApiController]
    [Route("/api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ItHelpDeskUser> _userManager;
        private readonly JwtTokenService _jwtTokenService;
        private readonly SessionService _sessionService;

        public AuthController(UserManager<ItHelpDeskUser> userManager, JwtTokenService jwtTokenService, SessionService sessionService)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _sessionService = sessionService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AccessTokenResponseDto>> Login(LoginRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                return Problem(detail: "Wrong email or password.", statusCode: 401);
            }

            if (!user.IsActive)
            {
                return Problem(detail: "Account is deactivated.", statusCode: 403);
            }

            var roles = await _userManager.GetRolesAsync(user);

            var sessionId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;
            var expiresAt = now.Add(_jwtTokenService.RefreshTokenLifeTime());

            var accessToken = _jwtTokenService.CreateAccessToken(user.Id, user.UserName!, roles);
            var refreshToken = _jwtTokenService.CreateRefreshToken(sessionId, user.Id, expiresAt);

            var device = Request.Headers.UserAgent.ToString();
            if (device.Length > 256)
            {
                device = device[..256];
            }

            await _sessionService.CreateSessionAsync(sessionId, user.Id, refreshToken, now, expiresAt, device);

            if (user.ForceRelogin)
            {
                user.ForceRelogin = false;
                await _userManager.UpdateAsync(user);
            }

            SetRefreshTokenCookie(refreshToken, expiresAt);

            return Ok(new AccessTokenResponseDto(accessToken));
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AccessTokenResponseDto>> Refresh()
        {
            if (!Request.Cookies.TryGetValue("RefreshToken", out var refreshToken))
            {
                return InvalidToken();
            }

            if (!_jwtTokenService.TryParseRefreshToken(refreshToken, out var claims))
            {
                return InvalidToken();
            }

            var userId = claims!.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (userId == null || !Guid.TryParse(claims!.FindFirstValue("SessionId"), out var sessionId))
            {
                return InvalidToken();
            }

            if (!await _sessionService.IsSessionValidAsync(sessionId, refreshToken))
            {
                return InvalidToken();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return InvalidToken();
            }

            if (user.ForceRelogin || !user.IsActive)
            {
                await _sessionService.InvalidateSessionAsync(sessionId);
                return InvalidToken();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var expiresAt = DateTimeOffset.UtcNow.Add(_jwtTokenService.RefreshTokenLifeTime());

            var accessToken = _jwtTokenService.CreateAccessToken(user.Id, user.UserName!, roles);
            refreshToken = _jwtTokenService.CreateRefreshToken(sessionId, user.Id, expiresAt);

            await _sessionService.ExtendSessionAsync(sessionId, refreshToken, expiresAt);

            SetRefreshTokenCookie(refreshToken, expiresAt);

            return Ok(new AccessTokenResponseDto(accessToken));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if (Request.Cookies.TryGetValue("RefreshToken", out var refreshToken) &&
                _jwtTokenService.TryParseRefreshToken(refreshToken, out var claims) &&
                Guid.TryParse(claims!.FindFirstValue("SessionId"), out var sessionId))
            {
                await _sessionService.InvalidateSessionAsync(sessionId);
            }

            Response.Cookies.Delete("RefreshToken", new CookieOptions { Path = "/api/auth", Secure = Request.IsHttps });
            return NoContent();
        }

        void SetRefreshTokenCookie(string refreshToken, DateTimeOffset expiresAt)
        {
            Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Expires = expiresAt,
                Path = "/api/auth",
                Secure = Request.IsHttps
            });
        }

        ActionResult InvalidToken() { return Problem(detail: "Invalid refresh token.", statusCode: 401); }
    }
}