using Application.Interfaces.Users;
using MediatR;
using ResponseResult.Models.Requests.Identity;
using ResponseResult.Wrappers;

namespace Application.Features.Users.Commands
{
    public class ChangeUserStatusCommand : IRequest<IResponseWrapper>
    {
        public ChangeUserStatusRequest ChangeUserStatus { get; set; }
    }

    public class ChangeUserStatusCommandHandler : IRequestHandler<ChangeUserStatusCommand, IResponseWrapper>
    {
        private readonly IUserService _userService;
        public ChangeUserStatusCommandHandler(IUserService userService)
        {
            _userService = userService;
        }
        public async Task<IResponseWrapper> Handle(ChangeUserStatusCommand request, CancellationToken cancellationToken)
        {
            return await _userService.ChnageUserStatusAsync(request.ChangeUserStatus);
        }

    }
}
