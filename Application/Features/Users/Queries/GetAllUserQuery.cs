using Application.Interfaces.Users;
using MediatR;
using ResponseResult.Wrappers;

namespace Application.Features.Users.Queries
{
    public class GetAllUserQuery : IRequest<IResponseWrapper>
    {
        public class GetAllUserQueryHandler : IRequestHandler<GetAllUserQuery, IResponseWrapper>
        {
            private readonly IUserService _userService;
            public GetAllUserQueryHandler(IUserService userService)
            {
                _userService = userService;
            }
            public async Task<IResponseWrapper> Handle(GetAllUserQuery request, CancellationToken cancellationToken)
            {
                return await _userService.GetAllUsersAsync();
            }
        }
    }
}
