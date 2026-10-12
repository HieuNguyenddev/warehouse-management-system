using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Common.Dtos;
using UserManagementAPI.Modules.Auth.DTOs;
using UserManagementAPI.Modules.Auth.Services;

namespace UserManagementAPI.Modules.Auth.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _auth.LoginAsync(request, cancellationToken));
        }
        catch (AppException exception)
        {
            return StatusCode(exception.StatusCode, new { message = exception.Message });
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var raw = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(raw, out var userId))
        {
            return Unauthorized(new { message = "Phiên đăng nhập không hợp lệ." });
        }

        var user = await _auth.CurrentAsync(userId, cancellationToken);
        return user is null ? Unauthorized(new { message = "Phiên đăng nhập không hợp lệ." }) : Ok(user);
    }
}
