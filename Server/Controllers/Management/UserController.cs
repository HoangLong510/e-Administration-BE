using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.DTOs.User;
using Server.Repositories;
using Server.Services;
using System.Text.RegularExpressions;

namespace Server.Controllers.Management
{
    [Route("api/management/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository userRepo;
        private readonly TokenService tokenService;

        public UserController(IUserRepository userRepo, TokenService tokenService)
        {
            this.userRepo = userRepo;
            this.tokenService = tokenService;
        }

        [HttpPost("get-users")]
        public async Task<ActionResult> GetUsers(GetUsersRequestDto req)
        {
            var token = Request.Cookies["token"];
            var role = tokenService.GetRoleFromToken(token);
            if(role != "Admin")
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "You do not have permission to perform this action"
                });
            }

            var (users, totalPages) = await userRepo.GetUsers(req);

            return Ok(new
            {
                Success = true,
                Users = users,
                TotalPages = totalPages
            });
        }

        [HttpPost("create-user")]
        public async Task<ActionResult> CreateUser(UserCreateDto user)
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

            // check fullname
            if (string.IsNullOrWhiteSpace(user.FullName))
            {
                errors["fullName"] = "Full name is required";
            }
            else
            {
                string pattern = @"^(?! )[a-zA-Z\u0080-\uFFFF\s]{2,50}(?<! )$";
                Regex regex = new Regex(pattern);
                if (!regex.IsMatch(user.FullName))
                {
                    errors["fullName"] = "Full name must be between 2 and 50 characters and not contain any special characters";
                }
            }

            // check username
            if (string.IsNullOrWhiteSpace(user.Username))
            {
                errors["username"] = "Username is required";
            }
            else
            {
                string pattern = @"^[a-z][a-z0-9_]{4,19}$";
                Regex regex = new Regex(pattern);
                if (!regex.IsMatch(user.Username))
                {
                    errors["username"] = "Username must be between 5 and 20 characters. Must start with a lowercase letter";
                }
                else
                {
                    var checkUsername = await userRepo.CheckUsernameExists(user.Username);
                    if (checkUsername)
                    {
                        errors["email"] = "Email already exists";
                    }
                }
            }

            // check password
            if (string.IsNullOrWhiteSpace(user.Password))
            {
                errors["password"] = "Password is required";
            }
            else
            {
                string pattern = @"^.{6,30}$";
                Regex regex = new Regex(pattern);
                if (!regex.IsMatch(user.Password))
                {
                    errors["password"] = "Password must be between 6 and 30 characters";
                }
            }

            // check confirmPassword
            if (string.IsNullOrWhiteSpace(user.ConfirmPassword))
            {
                errors["confirmPassword"] = "Confirm password is required!";
            }
            else
            {
                if (!user.ConfirmPassword.Equals(user.Password))
                {
                    errors["confirmPassword"] = "Confirm password and password does not match";
                }
            }

            // check email
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                errors["email"] = "Email is required";
            }
            else
            {
                string pattern = @"^(([^<>()[\]\\.,;:\s@""']+(\.[^<>()[\]\\.,;:\s@""']+)*)|("".+""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$";
                Regex regex = new Regex(pattern);
                if (!regex.IsMatch(user.Email))
                {
                    errors["email"] = "Email address is not valid";
                }
            }

            // check phone
            if (string.IsNullOrWhiteSpace(user.Phone))
            {
                errors["phone"] = "Phone is required";
            }
            else
            {
                string pattern = @"^(0|\+84)(\s|\.)?((3[2-9])|(5[26-9])|(7[0|6-9])|(8[0-9])|(9[0-9]))(\d){7}$";
                Regex regex = new Regex(pattern);
                if (!regex.IsMatch(user.Phone))
                {
                    errors["phone"] = "Invalid phone number";
                }
            }

            // check DateOfBirth 
            if (string.IsNullOrWhiteSpace(user.DateOfBirth))
            {
                errors["dateOfBirth"] = "Date of Birth is required";
            }
            else
            {
                try
                {
                    var formatDate = DateTime.Parse(user.DateOfBirth);
                    if (formatDate >= DateTime.Now.Date)
                    {
                        errors["dateOfBirth"] = "Date of Birth cannot be in the future";
                    }
                }
                catch (FormatException)
                {
                    errors["dateOfBirth"] = "Invalid Date of Birth format";
                }
            }

            if (errors.Count > 0)
            {
                return BadRequest(new
                {
                    Success = false,
                    Errors = errors,
                    Message = "Invalid user information! Please check the errors of the fields again."
                });
            }

            var result = await userRepo.CreateUser(user);

            if (!result)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Create user failed!"
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "User created successfully!"
            });
        }

        [HttpGet("disable/{id}")]
        public async Task<ActionResult> DisableUser(int Id)
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

            var (success, message) = await userRepo.DisableUser(Id);

            if(!success)
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

        [HttpGet("get-user/{id}")]
        public async Task<ActionResult> GetUser (int Id)
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

            var user = await userRepo.GetUserById(Id);

            if (user == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            return Ok(new
            {
                Success = true,
                User = user
            });
        }

        [HttpPut("edit-user")]
        public async Task<ActionResult> EditUser (UserEditDto user)
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

            // check fullname
            if (string.IsNullOrWhiteSpace(user.FullName))
            {
                errors["fullName"] = "Full name is required";
            }
            else
            {
                string pattern = @"^(?! )[a-zA-Z\u0080-\uFFFF\s]{2,50}(?<! )$";
                Regex regex = new Regex(pattern);
                if (!regex.IsMatch(user.FullName))
                {
                    errors["fullName"] = "Full name must be between 2 and 50 characters and not contain any special characters";
                }
            }

            // check email
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                errors["email"] = "Email is required";
            }
            else
            {
                string pattern = @"^(([^<>()[\]\\.,;:\s@""']+(\.[^<>()[\]\\.,;:\s@""']+)*)|("".+""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$";
                Regex regex = new Regex(pattern);
                if (!regex.IsMatch(user.Email))
                {
                    errors["email"] = "Email address is not valid";
                }
            }

            // check phone
            if (string.IsNullOrWhiteSpace(user.Phone))
            {
                errors["phone"] = "Phone is required";
            }
            else
            {
                string pattern = @"^(0|\+84)(\s|\.)?((3[2-9])|(5[26-9])|(7[0|6-9])|(8[0-9])|(9[0-9]))(\d){7}$";
                Regex regex = new Regex(pattern);
                if (!regex.IsMatch(user.Phone))
                {
                    errors["phone"] = "Invalid phone number";
                }
            }

            // check DateOfBirth 
            if (string.IsNullOrWhiteSpace(user.DateOfBirth))
            {
                errors["dateOfBirth"] = "Date of Birth is required";
            }
            else
            {
                try
                {
                    var formatDate = DateTime.Parse(user.DateOfBirth);
                    if (formatDate >= DateTime.Now.Date)
                    {
                        errors["dateOfBirth"] = "Date of Birth cannot be in the future";
                    }
                }
                catch (FormatException)
                {
                    errors["dateOfBirth"] = "Invalid Date of Birth format";
                }
            }

            if (errors.Count > 0)
            {
                return BadRequest(new
                {
                    Success = false,
                    Errors = errors,
                    Message = "Invalid user information! Please check the errors of the fields again."
                });
            }

            var result = await userRepo.EditUser(user);

            if (!result)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Edit user failed!"
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "User edited successfully!"
            });
        }


        [HttpGet("total-users")]
        public async Task<IActionResult> GetTotalUsers()
        {
            try
            {
                var totalUsers = await userRepo.GetTotalUsersAsync();
                return Ok(new { success = true, totalUsers });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

    }
}
