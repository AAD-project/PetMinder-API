using Api.DTOs;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET api/user/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUserByIdAsync(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound(); // Return 404 if user not found
            }

            return Ok(user); // Return 200 with the UserResponseDto data
        }

        // DELETE api/user/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserByIdAsync(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound(); // Return 404 if user does not exist
            }

            await _userService.DeleteUserAsync(id);
            return NoContent(); // Return 204 No Content on successful deletion
        }

        // PATCH api/user/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateUserInfoAsync(
            string id,
            [FromBody] UserCreateRequestDto updatedUserDto
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Return 400 if model validation fails
            }

            var existingUser = await _userService.GetUserByIdAsync(id);

            if (existingUser == null)
            {
                return NotFound(); // Return 404 if user not found
            }

            await _userService.UpdateUserAsync(id, updatedUserDto);
            return NoContent(); // Return 204 on successful update
        }

        // POST api/user
        [HttpPost]
        public async Task<ActionResult<UserResponseDto>> CreateUserAsync(
            [FromBody] UserCreateRequestDto newUserDto
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Return 400 if model validation fails
            }

            var createdUser = await _userService.AddUserAsync(newUserDto);
            return CreatedAtAction(
                nameof(GetUserByIdAsync),
                new { id = createdUser.Id },
                createdUser
            ); // Return 201 with location of the created user
        }

        // GET api/user/{id}/pets
        [HttpGet("{id}/pets")]
        public async Task<ActionResult<IEnumerable<PetResponseDto>>> GetAllPetsFromUserAsync(
            string id
        )
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound(); // Return 404 if user not found
            }

            var pets = await _userService.GetAllPetsFromUserAsync(id);
            return Ok(pets); // Return 200 with the list of PetResponseDto
        }
    }
}
