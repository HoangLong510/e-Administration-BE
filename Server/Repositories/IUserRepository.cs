using Server.DTOs.User;
using Server.Models;
using Server.Models.Enums;

namespace Server.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserByLogin(UserLoginDto user);
        Task<User> GetUserById(int userId);
        Task<(List<UserResponseDto> users, int totalPages)> GetUsers(GetUsersRequestDto req);
        Task<bool> CreateUser(UserCreateDto user);
        Task<bool> CheckUsernameExists(string username);
        Task<(bool success, string message)> DisableUser(int Id);

        Task<bool> UpdateUser(int userId, UpdateUserDto updatedUser);
        Task<bool> EditUser(UserEditDto user);
        Task<bool> ChangePassword(int userId, string newPasswordHash);
        Task<int> GetTotalUsersAsync();

        Task<List<User>> GetUsersByRoleAsync(UserRole role);
    }
}
