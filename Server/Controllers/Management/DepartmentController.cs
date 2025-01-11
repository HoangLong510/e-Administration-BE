using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Repositories;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/management/[controller]")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentRepository _departmentRepository;

        public DepartmentsController(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Department>>> GetAllDepartments(string? searchQuery = null, string? sortBy = null)
        {
            try
            {
                var departments = await _departmentRepository.GetAllDepartmentsAsync(searchQuery, sortBy);
                return Ok(new { Success = true, Data = departments });
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(500, new { Success = false, Message = "Failed to get departments: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Department>> GetDepartmentById(int id)
        {
            try
            {
                var department = await _departmentRepository.GetDepartmentByIdAsync(id);
                if (department == null)
                {
                    return NotFound(new { Success = false, Message = "Department not found" });
                }
                return Ok(new { Success = true, Data = department });
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(500, new { Success = false, Message = "Failed to get department: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Department>> CreateDepartment(Department department)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdDepartment = await _departmentRepository.CreateDepartmentAsync(department);
                return CreatedAtAction(nameof(GetDepartmentById), new { id = createdDepartment.Id }, new { Success = true, Data = createdDepartment });
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(500, new { Success = false, Message = "Failed to create department: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, Department department)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedDepartment = await _departmentRepository.UpdateDepartmentAsync(id, department);
                if (updatedDepartment == null)
                {
                    return NotFound(new { Success = false, Message = "Department not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(500, new { Success = false, Message = "Failed to update department: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            try
            {
                var department = await _departmentRepository.GetDepartmentByIdAsync(id);
                if (department == null)
                {
                    return NotFound(new { Success = false, Message = "Department not found" });
                }

                var result = await _departmentRepository.DeleteDepartmentAsync(id);
                if (!result)
                {
                    return NotFound(new { Success = false, Message = "Department not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(500, new { Success = false, Message = "Failed to delete department: " + ex.Message });
            }
        }

        [HttpGet("get-all-departments-no-pagination")]
        public async Task<ActionResult> GetAllDepartmentsNoPagination()
        {
            var result = await _departmentRepository.GetAllDepartmentsNoPagination();
            return Ok(new
            {
                Success = true,
                Data = result
            });
        }
    }
}