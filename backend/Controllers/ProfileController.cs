using FinanzasApi.DTOs;
using FinanzasApi.Helpers;
using FinanzasApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasApi.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IUserService _users;
    public ProfileController(IUserService users) => _users = users;

    [HttpGet]
    public async Task<ActionResult<UserResponseDto>> Get() =>
        Ok(await _users.GetByIdAsync(User.GetUserId()));

    [HttpPut]
    public async Task<ActionResult<UserResponseDto>> Update(UpdateProfileDto dto) =>
        Ok(await _users.UpdateProfileAsync(User.GetUserId(), dto, User.IsAdmin()));

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        await _users.ChangePasswordAsync(User.GetUserId(), dto);
        return NoContent();
    }
}
