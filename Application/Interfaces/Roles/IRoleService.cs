using ResponseResult.Models.Requests.Identity;
using ResponseResult.Wrappers;

namespace Application.Interfaces.Roles
{
    public interface IRoleService
    {
        Task<IResponseWrapper> CreateRoleAsync(CreateRoleRequest createRoleRequest);
        Task<IResponseWrapper> UpdateRoleAsync(UpdateRoleRequest roleUpdateRequest);
        Task<IResponseWrapper> GetRoleByIdAsync(string roleId);
        Task<IResponseWrapper> GetRolesAsync();
        Task<IResponseWrapper> DeleteRolesAsync(string roleId);
        Task<IResponseWrapper> UpdatePermissionsAsync(UpdateRoleClaimsRequest updateRoleClaimsRequest);
        Task<IResponseWrapper> GetPermissionsAsync(string roleId);
    }
}
