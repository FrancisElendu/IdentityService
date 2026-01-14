using MediatR;
using ResponseResult.Models.Requests.Identity;
using ResponseResult.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Token.Queries
{
    public class GetRefreshTokenQuery : IRequest<IResponseWrapper>
    {
        public RefreshTokenRequest RefreshTokenRequest { get; set; }
    }

    public class GetRefreshTokenQueryHandler : IRequestHandler<GetRefreshTokenQuery, IResponseWrapper>
    {
        private readonly ITokenService _tokenService;
        public GetRefreshTokenQueryHandler(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }
        public async Task<IResponseWrapper> Handle(GetRefreshTokenQuery request, CancellationToken cancellationToken)
        {
            return await _tokenService.GetRefreshTokenAsync(request.RefreshTokenRequest);
        }
    }
}
