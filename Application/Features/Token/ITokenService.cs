using ResponseResult.Models.Requests.Identity;
using ResponseResult.Wrappers;

namespace Application.Features.Token
{
    public interface ITokenService
    {
        Task<IResponseWrapper> GetTokenAsync(TokenRequest tokenRequest);
        Task<IResponseWrapper> GetRefreshTokenAsync(RefreshTokenRequest refreshTokenRequest);
    }
}
