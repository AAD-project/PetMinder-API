using System.Security.Claims;
using System.Threading.Tasks;
using Api.DTOs;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PetController : ControllerBase
    {
        private readonly IPetService _petService;

        public PetController(IPetService petService)
        {
            _petService = petService;
        }

        // Helper method to get the authenticated user's ID
        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // ==================== Regular User Endpoints ====================

        // GET: api/pet/user
        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<PetResponseDto>>> GetUserPetsAsync()
        {
            try
            {
                var userId = GetUserId();
                var pets = await _petService.GetUserPetsAsync(userId);
                return Ok(pets);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while fetching your pets.");
            }
        }

        // GET: api/pet/user/{id}
        [HttpGet("user/{id}")]
        public async Task<ActionResult<PetResponseDto>> GetUserPetByIdAsync(string id)
        {
            try
            {
                var pet = await _petService.GetPetByIdAsync(id);

                if (pet == null)
                {
                    return NotFound("Pet not found.");
                }

                if (pet.OwnerId == GetUserId())
                {
                    // Owners can access their pets
                    return Ok(pet);
                }
                else
                {
                    return Forbid("You do not have permission to access this pet.");
                }
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while fetching the pet details.");
            }
        }

        // POST: api/pet/user
        [HttpPost("user")]
        public async Task<ActionResult<PetResponseDto>> AddUserPetAsync(
            [FromBody] PetCreateRequestDto newPet
        )
        {
            if (newPet == null)
            {
                return BadRequest("Pet data is invalid.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetUserId();
                newPet.OwnerId = userId; // Set the OwnerId to the authenticated user
                var addedPet = await _petService.AddPetAsync(newPet, userId);

                // Ensure the response is in the correct format for the test
                var response = new { message = "Pet successfully created", pet = addedPet };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while adding the pet: {ex.Message}");
            }
        }

        // PATCH: api/pet/user/{id}
        [HttpPatch("user/{id}")]
        public async Task<IActionResult> UpdateUserPetAsync(
            string id,
            [FromBody] PetCreateRequestDto updatedPet
        )
        {
            if (updatedPet == null)
            {
                return BadRequest("Pet data is invalid.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var pet = await _petService.GetPetByIdAsync(id);

                if (pet == null)
                {
                    return NotFound("Pet not found.");
                }

                if (pet.OwnerId == GetUserId())
                {
                    // Owners can update their pets
                    await _petService.UpdatePetAsync(id, updatedPet);
                    return NoContent();
                }
                else
                {
                    return Forbid("You do not have permission to update this pet.");
                }
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while updating the pet.");
            }
        }

        // DELETE: api/pet/user/{id}
        [HttpDelete("user/{id}")]
        public async Task<IActionResult> DeleteUserPetAsync(string id)
        {
            try
            {
                var pet = await _petService.GetPetByIdAsync(id);

                if (pet == null)
                {
                    return NotFound("Pet not found.");
                }

                if (pet.OwnerId == GetUserId())
                {
                    // Owners can delete their pets
                    await _petService.DeletePetAsync(id);
                    return NoContent();
                }
                else
                {
                    return Forbid("You do not have permission to delete this pet.");
                }
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the pet.");
            }
        }

        // ==================== Admin Endpoints ====================

        // GET: api/pet/admin
        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public async Task<ActionResult<IEnumerable<PetResponseDto>>> GetAllPetsAsync()
        {
            try
            {
                var pets = await _petService.GetAllPetsAsync();
                return Ok(pets);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while fetching all pets.");
            }
        }

        // GET: api/pet/admin/{id}
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/{id}")]
        public async Task<ActionResult<PetResponseDto>> GetPetByIdForAdminAsync(string id)
        {
            try
            {
                var pet = await _petService.GetPetByIdAsync(id);

                if (pet == null)
                {
                    return NotFound("Pet not found.");
                }

                // Admins can access any pet
                return Ok(pet);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while fetching the pet details.");
            }
        }

        // POST: api/pet/admin
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/{id}")]
        public async Task<ActionResult<PetResponseDto>> AddPetForUserAsync(
            string id,
            [FromBody] PetCreateRequestDto newPet
        )
        {
            if (newPet == null)
            {
                return BadRequest("Pet data is invalid.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Admins can assign the OwnerId to any user
                if (string.IsNullOrEmpty(newPet.OwnerId))
                {
                    return BadRequest(
                        "OwnerId must be provided for creating a pet for a specific user."
                    );
                }

                var addedPet = await _petService.AddPetAsync(newPet, id);
                return CreatedAtAction(
                    nameof(GetPetByIdForAdminAsync),
                    new { id = addedPet.Id },
                    addedPet
                );
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while adding the pet.");
            }
        }

        // PATCH: api/pet/admin/{id}
        [Authorize(Roles = "Admin")]
        [HttpPatch("admin/{id}")]
        public async Task<IActionResult> UpdatePetForAdminAsync(
            string id,
            [FromBody] PetCreateRequestDto updatedPet
        )
        {
            if (updatedPet == null)
            {
                return BadRequest("Pet data is invalid.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var pet = await _petService.GetPetByIdAsync(id);

                if (pet == null)
                {
                    return NotFound("Pet not found.");
                }

                // Admins can update any pet
                await _petService.UpdatePetAsync(id, updatedPet);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while updating the pet.");
            }
        }

        // DELETE: api/pet/admin/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("admin/{id}")]
        public async Task<IActionResult> DeletePetForAdminAsync(string id)
        {
            try
            {
                var pet = await _petService.GetPetByIdAsync(id);

                if (pet == null)
                {
                    return NotFound("Pet not found.");
                }

                // Admins can delete any pet
                await _petService.DeletePetAsync(id);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the pet.");
            }
        }
    }
}
