using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Repositories;
using System.Security.Claims;

namespace Server.Controllers
{
    [Route("api/management/[controller]")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly IClassRepository repo;

        public ClassController(IClassRepository repo)
        {
            this.repo = repo;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllClasses([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await repo.GetPagedClassesAsync(search, page, pageSize);

            return Ok(new
            {
                Success = true,
                Message = "Lấy danh sách lớp học thành công.",
                Data = result.Classes,
                Pagination = new
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalRecords = result.TotalCount,
                    TotalPages = (int)Math.Ceiling((double)result.TotalCount / pageSize)
                }
            });
        }


        [HttpGet("{id}")]
        public async Task<ActionResult> GetClassById(int id)
        {
            var cls = await repo.GetClassByIdAsync(id);
            if (cls == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Lớp học không tìm thấy."
                });
            }
            return Ok(new
            {
                Success = true,
                Message = "Lấy thông tin lớp học thành công.",
                Data = cls
            });
        }

        [HttpPost]
        public async Task<ActionResult> AddClass([FromBody] Class newClass)
        {
            if (newClass == null || string.IsNullOrEmpty(newClass.Name))
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Dữ liệu lớp học không hợp lệ."
                });
            }

            var exists = await repo.ClassNameExistsAsync(newClass.Name);
            if (exists)
            {
                return Conflict(new
                {
                    Success = false,
                    Message = $"Tên lớp học \"{newClass.Name}\" đã tồn tại."
                });
            }

            await repo.AddClassAsync(newClass);

            return CreatedAtAction(nameof(GetClassById), new { id = newClass.Id }, new
            {
                Success = true,
                Message = "Lớp học đã được thêm thành công.",
                Data = newClass
            });
        }


        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateClass(int id, [FromBody] Class updatedClass)
        {
            if (id != updatedClass.Id)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "ID không khớp với lớp học."
                });
            }

            var existingClass = await repo.GetClassByIdAsync(id);
            if (existingClass == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Lớp học không tìm thấy."
                });
            }

            await repo.UpdateClassAsync(updatedClass);

            return Ok(new
            {
                Success = true,
                Message = "Lớp học đã được cập nhật thành công.",
                Data = updatedClass
            });
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteClass(int id)
        {
            var existingClass = await repo.GetClassByIdAsync(id);
            if (existingClass == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Lớp học không tìm thấy."
                });
            }

            await repo.DeleteClassAsync(id);
            return Ok(new
            {
                Success = true,
                Message = "Lớp học đã được xóa thành công."
            });
        }

        [HttpGet("check-name")]
        public async Task<ActionResult> CheckClassNameExists([FromQuery] string name)
        {
            var exists = await repo.ClassNameExistsAsync(name);
            if (exists)
            {
                return Ok(new
                {
                    Success = true,
                    Exists = true,
                    Message = $"Class name \"{name}\" already exists."
                });
            }

            return Ok(new
            {
                Success = true,
                Exists = false,
                Message = $"Class name \"{name}\" is available."
            });
        }


    }
}
