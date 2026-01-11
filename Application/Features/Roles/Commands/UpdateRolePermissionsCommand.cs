using Application.Interfaces.Roles;
using MediatR;
using ResponseResult.Models.Requests.Identity;
using ResponseResult.Wrappers;

namespace Application.Features.Roles.Commands
{
    public class UpdateRolePermissionsCommand : IRequest<IResponseWrapper>
    {
        public UpdateRoleClaimsRequest UpdateRoleClaimsRequest { get; set; }
    }

    public class UpdateRolePermissionsCommandHandler : IRequestHandler<UpdateRolePermissionsCommand, IResponseWrapper>
    {
        private readonly IRoleService _roleService;
        public UpdateRolePermissionsCommandHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }
        public async Task<IResponseWrapper> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
        {
            return await _roleService.UpdatePermissionsAsync(request.UpdateRoleClaimsRequest);
        }
    }
}
