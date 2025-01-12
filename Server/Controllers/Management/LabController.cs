using Microsoft.AspNetCore.Mvc;
using Server.DTOs.Lab;
using Server.Models;
using Server.Repositories;
using Server.Services;


namespace Server.Controllers
{
    [ApiController]
    [Route("api/management/[controller]")]
    public class LabController : ControllerBase
    {
        private readonly ILabRepository _labRepository;
        private readonly TokenService tokenService;
        public LabController(ILabRepository labRepository)
        {
            _labRepository = labRepository;
            this.tokenService = tokenService;

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
        public async Task<ActionResult> CreateLab(LabDto labDto)
        {
            var errors = new Dictionary<string, string>();

            // Kiểm tra tên phòng thí nghiệm
            if (string.IsNullOrWhiteSpace(labDto.Name))
            {
                errors["name"] = "Name is required";
            }
            else
            {
                var checkName = await _labRepository.CheckNameExists(labDto.Name);
                if (checkName)
                {
                    errors["name"] = "Name already exists";
                }
            }

            if (errors.Count > 0)
            {
                var errorMessage = "Invalid lab information! Please check the errors of the fields again:\n";
                foreach (var error in errors)
                {
                    errorMessage += $"{error.Key}: {error.Value}\n";
                }
                return BadRequest(new
                {
                    Success = false,
                    Errors = errors,
                    Message = errorMessage
                });
            }

            var lab = new Lab
            {
                Name = labDto.Name,
                Status = labDto.Status
            };

            await _labRepository.CreateLabAsync(lab);
            return Ok(new
            {
                Success = true,
                Message = "Lab created successfully!",
                Lab = labDto
            });
        }
        


        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateLab(int id, LabDto labDto)
        {
            var errors = new Dictionary<string, string>();

            // Kiểm tra tên phòng thí nghiệm
            if (string.IsNullOrWhiteSpace(labDto.Name))
            {
                errors["name"] = "Name is required";
            }
            else
            {
                var isUniqueName = await _labRepository.IsLabNameUnique(labDto.Name, id);
                if (!isUniqueName)
                {
                    errors["name"] = "Name already exists";
                }
            }

            if (errors.Count > 0)
            {
                var errorMessage = "Invalid lab information! Please check the errors of the fields again:\n";
                foreach (var error in errors)
                {
                    errorMessage += $"{error.Key}: {error.Value}\n";
                }
                return BadRequest(new
                {
                    Success = false,
                    Errors = errors,
                    Message = errorMessage
                });
            }

            var lab = await _labRepository.GetLabByIdAsync(id);
            if (lab == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Lab not found"
                });
            }

            lab.Name = labDto.Name;
            lab.Status = labDto.Status;

            await _labRepository.UpdateLabAsync(id, lab);
            return Ok(new
            {
                Success = true,
                Message = "Lab updated successfully!",
                Lab = labDto
            });
        }


        [HttpGet("disable-lab/{id}")]
        public async Task<ActionResult> DeleteLab(int id)
        {
            
            var (result, message ) = await _labRepository.DisableLabAsync(id);
            if (!result)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = message
                });
            }

            return Ok(new
            {
                Success = true,
                Message = message
            });
        }

        [HttpGet("check-name-exists")]
        public async Task<ActionResult> CheckNameExists(string name)
        {
            var exists = await _labRepository.CheckNameExists(name);
            return Ok(new
            {
                Success = true,
                Exists = exists
            });
        }

        [HttpGet("is-lab-name-unique")]
        public async Task<ActionResult> IsLabNameUnique(string name, int labId)
        {
            var isUnique = await _labRepository.IsLabNameUnique(name, labId);
            return Ok(new
            {
                Success = true,
                IsUnique = isUnique
            });
        }

        [HttpGet("status-summary")]
        public async Task<ActionResult> GetLabsStatusSummary()
        {
            try
            {
                var (activeCount, inactiveCount) = await _labRepository.GetLabsStatusSummaryAsync();
                return Ok(new
                {
                    Success = true,
                    Data = new
                    {
                        ActiveCount = activeCount,
                        InactiveCount = inactiveCount,
                        TotalCount = activeCount + inactiveCount
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Failed to get labs status summary: " + ex.Message });
            }
        }

    }
}