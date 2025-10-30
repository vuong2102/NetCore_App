using MapsterMapper;
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
            IMapper mapper,
            IUserAccountRepository userAccountRepository) 
        {
            _context = context;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userAccountRepository = userAccountRepository;
        }

        public async Task<ResponseResult<string>> LoginRequestAsync(AccountDto account)
        {
            try
            {
                // Lấy tài khoản theo Email
                var userAccount = await _context.UserAccounts
                    .Include(x => x.User)
                    .ThenInclude(u => u.Role)
                    .FirstOrDefaultAsync(x => x.Email == account.Email);

                if (userAccount == null)
                    throw new UnauthorizedAccessException("Email không tồn tại");

                // Kiểm tra trạng thái tài khoản
                if (userAccount.IsActive != ActiveStatusEnum.Active.ToString())
                    throw new UnauthorizedAccessException("Tài khoản đã bị khóa hoặc chưa kích hoạt");

                // Kiểm tra mật khẩu
                var passwordHasher = new PasswordHasher<UserAccount>();
                var result = passwordHasher.VerifyHashedPassword(userAccount, userAccount.PasswordHash, account.Password);

                if (result == PasswordVerificationResult.Failed)
                    throw new UnauthorizedAccessException("Mật khẩu không chính xác");

                // Tạo JWT token hoặc token tạm (demo)
                string token = Guid.NewGuid().ToString(); // bạn có thể thay bằng token JWT thực tế

                return new ResponseResult<string>(token, ResultCode.SuccessResult);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Ném lại để GlobalExceptionHandler xử lý
                throw;
            }
            catch (Exception ex)
            {
                return new ResponseResult<string>($"Login failed: {ex.Message}", ResultCode.ExceptionResult);
            }
        }

        public async Task<ResponseResult<string>> RegisterAccountAsync(AccountDto account)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                //Register new account
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == RoleEnum.NormalUser.ToString());
                User user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    RoleId = role?.Id,
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
