using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.DTOs.Software;
using Server.Repositories;
using Server.Services;
using Server.Models;

namespace Server.Controllers
{
    [Route("api/management/[controller]")]
    [ApiController]
    public class SoftwareController : ControllerBase
    {
        private readonly ISoftwareRepository softwareRepo;
        private readonly TokenService tokenService;

        public SoftwareController(ISoftwareRepository softwareRepo, TokenService tokenService)
        {
            this.softwareRepo = softwareRepo;
            this.tokenService = tokenService;
        }

        [HttpPost("get-softwares")]
        public async Task<ActionResult> GetSoftwares(GetSoftwaresRequestDto req)
        {
            var token = Request.Cookies["token"];
            var role = tokenService.GetRoleFromToken(token);
            if (role != "Admin")
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "You do not have permission to perform this action"
                });
            }

            var (softwares, totalPages) = await softwareRepo.GetSoftwares(req);

            return Ok(new
            {
                Success = true,
                Softwares = softwares,
                TotalPages = totalPages
            });
        }

        [HttpGet("get-software/{id}")]
        public async Task<ActionResult> GetSoftwareById(int id)
        {
            var token = Request.Cookies["token"];
            var role = tokenService.GetRoleFromToken(token);
            if (role != "Admin")
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "You do not have permission to perform this action"
                });
            }

            var software = await softwareRepo.GetSoftwareById(id);
            if (software == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Software not found"
                });
            }

            return Ok(new
            {
                Success = true,
                Software = new
                {
                    software.Id,
                    software.LabId,
                    software.Name,
                    software.Type,
                    software.Description,
                    software.LicenseExpire,
                    software.Status
                }
            });
        }

        [HttpPost("create-software")]
        public async Task<ActionResult> CreateSoftware(SoftwareCreateDto software)
        {
            var token = Request.Cookies["token"];
            var role = tokenService.GetRoleFromToken(token);
            if (role != "Admin")
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "You do not have permission to perform this action"
                });
            }

            var errors = new Dictionary<string, string>();

            // Kiểm tra tên phần mềm
            if (string.IsNullOrWhiteSpace(software.Name))
            {
                errors["name"] = "Name is required";
            }
            else
            {
                var checkName = await softwareRepo.CheckNameExists(software.Name);
                if (checkName)
                {
                    errors["name"] = "Name already exists";
                }
            }

            // Kiểm tra ngày License Expire
            if (string.IsNullOrWhiteSpace(software.LicenseExpire))
            {
                errors["LicenseExpire"] = "License Expire is required";
            }
            else
            {
                try
                {
                    var formatDate = DateTime.Parse(software.LicenseExpire);
                    if (formatDate < DateTime.Now.Date)
                    {
                        errors["LicenseExpire"] = "License Expire date cannot be earlier than today";
                    }
                }
                catch (FormatException)
                {
                    errors["LicenseExpire"] = "Invalid License Expire format";
                }
            }

            // Kiểm tra mô tả
            if (string.IsNullOrWhiteSpace(software.Description))
            {
                errors["description"] = "Description is required";
            }

            if (errors.Count > 0)
            {
                var errorMessage = "Invalid software information! Please check the errors of the fields again:\n";
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

            try
            {
                var result = await softwareRepo.CreateSoftware(software);
                if (!result)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Create software failed!"
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Software created successfully!"
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating software: {ex.Message}");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while creating the software."
                });
            }
        }


        [HttpPut("update-software/{id}")]
        public async Task<ActionResult> UpdateSoftware(int id, SoftwareUpdateDto request)
        {
            var token = Request.Cookies["token"];
            var role = tokenService.GetRoleFromToken(token);
            if (role != "Admin")
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "You do not have permission to perform this action"
                });
            }

            var errors = new Dictionary<string, string>();

            // Kiểm tra tên phần mềm
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                errors["name"] = "Name is required";
            }
            else
            {
                var isUniqueName = await softwareRepo.IsSoftwareNameUnique(request.Name, id);
                if (!isUniqueName)
                {
                    errors["name"] = "Name already exists";
                }
            }

            // Kiểm tra ngày License Expire nếu đã thay đổi
            var software = await softwareRepo.GetSoftwareById(id);
            if (software == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Software not found"
                });
            }

            if (!string.IsNullOrWhiteSpace(request.LicenseExpire) && software.LicenseExpire != DateTime.Parse(request.LicenseExpire))
            {
                if (string.IsNullOrWhiteSpace(request.LicenseExpire))
                {
                    errors["LicenseExpire"] = "License Expire is required";
                }
                else
                {
                    try
                    {
                        var formatDate = DateTime.Parse(request.LicenseExpire);
                        if (formatDate < DateTime.Now.Date)
                        {
                            errors["LicenseExpire"] = "License Expire date cannot be earlier than today";
                        }
                    }
                    catch (FormatException)
                    {
                        errors["LicenseExpire"] = "Invalid License Expire format";
                    }
                }
            }

            // Kiểm tra mô tả
            if (string.IsNullOrWhiteSpace(request.Description))
            {
                errors["description"] = "Description is required";
            }

            if (errors.Count > 0)
            {
                var errorMessage = "Invalid software information! Please check the errors of the fields again:\n";
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

            try
            {
                var result = await softwareRepo.UpdateSoftware(id, request);
                if (!result)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Failed to update software"
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Software updated successfully"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"An error occurred while updating the software: {ex.Message}"
                });
            }
        }


        [HttpGet("disable-software/{id}")]
        public async Task<ActionResult> DisableSoftware(int id)
        {
            var token = Request.Cookies["token"];
            var role = tokenService.GetRoleFromToken(token);
            if (role != "Admin")
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "You do not have permission to perform this action"
                });
            }

            var (result, message) = await softwareRepo.DisableSoftware(id);
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

        [HttpGet("count-expired-softwares")]
        public async Task<ActionResult> CountExpiredSoftwares()
        {
            try
            {
                var count = await softwareRepo.CountExpiredSoftware();

                return Ok(new
                {
                    Success = true,
                    Count = count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"An error occurred while counting expired software: {ex.Message}"
                });
            }
        }

        // New Endpoint to Trigger Status Update for Expired Licenses
        [HttpPut("update-status-expired")]
        public async Task<ActionResult> UpdateStatusForExpiredLicenses()
        {
            var token = Request.Cookies["token"];
            var role = tokenService.GetRoleFromToken(token);
            if (role != "Admin")
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "You do not have permission to perform this action"
                });
            }

            try
            {
                await softwareRepo.UpdateStatusForExpiredLicenses();
                return Ok(new
                {
                    Success = true,
                    Message = "Status of expired licenses updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"An error occurred while updating status: {ex.Message}"
                });
            }
        }
    }
}
