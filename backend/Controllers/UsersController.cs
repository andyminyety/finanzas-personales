using FinanzasApi.DTOs;
using FinanzasApi.Helpers;
using FinanzasApi.Models;
using FinanzasApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasApi.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = Roles.Admin)]
public class UsersController : ControllerBase
{
    private readonly IUserService _users;
    public UsersController(IUserService users) => _users = users;

    [HttpGet]
    public async Task<ActionResult<List<UserResponseDto>>> GetAll() => Ok(await _users.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseDto>> Get(string id) => Ok(await _users.GetByIdAsync(id));

    [HttpPost]
    public async Task<ActionResult<UserResponseDto>> Create(CreateUserDto dto) =>
        Ok(await _users.CreateAsync(dto));

    [HttpPut("{id}")]
    public async Task<ActionResult<UserResponseDto>> Update(string id, UpdateUserDto dto) =>
        Ok(await _users.UpdateAsync(id, dto));

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<UserResponseDto>> SetStatus(string id, StatusDto dto) =>
        Ok(await _users.SetStatusAsync(id, dto.IsActive, User.GetUserId()));
}
