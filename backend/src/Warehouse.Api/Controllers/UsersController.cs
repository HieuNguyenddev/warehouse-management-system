using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.Common;
using Warehouse.Application.Users;

namespace Warehouse.Api.Controllers;

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
    public Task<IActionResult> Create(UpsertUserRequest request, CancellationToken cancellationToken)
    {
        return Execute(async () =>
        {
            var user = await _users.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        });
    }

    [HttpPut("{id:guid}")]
    public Task<IActionResult> Update(Guid id, UpsertUserRequest request, CancellationToken cancellationToken)
    {
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
