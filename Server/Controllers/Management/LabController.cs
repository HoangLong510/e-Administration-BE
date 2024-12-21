using Microsoft.AspNetCore.Mvc;
using Server.DTOs.Lab;
using Server.Models;
using Server.Repositories;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/management/[controller]")]
    public class LabController : ControllerBase
    {
        private readonly ILabRepository _labRepository;

        public LabController(ILabRepository labRepository)
        {
            _labRepository = labRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LabDto>>> GetAllLabs(string? searchQuery = null, string? statusFilter = null)
        {
            try
            {
                var labs = await _labRepository.GetAllLabsAsync(searchQuery, statusFilter);
                var labDtos = labs.Select(lab => new LabDto
                {
                    Id = lab.Id,
                    Name = lab.Name,
                    Status = lab.Status
                });
                return Ok(new { Success = true, Data = labDtos });
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(500, new { Success = false, Message = "Failed to get labs: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LabDto>> GetLabById(int id)
        {
            var lab = await _labRepository.GetLabByIdAsync(id);
            if (lab == null)
            {
                return NotFound(new { Success = false, Message = "Lab not found" }); // Trả về NotFound với Success = false
            }
            var labDto = new LabDto
            {
                Id = lab.Id,
                Name = lab.Name,
                Status = lab.Status
            };
            return Ok(new { Success = true, Data = labDto }); // Bao bọc kết quả trong object với Success = true
        }

        [HttpPost]
        public async Task<ActionResult> CreateLab(CreateLabDto labDto)
        {
            var lab = new Lab
            {
                Name = labDto.Name,
                Status = labDto.Status
            };
            var result = await _labRepository.CreateLabAsync(lab);

            if (result == null)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Create Lab fail!"
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Create Lab successfully!"
            }); // Bao bọc kết quả trong object với Success = true
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLab(int id, UpdateLabDto labDto)
        {
            var lab = new Lab
            {
                Name = labDto.Name,
                Status = labDto.Status
            };
            var updatedLab = await _labRepository.UpdateLabAsync(id, lab);
            if (updatedLab == null)
            {
                return NotFound(new { Success = false, Message = "Lab not found" }); // Trả về NotFound với Success = false
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLab(int id)
        {
            var lab = await _labRepository.GetLabByIdAsync(id);
            if (lab == null)
            {
                return NotFound(new { Success = false, Message = "Lab not found" }); // Trả về NotFound với Success = false
            }

            var result = await _labRepository.DeleteLabAsync(id);
            if (!result)
            {
                return NotFound(new { Success = false, Message = "Lab not found" }); // Trả về NotFound với Success = false
            }

            // Trả về LabDto chứa thông tin lab vừa xóa
            var deletedLabDto = new LabDto
            {
                Id = lab.Id,
                Name = lab.Name,
                Status = lab.Status
            };
            return Ok(new { Success = true, Data = deletedLabDto }); // Bao bọc kết quả trong object với Success = true
        }
    }
}