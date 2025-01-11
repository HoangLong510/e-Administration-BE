using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs.User;
using Server.Models;
using Server.Models.Enums;
using Server.Utils;

namespace Server.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DatabaseContext db;

        public UserRepository(DatabaseContext db)
        {
            this.db = db;
        }

        public async Task<User> GetUserByLogin(UserLoginDto user)
        {
            var result = await db.Users.SingleOrDefaultAsync(u => u.Username == user.UserName);
            if (result != null)
            {
                var isValidPassword = PasswordHasher.VerifyPassword(user.Password, result.Password);
                if (isValidPassword)
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }
            return null;
        }

        public async Task<User> GetUserById(int userId)
        {
            var user = await db.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                return user;
            }
            return null;
        }

        public async Task<(List<UserResponseDto> users, int totalPages)> GetUsers(GetUsersRequestDto req)
        {
            var pageSize = 10;

            // Ensure PageNumber is at least 1 to prevent negative or zero values
            var pageNumber = Math.Max(1, req.PageNumber);

            var users = db.Users.AsQueryable();

            var totalUsers = await users.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);

            users = users.Where(u => u.IsActive == req.IsActive);

            if (!string.IsNullOrEmpty(req.Role))
            {
                users = users.Where(u => u.Role.ToString() == req.Role);
            }

            if (!string.IsNullOrEmpty(req.SearchValue))
            {
                string searchValueLower = req.SearchValue.ToLower();
                users = users.Where(u => u.FullName.ToLower().Contains(searchValueLower) ||
                                        u.Username.ToLower().Contains(searchValueLower) ||
                                        u.Email.ToLower().Contains(searchValueLower) ||
                                        u.Phone.ToLower().Contains(searchValueLower));
            }

            var pagedUsers = await users
                .Skip((pageNumber - 1) * pageSize)  // Ensure pageNumber is valid
                .Take(pageSize)
                .ToListAsync();

            var result = new List<UserResponseDto>();
            foreach (var user in pagedUsers)
            {
                result.Add(new UserResponseDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Address = user.Address,
                    Gender = user.Gender.ToString(),
                    Role = user.Role.ToString(),
                    IsActive = user.IsActive,
                    ClassId = user.ClassId,
                    DepartmentId = user.DepartmentId,
                    Avatar = user.Avatar
                });
            }

            return (result, totalPages);
        }

        public async Task<bool> CheckUsernameExists(string username)
        {
            var user = await db.Users.SingleOrDefaultAsync(u => u.Username == username);
            if(user == null)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> CreateUser(UserCreateDto user)
        {
            var hashedPassword = PasswordHasher.HashPassword(user.Password);
            var newUser = new User
            {
                FullName = user.FullName,
                Username = user.Username,
                Password = hashedPassword,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                DateOfBirth = DateTime.Parse(user.DateOfBirth),
            };
            try
            {
                newUser.Gender = Enum.Parse<UserGender>(user.Gender, true);
            }
            catch (ArgumentException)
            {
                newUser.Gender = UserGender.Other;
            }
            try
            {
                newUser.Role = Enum.Parse<UserRole>(user.Role, true);
            }
            catch (ArgumentException)
            {
                newUser.Role = UserRole.Student;
            }
            db.Users.Add(newUser);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<(bool success, string message)> DisableUser(int Id)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == Id);

            if (user == null)
            {
                return (false, "User does not exist!");
            }

            if(user.Username == "admin")
            {
                return (false, "You cannot disable the default administrator account");
            }

            user.IsActive = false;
            await db.SaveChangesAsync();

            return (true, "Disable account user successfully!");
        }
    }
}
