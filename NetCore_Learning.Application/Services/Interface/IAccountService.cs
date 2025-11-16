using Microsoft.AspNetCore.Mvc;
using NetCore_Learning.Application.Models.DTO;
using NetCore_Learning.Share.Common;

namespace NetCore_Learning.Application.Services.Interface
{
    public interface IAccountService
    {
        Task<ResponseResult<TokenResponseDto>> LoginRequestAsync(AccountDto account);
        Task<ResponseResult<string>> RegisterAccountAsync(AccountDto account);
        Task<ResponseResult<TokenResponseDto>> RefreshTokenAsync(TokenRefreshRequestDto tokenRequest);
        Task<ResponseResult<string>> LogOut(TokenRequestDto tokenRequest);
        
        /// <summary>
        /// Hủy tất cả tokens của một user (tất cả devices)
        /// </summary>
        Task<ResponseResult<string>> RevokeAllUserTokensAsync(string userId);
    }
}
