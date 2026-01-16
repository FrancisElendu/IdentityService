using Application.Features.Roles.Commands;
using Application.Features.Users.Commands;
using Application.Features.Users.Queries;
using AuthLibrary.Attributes;
using AuthLibrary.Constants.Authentication;
using Microsoft.AspNetCore.Mvc;
using ResponseResult.Models.Requests.Identity;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : MyBaseController<UsersController>
    {
        [HttpPost("register")]
        [MustHavePermission(AppService.Identity, AppFeature.Users, AppAction.Create)]
        public async Task<IActionResult> RegisterUserAsync([FromBody] UserRegistrationRequest userRegistration)
        {
            var response = await Sender.Send(new UserRegistrationCommand { UserRegistration = userRegistration });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpGet("{userId}")]
        [MustHavePermission(AppService.Identity, AppFeature.Users, AppAction.Read)]
        public async Task<IActionResult> GetUserByIdAsync(string userId)
        {
            var response = await Sender.Send(new GetUserByIdQuery { UserId = userId });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return NotFound(response);
        }

        [HttpGet("all")]
        [MustHavePermission(AppService.Identity, AppFeature.Users, AppAction.Read)]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            var response = await Sender.Send(new GetAllUserQuery());
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return NotFound(response);
        }

        [HttpPut]
        [MustHavePermission(AppService.Identity, AppFeature.Users, AppAction.Update)]
        public async Task<IActionResult> UpdateUserAsync([FromBody] UpdateUserRequest updateUserRequest)
        {
            var response = await Sender.Send(new UpdateUserCommand { UpdateUser = updateUserRequest });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPut("change-password")]
        [MustHavePermission(AppService.Identity, AppFeature.Users, AppAction.Create)]
        public async Task<IActionResult> ChangeUserPasswordAsync([FromBody] ChangePasswordRequest changePasswordRequest)
        {
            var response = await Sender.Send(new ChangeUserPasswordCommand { ChangePassword = changePasswordRequest });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPut("change-status")]
        [MustHavePermission(AppService.Identity, AppFeature.Users, AppAction.Update)]
        public async Task<IActionResult> ChangeUserStatusAsync([FromBody] ChangeUserStatusRequest changeUserStatusRequest)
        {
            var response = await Sender.Send(new ChangeUserStatusCommand { ChangeUserStatus = changeUserStatusRequest });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPut("user-roles")]
        [MustHavePermission(AppService.Identity, AppFeature.UserRoles, AppAction.Update)]
        public async Task<IActionResult> UpdateUserRolesAsync([FromBody] UpdateUserRolesRequest updateUserRolesRequest)
        {
            var response = await Sender.
                Send(new UpdateUserRolesCommand { UpdateUserRolesRequest = updateUserRolesRequest });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
    }
}
