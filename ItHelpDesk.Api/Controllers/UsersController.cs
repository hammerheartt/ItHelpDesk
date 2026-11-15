using ItHelpDesk.Api.Auth;
using ItHelpDesk.Api.Data.Dtos.Users;
using ItHelpDesk.Api.Data.Entities;
using ItHelpDesk.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ItHelpDesk.Api.Controllers
{
    ///api/users GET ALL 200 (filters: role, isActive)
    ///api/users/{userId} GET 200
    ///api/users POST 201
    ///api/users/{userId}/role PATCH 200
    ///api/users/{userId}/active PATCH 200
    
    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = ItHelpDeskRoles.Admin)]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ItHelpDeskUser> _userManager;
        private readonly ITicketsRepository _ticketsRepository;
        private readonly string _emailDomain;

        public UsersController(UserManager<ItHelpDeskUser> userManager, ITicketsRepository ticketsRepository, IConfiguration configuration)
        {
            _userManager = userManager;
            _ticketsRepository = ticketsRepository;
            _emailDomain = configuration["Company:EmailDomain"]!.ToLowerInvariant();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAll([FromQuery] string? role, [FromQuery] bool? isActive)
        {
            if (role != null && !ItHelpDeskRoles.All.Contains(role))
                return Problem(detail: "Unknown role.", statusCode: 400);

            IEnumerable<ItHelpDeskUser> users = role != null
                ? await _userManager.GetUsersInRoleAsync(role)
                : await _userManager.Users.ToListAsync();

            if (isActive != null) { users = users.Where(u => u.IsActive == isActive); }

            var result = new List<UserDto>();
            foreach (var user in users.OrderBy(u => u.FullName))
            {
                result.Add(await ToDto(user));
            }
            return Ok(result);
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<UserDto>> Get(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            return Ok(await ToDto(user));
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> Create(CreateUserDto dto)
        {
            if (!ItHelpDeskRoles.All.Contains(dto.Role))
                return Problem(detail: "Unknown role.", statusCode: 400);

            var email = dto.Email.Trim().ToLowerInvariant();
            if (!email.EndsWith("@" + _emailDomain))
                return Problem(detail: $"Email must be a @{_emailDomain} address.", statusCode: 400);

            if (await _userManager.FindByEmailAsync(email) != null)
                return Problem(detail: "Email is already in use.", statusCode: 409);

            var user = new ItHelpDeskUser { UserName = email, Email = email, FullName = dto.FullName };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return ValidationProblem(ModelState);
            }

            await _userManager.AddToRoleAsync(user, dto.Role);

            return CreatedAtAction(nameof(Get), new { userId = user.Id }, await ToDto(user));
        }

        [HttpPatch("{userId}/role")]
        public async Task<ActionResult<UserDto>> ChangeRole(string userId, ChangeUserRoleDto dto)
        {
            if (!ItHelpDeskRoles.All.Contains(dto.Role))
                return Problem(detail: "Unknown role.", statusCode: 400);

            if (userId == CurrentUserId())
                return Problem(detail: "You cannot change your own role.", statusCode: 409);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Contains(dto.Role)) return Ok(await ToDto(user));

            if (currentRoles.Contains(ItHelpDeskRoles.Technician))
            {
                var open = await _ticketsRepository.CountOpenAssigned(user.Id);
                if (open > 0)
                    return Problem(detail: $"Reassign or close {open} ticket(s) first.", statusCode: 409);
            }

            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, dto.Role);

            user.ForceRelogin = true;
            await _userManager.UpdateAsync(user);

            return Ok(await ToDto(user));
        }

        [HttpPatch("{userId}/active")]
        public async Task<ActionResult<UserDto>> SetActive(string userId, SetUserActiveDto dto)
        {
            if (userId == CurrentUserId())
                return Problem(detail: "You cannot change your own active status.", statusCode: 409);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var activate = dto.IsActive!.Value;
            if (!activate && await _userManager.IsInRoleAsync(user, ItHelpDeskRoles.Technician))
            {
                var open = await _ticketsRepository.CountOpenAssigned(user.Id);
                if (open > 0)
                    return Problem(detail: $"Reassign or close {open} ticket(s) first.", statusCode: 409);
            }

            user.IsActive = activate;
            await _userManager.UpdateAsync(user);

            return Ok(await ToDto(user));
        }

        private async Task<UserDto> ToDto(ItHelpDeskUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return new UserDto(user.Id, user.FullName, user.Email!, roles.FirstOrDefault() ?? "", user.IsActive);
        }

        private string CurrentUserId() => User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;
    }
}
