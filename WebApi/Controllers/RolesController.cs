using Application.Features.Roles.Commands;
using Application.Features.Roles.Queries;
using AuthLibrary.Attributes;
using AuthLibrary.Constants.Authentication;
using Microsoft.AspNetCore.Mvc;
using ResponseResult.Models.Requests.Identity;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class RolesController : MyBaseController<RolesController>
    {
        [HttpPost()]
        [MustHavePermission(AppService.Identity, AppFeature.Roles, AppAction.Create)]
        public async Task<IActionResult> CreateRoleAsync([FromBody] CreateRoleRequest createRoleRequest)
        {
            var response = await Sender.Send(new CreateRoleCommand { CreateRole = createRoleRequest });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpGet("all")]
        [MustHavePermission(AppService.Identity, AppFeature.Roles, AppAction.Read)]
        public async Task<IActionResult> GetAllRolesAsync()
        {
            var response = await Sender.Send(new GetRolesQuery());
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return NotFound(response);
        }

        [HttpPut]
        [MustHavePermission(AppService.Identity, AppFeature.Roles, AppAction.Update)]
        public async Task<IActionResult> UpdateRoleRequest([FromBody] UpdateRoleRequest updateRoleRequest)
        {
            var response = await Sender.Send(new UpdateRoleCommand{UpdateRole = updateRoleRequest});
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpGet("{roleId}")]
        [MustHavePermission(AppService.Identity, AppFeature.Roles, AppAction.Read)]
        public async Task<IActionResult> GetRoleByIdAsync(string roleId)
        {
            var response = await Sender.Send(new GetRoleByIdQuery { RoleId = roleId});
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return NotFound(response);
        }

        [HttpDelete("{roleId}")]
        [MustHavePermission(AppService.Identity, AppFeature.Roles, AppAction.Delete)]
        public async Task<IActionResult> DeleteRoleAsync(string roleId)
        {
            var response = await Sender.Send(new DeleteRoleCommand { RoleId = roleId });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpGet("permissions/{roleId}")]
        [MustHavePermission(AppService.Identity, AppFeature.RoleClaims, AppAction.Read)]
        public async Task<IActionResult> GetPermissionsAsync(string roleId)
        {
            var response = await Sender.Send(new GetPermissionsQuery { RoleId = roleId });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return NotFound(response);
        }

        [HttpPut("update-permissions")]
        [MustHavePermission(AppService.Identity, AppFeature.RoleClaims, AppAction.Update)]
        public async Task<IActionResult> UpdateRolePermissions([FromBody] UpdateRoleClaimsRequest updateRoleClaimsRequest)
        {
            var response = await Sender.Send(new UpdateRolePermissionsCommand { UpdateRoleClaimsRequest = updateRoleClaimsRequest });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
    }
}
