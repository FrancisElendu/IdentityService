using Application.Interfaces.Roles;
using MediatR;
using ResponseResult.Models.Requests.Identity;
using ResponseResult.Wrappers;

namespace Application.Features.Roles.Commands
{
    public class UpdateUserRolesCommand : IRequest<IResponseWrapper>
    {
        public UpdateUserRolesRequest UpdateUserRolesRequest { get; set; }
    }

    public class UpdateUserRolesCommandHandler : IRequestHandler<UpdateUserRolesCommand, IResponseWrapper>
    {
        private readonly IRoleService _roleService;
        public UpdateUserRolesCommandHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }
        public async Task<IResponseWrapper> Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
        {
            return await _roleService.UpdateUserRolesAsync(request.UpdateUserRolesRequest);
        }

    }
}
