using Application.Interfaces.Roles;
using AuthLibrary.Constants.Authentication;
using Infrastructure.Constants;
using Infrastructure.Contexts;
using Infrastructure.Models;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ResponseResult.Models.Requests.Identity;
using ResponseResult.Models.Responses.Identity;
using ResponseResult.Wrappers;

namespace Infrastructure.Services.Roles
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public RoleService(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IResponseWrapper> CreateRoleAsync(CreateRoleRequest createRoleRequest)
        {
            var roleInDb = await _roleManager.FindByNameAsync(createRoleRequest.Name);
            if (roleInDb is not null)
            {
                return await ResponseWrapper.FailAsync("Role already exists");
            }

            var newRole = new ApplicationRole
            {
                Name = createRoleRequest.Name,
                Description = createRoleRequest.Description
            };

            var identityResult = await _roleManager.CreateAsync(newRole);
            if(identityResult.Succeeded)
            {
                return await ResponseWrapper.SuccessAsync("Role created successfully");
            }
            return await ResponseWrapper.FailAsync(GetIdentityResultErrorDescription(identityResult));
        }

        public async Task<IResponseWrapper> DeleteRolesAsync(string roleId)
        {
            var roleInDb = await _roleManager.FindByIdAsync(roleId);
            if (roleInDb is not null)
            {
                if(roleInDb.Name != AppRoles.Admin)
                {
                    var allUsers = await _userManager.Users.ToListAsync();
                    foreach (var user in allUsers)
                    {
                        if (await _userManager.IsInRoleAsync(user, roleInDb.Name))
                        {
                            return await ResponseWrapper.FailAsync($"Role: {roleInDb.Name} is currently assigned to a user");

                        }
                    }

                    var identityResult = await _roleManager.DeleteAsync(roleInDb);
                    if (identityResult.Succeeded)
                    {
                        return await ResponseWrapper.SuccessAsync("Role deleted successfully");
                    }
                    return await ResponseWrapper.FailAsync(GetIdentityResultErrorDescription(identityResult));
                }
                return await ResponseWrapper.FailAsync("Failed to delete role");
            }
            return await ResponseWrapper.FailAsync("Role not found");
        }

        public async Task<IResponseWrapper> GetRoleByIdAsync(string roleId)
        {
            var roleInDb = await _roleManager.FindByIdAsync(roleId);
            if (roleInDb is not null)
            {
                var mappedRole = roleInDb.Adapt<RoleResponse>();
                return await ResponseWrapper<RoleResponse>.SuccessAsync(mappedRole);
            }
            return await ResponseWrapper.FailAsync("Role not found");
        }

        public async Task<IResponseWrapper> GetRolesAsync()
        {
            var allRoles = await _roleManager.Roles.ToListAsync();
            if (allRoles.Any())
            {
                var mappedRoles = allRoles.Adapt<List<RoleResponse>>();
                return await ResponseWrapper<List<RoleResponse>>.SuccessAsync(mappedRoles);
            }
            return await ResponseWrapper.FailAsync("No roles found");
        }
        public async Task<IResponseWrapper> UpdateRoleAsync(UpdateRoleRequest roleUpdateRequest)
        {
            var roleInDb = await _roleManager.FindByIdAsync(roleUpdateRequest.RoleId);
            if (roleInDb is not null)
            {
                if (roleInDb.Name != AppRoles.Admin)
                {
                    roleInDb.Name = roleUpdateRequest.Name;
                    roleInDb.Description = roleUpdateRequest.Description;

                    var identityResult = await _roleManager.UpdateAsync(roleInDb);
                    if (identityResult.Succeeded)
                    {
                        return await ResponseWrapper.SuccessAsync("Role updated successfully");
                    }
                    return await ResponseWrapper.FailAsync(GetIdentityResultErrorDescription(identityResult));
                }
                return await ResponseWrapper.FailAsync("Cannot update Admin role");
            }
            return await ResponseWrapper.FailAsync("Role not found");
        }

        public async Task<IResponseWrapper> UpdatePermissionsAsync(UpdateRoleClaimsRequest updateRoleClaimsRequest)
        {
            var roleInDb = await _roleManager.FindByIdAsync(updateRoleClaimsRequest.RoleId);
            if (roleInDb is not null)
            {
                if (roleInDb.Name != AppRoles.Admin)
                {
                    var toBeAssignedPermissions = updateRoleClaimsRequest.RoleClaims
                        .Where(rc => rc.IsAssignedToRole ==true)
                        .ToList();

                    var currentlyAssignedPermissions = await _roleManager.GetClaimsAsync(roleInDb);

                    //drop permissions here
                    foreach (var claim in currentlyAssignedPermissions)
                    {
                        var removeResult = await _roleManager.RemoveClaimAsync(roleInDb, claim);
                        if (!removeResult.Succeeded)
                        {
                            return await ResponseWrapper.FailAsync(GetIdentityResultErrorDescription(removeResult));
                        }
                    }

                    //add new permissions here
                    var mappedRoleClaims = toBeAssignedPermissions.Adapt<List<ApplicationRoleClaim>>();
                    await _context.RoleClaims.AddRangeAsync(mappedRoleClaims);
                    await _context.SaveChangesAsync();

                    return await ResponseWrapper.SuccessAsync("Role permissions updated successfully");                   
                }
                return await ResponseWrapper.FailAsync("Cannot update permissions for Admin role");
            }
            return await ResponseWrapper.FailAsync("Role not found");
        }
        public async Task<IResponseWrapper> GetPermissionsAsync(string roleId)
        {
            var roleInDb = await _roleManager.FindByIdAsync(roleId);
            if (roleInDb is not null)
            {
                var allPermissions = AppPermissions.AllPermissions;
                var roleClaimResponses = new RoleClaimsResponse
                {
                    Role = new RoleResponse
                    {
                        Id = roleInDb.Id,
                        Name = roleInDb.Name,
                        Description = roleInDb.Description
                    },
                    RoleClaims = new List<RoleClaimViewModel>()
                };

                var currentlyAssignedClaims = await GetAllClaimsForRoleAsync(roleId);

                var allPermissionNames = allPermissions.Select(ap => ap.Name).ToList(); //Permission.Identity.Users.Create
                
                var currentlyAssignedClaimValues = currentlyAssignedClaims
                    .Select(cac => cac.ClaimValue).ToList(); //Permission.Identity.Users.Create

                var currentlyAssignedRoleClaimsNames = allPermissionNames
                    .Intersect(currentlyAssignedClaimValues)
                    .ToList();

                foreach (var permission in allPermissions)
                {
                    if (currentlyAssignedRoleClaimsNames.Contains(permission.Name))
                    {
                        roleClaimResponses.RoleClaims.Add(new RoleClaimViewModel
                        {
                            ClaimType = AppClaim.Permission,
                            ClaimValue = permission.Name,
                            IsAssignedToRole = true,
                            RoleId = roleId,
                            Description = permission.Description,
                            Group = permission.Group
                        });
                    }
                    else
                    {
                        roleClaimResponses.RoleClaims.Add(new RoleClaimViewModel
                        {
                            ClaimType = AppClaim.Permission,
                            ClaimValue = permission.Name,
                            IsAssignedToRole = false,
                            RoleId = roleId,
                            Description = permission.Description,
                            Group = permission.Group
                        });
                    }
                }
                return await ResponseWrapper<RoleClaimsResponse>.SuccessAsync(data: roleClaimResponses);
            }
            return await ResponseWrapper.FailAsync("Role not found");
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

        private async Task<List<RoleClaimViewModel>> GetAllClaimsForRoleAsync(string roleId)
        {
            var roleClaims = await _context.RoleClaims
                .Where(rc => rc.RoleId == roleId)
                .ToListAsync();

            if (roleClaims.Any())
            {
                var mappedRoleClaim = roleClaims.Adapt<List<RoleClaimViewModel>>();
                return mappedRoleClaim;
            }
            return new List<RoleClaimViewModel>();
        }
        #endregion
    }
}
