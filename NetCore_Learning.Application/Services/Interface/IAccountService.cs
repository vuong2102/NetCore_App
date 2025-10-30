using Microsoft.AspNetCore.Mvc;
using NetCore_Learning.Application.Models.DTO;
using NetCore_Learning.Share.Common;

namespace NetCore_Learning.Application.Services.Interface
{
    public interface IAccountService
    {
        Task<ResponseResult<string>> LoginRequestAsync(AccountDto account);
        Task<ResponseResult<string>> RegisterAccountAsync(AccountDto account);
    }
}
