using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NetCore_Learning.Application.Models.DTO;
using NetCore_Learning.Application.Services.Interface;
using NetCore_Learning.Data.Configuration;
using NetCore_Learning.Data.Core;
using NetCore_Learning.Data.Core.YourApp.Core.Interfaces;
using NetCore_Learning.Data.Models;
using NetCore_Learning.Data.Repositories.Interface;
using NetCore_Learning.Share.Common;

namespace NetCore_Learning.Application.Services.Implement
{
    public class AccountService : IAccountService
    {
        readonly IMapper _mapper;
        readonly IUnitOfWork _unitOfWork;
        readonly IUserAccountRepository _userAccountRepository;
        readonly ApplicationDbContext  _context;
        public AccountService(ApplicationDbContext context,
            IUnitOfWork unitOfWork,
            IUserAccountRepository userAccountRepository) 
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _userAccountRepository = userAccountRepository;
        }

        public async Task<ResponseResult<AccountDto>> LoginRequestAsync(AccountDto account)
        {
            await Task.Delay(100);
            string token = "";
            if (account.Email == "admin" && account.Password == "1")
            {
                return new ResponseResult<AccountDto>(null, ResultCode.SuccessResult);
            }
            else
            {
                // Throw UnauthorizedAccessException để GlobalExceptionHandler có thể xử lý
                throw new UnauthorizedAccessException("Tên đăng nhập hoặc mật khẩu không đúng");
            }
        }

        public async Task<ResponseResult<string>> RegisterAccountAsync(AccountDto account)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == RoleEnum.NormalUser.ToString());
                User user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    IsActive = 1
                };

                UserAccount userAccount = _mapper.Map<UserAccount>(account);
                var passwordHasher = new PasswordHasher<UserAccount>();

                userAccount.Id = Guid.NewGuid().ToString();
                userAccount.UserId = user.Id;
                userAccount.Role = role?.Id;
                userAccount.PasswordHash = passwordHasher.HashPassword(userAccount, account.Password);
                userAccount.IsActive = ActiveStatusEnum.Active.ToString();

                await _context.Users.AddAsync(user);
                await _userAccountRepository.AddAsync(userAccount, false);

                await _unitOfWork.CommitTransactionAsync();
                return new ResponseResult<string>("Register successfully", ResultCode.SuccessResult);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new ResponseResult<string>($"Register failed: {ex.Message}", ResultCode.ExceptionResult);
            }
        }
    }
}
