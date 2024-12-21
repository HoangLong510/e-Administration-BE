using Server.DTOs.User;
using Server.Models;

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
    }
}
