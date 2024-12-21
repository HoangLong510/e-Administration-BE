using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.DTOs.User;
using Server.Repositories;
using Server.Services;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository userRepo;
        private readonly TokenService tokenService;

        public AuthController(IUserRepository userRepo, TokenService tokenService)
        {
            this.userRepo = userRepo;
            this.tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login (UserLoginDto userLogin)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(userLogin.UserName))
            {
                errors["username"] = "Username is required";
            }

            if (string.IsNullOrWhiteSpace(userLogin.Password))
            {
                errors["password"] = "Password is required";
            }

            if (errors.Count > 0)
            {
                return BadRequest(new
                {
                    Success = false,
                    Errors = errors,
                    Message = "Invalid login information! Please check the errors of the fields again."
                });
            }
            else
            {
                var user = await userRepo.GetUserByLogin(userLogin);

                if (user == null)
                {
                    errors["username"] = "Username or password is incorrect!";
                    errors["password"] = "Email or password is incorrect!";

                    return BadRequest(new
                    {
                        Success = false,
                        Errors = errors,
                        Message = "Username or password is incorrect!"
                    });
                }

                if (!user.IsActive)
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = "Your account is not active. Please contact administrator to resolve"
                    });
                }

                var token = tokenService.GenerateToken(user);

                Response.Cookies.Append("token", token, new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.None,
                    Secure = true,
                    Expires = DateTime.UtcNow.AddHours(1)
                });

                var userResponse = new UserResponseDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Address = user.Address,
                    Avatar = user.Avatar,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender.ToString(),
                    Role = user.Role.ToString(),
                    IsActive = user.IsActive,
                    ClassId = user.ClassId,
                    DepartmentId = user.DepartmentId
                };

                return Ok(new
                {
                    Success = true,
                    Message = "Login Successfully!",
                    User = userResponse
                });
            }
        }

        [HttpGet("fetch-user")]
        public async Task<ActionResult> fetchUser()
        {
            var token = Request.Cookies["token"];

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new
                {
                    Success = false
                });
            }

            var userId = tokenService.GetUserIdFromToken(token);
            var tokenExp = tokenService.GetTokenExpiration(token);

            if (tokenExp == null || userId == null)
            {
                return Unauthorized(new
                {
                    Success = false
                });
            }

            var user = await userRepo.GetUserById(int.Parse(userId));

            if (user == null)
            {
                return Unauthorized(new
                {
                    Success = false
                });
            }

            // refresh token
            var timeLeft = tokenExp.Value - DateTime.UtcNow;

            if (timeLeft.TotalMinutes < 10)
            {
                var refreshToken = tokenService.GenerateToken(user);

                Response.Cookies.Append("token", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.None,
                    Secure = true,
                    Expires = DateTime.UtcNow.AddHours(1)
                });
            }

            var userResponse = new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Avatar = user.Avatar,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender.ToString(),
                Role = user.Role.ToString()
            };

            return Ok(new
            {
                Success = true,
                user = userResponse
            });
        }

        [HttpGet("logout")]
        public async Task<ActionResult> Logout()
        {
            Response.Cookies.Append("token", "", new CookieOptions
            {
                Expires = DateTime.UtcNow.AddDays(-1),
                Path = "/",
                Secure = true,
                HttpOnly = true,
                SameSite = SameSiteMode.None
            });
            return Ok(new
            {
                Success = true
            });
        }

        [HttpPut("update-user")]
        public async Task<ActionResult> UpdateUser(UpdateUserDto userUpdate)
        {
            var token = Request.Cookies["token"];
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { Success = false, Message = "Unauthorized" });
            }

            var userId = tokenService.GetUserIdFromToken(token);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Success = false, Message = "Invalid token" });
            }

            var updateResult = await userRepo.UpdateUser(int.Parse(userId), userUpdate);

            if (!updateResult)
            {
                return NotFound(new { Success = false, Message = "User not found" });
            }

            var updatedUser = await userRepo.GetUserById(int.Parse(userId));

            var updatedUserResponse = new UserResponseDto
            {
                Id = updatedUser.Id,
                Username = updatedUser.Username,
                FullName = updatedUser.FullName,
                Email = updatedUser.Email,
                Phone = updatedUser.Phone,
                Address = updatedUser.Address,
                Avatar = updatedUser.Avatar,
                DateOfBirth = updatedUser.DateOfBirth,
                Gender = updatedUser.Gender.ToString(),
                Role = updatedUser.Role.ToString(),
                IsActive = updatedUser.IsActive
            };

            return Ok(new { Success = true, User = updatedUserResponse, Message = "Profile updated successfully!" });
        }


    }
}
