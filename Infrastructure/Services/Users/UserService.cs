using Application.Interfaces.Users;
using Infrastructure.Constants;
using Infrastructure.Models;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ResponseResult.Models.Requests.Identity;
using ResponseResult.Models.Responses.Identity;
using ResponseResult.Wrappers;

namespace Infrastructure.Services.Users
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IResponseWrapper> ChangeUserPasswordAsync(ChangePasswordRequest changePassword)
        {
            var userInDb = await _userManager.FindByIdAsync(changePassword.UserId);
            if (userInDb is not null)
            {
                var identityResult = await _userManager.ChangePasswordAsync(
                userInDb,
                changePassword.CurrentPassword,
                changePassword.NewPassword);

                if (identityResult.Succeeded)
                {
                    return await ResponseWrapper.SuccessAsync(message: "Password changed successfully.");
                }
                return await ResponseWrapper.FailAsync(GetIdentityResultErrorDescription(identityResult));
            }
            return await ResponseWrapper.FailAsync("User not found.");

        }

        public async Task<IResponseWrapper> ChnageUserStatusAsync(ChangeUserStatusRequest changeUserStatus)
        {
            var userInDb = await _userManager.FindByIdAsync(changeUserStatus.UserId);
            if (userInDb is not null) 
            {
                userInDb.IsActive = changeUserStatus.ActivateOrDeactivate;

                var identityResult = await _userManager.UpdateAsync(userInDb);
                if (identityResult.Succeeded)
                {
                    var status = changeUserStatus.ActivateOrDeactivate ? "activated" 
                        : "deactivated";
                    return await ResponseWrapper.SuccessAsync($"User {status} successfully.");
                }
                return await ResponseWrapper.FailAsync(GetIdentityResultErrorDescription(identityResult));
            }
            return await ResponseWrapper.FailAsync("User not found");
        }

        public async Task<IResponseWrapper> GetAllUsersAsync()
        {
            var usersInDb = await _userManager.Users.ToListAsync();
            if (usersInDb.Any()) 
            {
                var mappedUsers = usersInDb.Adapt<List<UserResponse>>();
                return await ResponseWrapper<List<UserResponse>>.SuccessAsync(data: mappedUsers);
            }
            return await ResponseWrapper.FailAsync("No users found.");
        }

        public Task<IResponseWrapper> GetUserByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        public async Task<IResponseWrapper> GetUserByIdAsync(string userId)
        {
            var userInDb = _userManager.FindByIdAsync(userId);
            if (userInDb is not null)
            {
                var mappedUser = userInDb.Adapt<UserResponse>();
                return await ResponseWrapper<UserResponse>.SuccessAsync(data: mappedUser);
            }
            return await ResponseWrapper.FailAsync("User not found.");
        }

        public Task<IResponseWrapper> UpdateUserRolesAsync(UpdateUserRolesRequest updateUserRoles)
        {
            throw new NotImplementedException();
        }
        public Task<IResponseWrapper> GetUserRolesAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<IResponseWrapper> RegisterUserAsync(UserRegistrationRequest userRegistration)
        {
            var userWithEmail = await _userManager.FindByEmailAsync(userRegistration.Email);
            if (userWithEmail is not null)
                return await ResponseWrapper.FailAsync("Email address is already registered.");

            var newUser = new ApplicationUser
            {
                FirstName = userRegistration.FirstName,
                LastName = userRegistration.LastName,
                UserName = userRegistration.Username,
                Email = userRegistration.Email,
                IsActive = userRegistration.ActivateUser,
                PhoneNumber = userRegistration.PhoneNumber,
                EmailConfirmed = userRegistration.AutoConfirmEmail
            };

            //password hasher
            var password = new PasswordHasher<ApplicationUser>();
            newUser.PasswordHash = password.HashPassword(newUser, userRegistration.Password);

            var identityUserResult = await _userManager.CreateAsync(newUser);
            if (identityUserResult.Succeeded)
            {
                //Assign to role(s)
                var identityRoleResult = await _userManager.AddToRoleAsync(newUser, AppRoles.Basic);
                if (identityRoleResult.Succeeded) 
                {
                    return await ResponseWrapper.SuccessAsync("User registered successfully.");
                }
                return await ResponseWrapper.FailAsync(GetIdentityResultErrorDescription(identityRoleResult));
            }
            return await ResponseWrapper.FailAsync(GetIdentityResultErrorDescription(identityUserResult));
        }

        public async Task<IResponseWrapper> UpdateUserAsync(UpdateUserRequest userUpdate)
        {
            var userInDb = await _userManager.FindByIdAsync(userUpdate.UserId);
            if (userInDb is not null)
            {
                // update
                userInDb.FirstName = userUpdate.FirstName;
                userInDb.LastName = userUpdate.LastName;
                userInDb.PhoneNumber = userUpdate.PhoneNumber;

                var identityResult = await _userManager.UpdateAsync(userInDb);
                if (identityResult.Succeeded)
                {
                    return await ResponseWrapper.SuccessAsync("User updated successfully.");
                }
                return await ResponseWrapper.FailAsync(GetIdentityResultErrorDescription(identityResult));
            }
            return await ResponseWrapper.FailAsync("User not found.");
        }

        #region Private Helper
        private List<string> GetIdentityResultErrorDescription(IdentityResult identityResult)
        {
            var errorDescriptions = new List<string>();
            foreach (var error in identityResult.Errors)
            {
                errorDescriptions.Add(error.Description);
            }
            return errorDescriptions;
        }
        #endregion
    }
}
