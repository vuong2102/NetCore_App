using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Net_Learning.Models.Models;
using NetCore_Learning.Application.Models.DTO;
using NetCore_Learning.Application.Services.Interface;
using NetCore_Learning.Infrastructure.Services.Caching;
using NetCore_Learning.Share.Common;

namespace Net_Learning.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion(1.0)]
    public class AuthController(
        ILifeTimeOfDependencyService lifeTimeOfDependencyService1,
        ILifeTimeOfDependencyService lifeTimeOfDependencyService2,
        IAccountService accountService
        ) : ControllerBase
    {

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ResponseResult<TokenResponseDto>> Login(AccountDto account)
        {
            try
            {
                var result = await accountService.LoginRequestAsync(account);
                return result;
            }
            catch (Exception ex)
            {
                return new InvalidDataResponseResult<TokenResponseDto>(ex.Message);
            }
        }

        [HttpPost("refresh-token")]
        public async Task<ResponseResult<TokenResponseDto>> RefreshToken(TokenRequestDto request)
        {
            try
            {
                var result = await accountService.RefreshTokenAsync(request);
                if (result == null)
                {
                    return new InvalidDataResponseResult<TokenResponseDto>("Invalid refresh token or user ID.");
                }
                return result;
            }
            catch (Exception ex)
            {
                return new InvalidDataResponseResult<TokenResponseDto>(ex.Message);
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ResponseResult<string>> Register(AccountDto request)
        {
            try
            {
                var result = await accountService.RegisterAccountAsync(request);
                return result;
            }
            catch (Exception ex)
            {
                return new InvalidDataResponseResult<string>(ex.Message);
            }
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<ResponseResult<string>> LogOut(TokenRequestDto request)
        {
            try
            {
                var result = await accountService.LogOut(request);
                return result;
            }
            catch (Exception ex)
            {
                return new InvalidDataResponseResult<string>(ex.Message);
            }
        
        }

        [HttpGet("LifeTime-of-dependency_injection")]
        public IActionResult LifeTimeOfDependencyInjection()
        {
            try
            {
                var result1 = lifeTimeOfDependencyService1.GetOperationId();
                var result2 = lifeTimeOfDependencyService2.GetOperationId();
                return Ok(result1 + "\n" + result2);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }
    }
}
