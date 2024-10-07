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

        // GET: api/pet
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PetResponseDto>>> GetAllPetsAsync()
        {
            try
            {
                var pets = await _petService.GetAllPetsAsync();
                return Ok(pets); // Return 200 with the list of PetResponseDto
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // GET: api/pet/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PetResponseDto>> GetPetByIdAsync(string id)
        {
            try
            {
                var pet = await _petService.GetPetByIdAsync(id);

                if (pet == null)
                {
                    return NotFound();
                }

                return Ok(pet);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // POST: api/pet
        [HttpPost]
        public async Task<ActionResult<PetResponseDto>> AddPetAsync(
            [FromBody] PetCreateRequestDto newPet
        )
        {
            if (newPet == null)
            {
                return BadRequest("Pet data is invalid."); // Return 400 if no pet is provided
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Return 400 if model validation fails
            }

            try
            {
                var addedPet = await _petService.AddPetAsync(newPet);
                return CreatedAtAction(nameof(GetPetByIdAsync), new { id = addedPet.Id }, addedPet); // Return 201 with the created PetResponseDto
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // PATCH: api/pet/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatePetAsync(
            string id,
            [FromBody] PetCreateRequestDto updatedPet
        )
        {
            if (updatedPet == null)
            {
                return BadRequest("Pet data is invalid."); // Return 400 if no pet is provided
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Return 400 if model validation fails
            }

            try
            {
                var existingPet = await _petService.GetPetByIdAsync(id);

                if (existingPet == null)
                {
                    return NotFound(); // Return 404 if pet not found
                }

                await _petService.UpdatePetAsync(id, updatedPet);
                return NoContent(); // Return 204 on successful update
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // DELETE: api/pet/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePetAsync(string id)
        {
            try
            {
                var pet = await _petService.GetPetByIdAsync(id);

                if (pet == null)
                {
                    return NotFound(); // Return 404 if pet not found
                }

                await _petService.DeletePetAsync(id);
                return NoContent(); // Return 204 on successful deletion
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
