using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Common.Dtos;
using UserManagementAPI.Modules.Users.DTOs;
using UserManagementAPI.Modules.Users.Services;

namespace UserManagementAPI.Modules.Users.Controllers;

[Authorize]
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _users;

    public UsersController(IUserService users)
    {
        _users = users;
    }

    [HttpGet]
    public Task<IActionResult> Search([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        return Execute(async () => Ok(await _users.SearchAsync(search, page, pageSize, cancellationToken)));
    }

    [HttpGet("{id:guid}/password")]
    public Task<IActionResult> GetPassword(Guid id, CancellationToken cancellationToken)
    {
        if (!IsAdmin)
        {
            return Task.FromResult<IActionResult>(StatusCode(403, new { message = "Chỉ vai trò admin mới xem được mật khẩu." }));
        }

        return Execute(async () =>
        {
            var password = await _users.GetPasswordAsync(id, cancellationToken);
            return password is null ? NotFound(new { message = "Không tìm thấy người dùng." }) : Ok(password);
        });
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Execute(async () =>
        {
            var user = await _users.GetByIdAsync(id, cancellationToken);
            return user is null ? NotFound(new { message = "Không tìm thấy người dùng." }) : Ok(user);
        });
    }

    [HttpPost]
    public Task<IActionResult> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (!IsAdmin)
        {
            return Task.FromResult<IActionResult>(StatusCode(403, new { message = "Chỉ vai trò admin mới đặt được mật khẩu." }));
        }

        return Execute(async () =>
        {
            var user = await _users.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        });
    }

    [HttpPut("{id:guid}")]
    public Task<IActionResult> Update(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.Password) && !IsAdmin)
        {
            return Task.FromResult<IActionResult>(StatusCode(403, new { message = "Chỉ vai trò admin mới đặt lại được mật khẩu." }));
        }

        return Execute(async () =>
        {
            var user = await _users.UpdateAsync(id, request, cancellationToken);
            return user is null ? NotFound(new { message = "Không tìm thấy người dùng." }) : Ok(user);
        });
    }

    [HttpDelete("{id:guid}")]
    public Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return Execute(async () =>
        {
            var deleted = await _users.DeleteAsync(id, cancellationToken);
            return deleted ? NoContent() : NotFound(new { message = "Không tìm thấy người dùng." });
        });
    }

    private bool IsAdmin
    {
        get
        {
            var role = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");
            return string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase);
        }
    }

    private async Task<IActionResult> Execute(Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (AppException exception)
        {
            return StatusCode(exception.StatusCode, new { message = exception.Message });
        }
    }
}
