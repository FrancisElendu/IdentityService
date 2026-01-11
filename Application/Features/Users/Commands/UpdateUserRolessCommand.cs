using Application.Interfaces.Users;
using MediatR;
using ResponseResult.Models.Requests.Identity;
using ResponseResult.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands
{
    public class UpdateUserRolessCommand : IRequest<IResponseWrapper>
    {
        public UpdateUserRolesRequest UpdateUserRoles { get; set; }
    }
    public class UpdateUserRolesCommandHandler : IRequestHandler<UpdateUserRolessCommand, IResponseWrapper>
    {
        private readonly IUserService _userService;
        public UpdateUserRolesCommandHandler(IUserService userService)
        {
            _userService = userService;
        }
        public async Task<IResponseWrapper> Handle(UpdateUserRolessCommand request, CancellationToken cancellationToken)
        {
            return await _userService.UpdateUserRolesAsync(request.UpdateUserRoles);
        }
    }
}
