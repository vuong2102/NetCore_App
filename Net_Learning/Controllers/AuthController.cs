using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Net_Learning.Models.Models;
using NetCore_Learning.Application.Models.DTO;
using NetCore_Learning.Application.Services.Interface;
using NetCore_Learning.Share.Common;

namespace Net_Learning.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion(1.0)]
    public class AuthController : ControllerBase
    {
        private readonly ILifeTimeOfDependencyService _lifeTimeOfDependencyService1;
        private readonly ILifeTimeOfDependencyService _lifeTimeOfDependencyService2;
        private readonly IAccountService _accountService;
        public AuthController(ILifeTimeOfDependencyService lifeTimeOfDependencyService1,
                                ILifeTimeOfDependencyService lifeTimeOfDependencyService2,
                                IAccountService accountService)
        {
            _lifeTimeOfDependencyService1 = lifeTimeOfDependencyService1;
            _lifeTimeOfDependencyService2 = lifeTimeOfDependencyService2;
            _accountService = accountService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ResponseResult<AccountDto>> Login(AccountDto account)
        {
            try
            {
                var result = await _accountService.LoginRequestAsync(account);
                return result;
            }
            catch (Exception ex)
            {
                return new InvalidDataResponseResult<AccountDto>(ex.Message);
            }

        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ResponseResult<string>> Register(AccountDto request)
        {
            try
            {
                var result = await _accountService.RegisterAccountAsync(request);
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
                var result1 = _lifeTimeOfDependencyService1.GetOperationId();
                var result2 = _lifeTimeOfDependencyService2.GetOperationId();
                return Ok(result1 + "\n" + result2);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }
    }
}
