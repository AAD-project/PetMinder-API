using System.Security.Claims;
using Api.DTOs;
using Api.Models;
using AutoMapper; // Make sure to import AutoMapper
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;

        public UserController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IMapper mapper
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper; // Inject AutoMapper
        }

        // GET api/user/me
        [HttpGet("me")]
        public async Task<ActionResult<UserResponseDto>> GetCurrentUserAsync()
        {
            // Get the authenticated user's ID from the token (NameIdentifier claim)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Use AutoMapper to map the User model to UserResponseDto
            var userDto = _mapper.Map<UserResponseDto>(user);

            return Ok(userDto);
        }

        // PATCH api/user/me
        [Authorize]
        [HttpPatch("me")]
        public async Task<IActionResult> UpdateCurrentUserAsync(
            [FromBody] UserCreateRequestDto CreateUserDto
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the authenticated user's ID from the token (NameIdentifier claim)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if userId exists (this should not happen unless something went wrong with the token)
            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            // Find the current user by ID
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Map the updated fields from the DTO to the user model
            // This assumes _mapper is configured to handle null/unchanged properties
            _mapper.Map(CreateUserDto, user);

            // Update the user information using UserManager
            var result = await _userManager.UpdateAsync(user);

            // Handle potential errors during the update
            if (!result.Succeeded)
            {
                // Return a list of errors if the update failed
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            // Optionally, return the updated user (or just return NoContent if not needed)
            return Ok(user); // Alternatively, return NoContent();
        }

        // DELETE api/user/me
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteCurrentUserAsync()
        {
            // Get the authenticated user's ID from the token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }

        // ============ Admin Endpoints ============

        // GET api/user/{id}
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUserByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Use AutoMapper to map the User model to UserResponseDto
            var userDto = _mapper.Map<UserResponseDto>(user);

            return Ok(userDto);
        }

        // GET api/user
        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public ActionResult<IEnumerable<UserResponseDto>> GetAllUsers()
        {
            var users = _userManager.Users.ToList();

            // Use AutoMapper to map the list of Users to list of UserResponseDto
            var userDtos = _mapper.Map<List<UserResponseDto>>(users);

            return Ok(userDtos);
        }

        // POST api/user
        [Authorize(Roles = "Admin")]
        [HttpPost("admin")]
        public async Task<ActionResult<UserResponseDto>> CreateUserAsync(
            [FromBody] UserCreateRequestDto newUserDto
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newUser = _mapper.Map<User>(newUserDto); // Map the UserCreateRequestDto to User

            var result = await _userManager.CreateAsync(newUser);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var createdUserDto = _mapper.Map<UserResponseDto>(newUser);

            return CreatedAtAction(
                nameof(GetUserByIdAsync),
                new { id = createdUserDto.Id },
                createdUserDto
            );
        }

        // PATCH api/user/{id}
        [Authorize(Roles = "Admin")]
        [HttpPatch("admin/{id}")]
        public async Task<IActionResult> UpdateUserByIdAsync(
            string id,
            [FromBody] UserUpdateRequestDto updatedUserDto
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Map the updated fields from the DTO to the user model
            _mapper.Map(updatedUserDto, user);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }

        // DELETE api/user/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("admin/{id}")]
        public async Task<IActionResult> DeleteUserByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }
    }
}
