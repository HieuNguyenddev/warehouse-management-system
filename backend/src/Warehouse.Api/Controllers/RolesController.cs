using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.Common;
using Warehouse.Application.Roles;

namespace Warehouse.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/roles")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roles;

    public RolesController(IRoleService roles)
    {
        _roles = roles;
    }

    [HttpGet]
    public Task<IActionResult> Search([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        return Execute(async () => Ok(await _roles.SearchAsync(search, page, pageSize, cancellationToken)));
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Execute(async () =>
        {
            var role = await _roles.GetByIdAsync(id, cancellationToken);
            return role is null ? NotFound(new { message = "Không tìm thấy vai trò." }) : Ok(role);
        });
    }

    [HttpPost]
    public Task<IActionResult> Create(UpsertRoleRequest request, CancellationToken cancellationToken)
    {
        return Execute(async () =>
        {
            var role = await _roles.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
        });
    }

    [HttpPut("{id:guid}")]
    public Task<IActionResult> Update(Guid id, UpsertRoleRequest request, CancellationToken cancellationToken)
    {
        return Execute(async () =>
        {
            var role = await _roles.UpdateAsync(id, request, cancellationToken);
            return role is null ? NotFound(new { message = "Không tìm thấy vai trò." }) : Ok(role);
        });
    }

    [HttpDelete("{id:guid}")]
    public Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return Execute(async () =>
        {
            var deleted = await _roles.DeleteAsync(id, cancellationToken);
            return deleted ? NoContent() : NotFound(new { message = "Không tìm thấy vai trò." });
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
