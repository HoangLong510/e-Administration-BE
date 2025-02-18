﻿using Microsoft.EntityFrameworkCore;
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

        public async Task<List<User>> GetUsersByRoleAsync(UserRole role)
        {
            var users = await db.Users
                                        .Where(u => u.Role == role && u.IsActive)
                                        .ToListAsync();
            return users;
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

            var totalUsers = await users.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);

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
                DateOfBirth = DateTime.Parse(user.DateOfBirth)
            };

            if (user.ClassId != 0)
            {
                newUser.ClassId = user.ClassId;
            }
            else
            {
                newUser.ClassId = null;
            }

            if (user.DepartmentId != 0)
            {
                newUser.DepartmentId = user.DepartmentId;
            }
            else
            {
                newUser.DepartmentId = null;
            }

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
                return false;
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

        public async Task<bool> UpdateUser(int userId, UpdateUserDto updatedUser)
        {
            var user = await db.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            // Cập nhật thông tin người dùng
            if (!string.IsNullOrEmpty(updatedUser.Phone))
            {
                user.Phone = updatedUser.Phone;
            }

            if (!string.IsNullOrEmpty(updatedUser.Email))
            {
                user.Email = updatedUser.Email;
            }

            if (!string.IsNullOrEmpty(updatedUser.Address))
            {
                user.Address = updatedUser.Address;
            }

            // Xử lý avatar
            if (updatedUser.Avatar != null)
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Uploads");

                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                var originalFileName = Path.GetFileName(updatedUser.Avatar.FileName);
                var filePath = Path.Combine(uploadDir, originalFileName);

                if (File.Exists(filePath))
                {
                    user.Avatar = originalFileName;
                }
                else
                {
                    var uniqueFileName = $"{Guid.NewGuid()}_{originalFileName}";
                    var uniqueFilePath = Path.Combine(uploadDir, uniqueFileName);

                    try
                    {
                        if (!string.IsNullOrEmpty(user.Avatar))
                        {
                            var oldAvatarPath = Path.Combine(uploadDir, user.Avatar);
                            if (File.Exists(oldAvatarPath))
                            {
                                File.Delete(oldAvatarPath);
                            }
                        }

                        using (var stream = new FileStream(uniqueFilePath, FileMode.Create))
                        {
                            await updatedUser.Avatar.CopyToAsync(stream);
                        }

                        user.Avatar = uniqueFileName;
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
            }

            await db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> EditUser(UserEditDto user)
        {
            var findUser = await db.Users.FirstOrDefaultAsync(u => u.Id ==  user.Id);

            if (findUser == null)
            {
                return false;
            }

            findUser.FullName = user.FullName;
            findUser.Email = user.Email;
            findUser.Phone = user.Phone;
            findUser.Address = user.Address;
            findUser.DateOfBirth = DateTime.Parse(user.DateOfBirth);
            if(user.DepartmentId != 0)
            {
                findUser.DepartmentId = user.DepartmentId;
            }
            else
            {
                findUser.DepartmentId = null;
            }

            if(user.ClassId != 0)
            {
                findUser.ClassId = user.ClassId;
            }
            else
            {
                findUser.ClassId = null;
            }

            try
            {
                findUser.Gender = Enum.Parse<UserGender>(user.Gender, true);
            }
            catch (ArgumentException)
            {
                findUser.Gender = UserGender.Other;
            }
            try
            {
                findUser.Role = Enum.Parse<UserRole>(user.Role, true);
            }
            catch (ArgumentException)
            {
                return false;
            }
            findUser.IsActive = user.IsActive;

            await db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ChangePassword(int userId, string newPasswordHash)
        {
            var user = await db.Users.FindAsync(userId);

            if (user == null)
                return false;

            user.Password = newPasswordHash;

            db.Users.Update(user);
            return await db.SaveChangesAsync() > 0;
        }


        public async Task<int> GetTotalUsersAsync()
        {
            try
            {
                var totalUsers = await db.Users.CountAsync(u => u.IsActive); 
                return totalUsers;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching the total users.", ex);
            }
        }

        public async Task<List<UserResponseDto>> GetUserForTaskAssignees()
        {
            var users = await db.Users.Where(u => u.Role.ToString() == "TechnicalStaff" ||
                                                  u.Role.ToString() == "Instructor")
                                      .ToListAsync();
            
            var result = users.Select(user => new UserResponseDto
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
            }).ToList();

            return result;
        }
    }
}
