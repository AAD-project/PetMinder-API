using System.Collections.Generic;
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

        // GET: api/pet
        [HttpGet]
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

        // GET: api/pet/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PetResponseDto>> GetPetByIdAsync(string id)
        {
            try
            {
                var pet = await _petService.GetPetByIdAsync(id);

                if (pet == null)
                {
                    return NotFound(); // Return 404 if the pet is not found
                }

                return Ok(pet);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while fetching the pet details.");
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
                return BadRequest("Pet data is invalid.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var addedPet = await _petService.AddPetAsync(newPet);
                return CreatedAtAction(nameof(GetPetByIdAsync), new { id = addedPet.Id }, addedPet);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while adding the pet.");
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
                return BadRequest("Pet data is invalid.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var existingPet = await _petService.GetPetByIdAsync(id);
                if (existingPet == null)
                {
                    return NotFound();
                }

                await _petService.UpdatePetAsync(id, updatedPet);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while updating the pet."); // Ensure 500 is returned on exception
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
                    return NotFound();
                }

                await _petService.DeletePetAsync(id);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the pet."); // Ensure 500 is returned on exception
            }
        }

        // POST: api/pet/{id}/healthdata
        [HttpPost("{id}/healthdata")]
        public async Task<ActionResult<HealthDataResponseDto>> AddHealthDataAsync(
            string id,
            [FromBody] HealthDataCreateRequestDto newHealthData
        )
        {
            if (newHealthData == null)
            {
                return BadRequest("Health data is invalid.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var healthData = await _petService.AddHealthDataAsync(id, newHealthData);
                return Ok(healthData);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Pet with Id {id} not found.");
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while adding the health data.");
            }
        }

        // DELETE: api/pet/{id}/healthdata/{healthDataId}
        [HttpDelete("{id}/healthdata/{healthDataId}")]
        public async Task<IActionResult> DeleteHealthDataAsync(string id, string healthDataId)
        {
            try
            {
                await _petService.DeleteHealthDataAsync(id, healthDataId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Health data or Pet with Id {id} not found.");
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the health data.");
            }
        }
    }
}
