using ResponseResult.Models.Requests.Identity;
using ResponseResult.Wrappers;

namespace Application.Interfaces.Users
{
    public interface IUserService
    {
        Task<IResponseWrapper> RegisterUserAsync(UserRegistrationRequest userRegistration);
        Task<IResponseWrapper> UpdateUserAsync(UpdateUserRequest userUpdate);
        Task<IResponseWrapper> GetUserByIdAsync(string userId);
        Task<IResponseWrapper> GetAllUsersAsync();
        Task<IResponseWrapper> ChangeUserPasswordAsync(ChangePasswordRequest changePassword);
        Task<IResponseWrapper> ChnageUserStatusAsync(ChangeUserStatusRequest changeUserStatus);
        Task<IResponseWrapper> GetUserRolesAsync(string userId);
        Task<IResponseWrapper> UpdateUserRolesAsync(UpdateUserRolesRequest updateUserRoles);
        Task<IResponseWrapper> GetUserByEmailAsync(string email);


    }
}
