using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NetCore_Learning.Application.Models.DTO;
using NetCore_Learning.Application.Services.Interface;
using NetCore_Learning.Data.Configuration;
using NetCore_Learning.Data.Core;
using NetCore_Learning.Data.Core.YourApp.Core.Interfaces;
using NetCore_Learning.Data.Models;
using NetCore_Learning.Data.Repositories.Interface;
using NetCore_Learning.Share.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace NetCore_Learning.Application.Services.Implement
{
    public class AccountService(
        ApplicationDbContext context,
        IMapper _mapper,
        IUnitOfWork _unitOfWork,
        IConfiguration _configuration,
        IUserAccountRepository _userAccountRepository
        ) : IAccountService
    {
        readonly ApplicationDbContext  _context = context;

        public async Task<ResponseResult<TokenResponseDto>> LoginRequestAsync(AccountDto account)
        {
            try
            {
                // Get user by email
                var userAccount = await _context.UserAccounts
                    .Include(x => x.User)
                    .ThenInclude(u => u.Role)
                    .FirstOrDefaultAsync(x => x.Email == account.Email);

                if (userAccount == null)
                    throw new UnauthorizedAccessException("Email không tồn tại");

                // Check if account is active
                if (userAccount.IsActive != ActiveStatusEnum.Active.ToString())
                    throw new UnauthorizedAccessException("Tài khoản đã bị khóa hoặc chưa kích hoạt");

                // Verify password
                var passwordHasher = new PasswordHasher<UserAccount>();
                var result = passwordHasher.VerifyHashedPassword(userAccount, userAccount.PasswordHash, account.Password);

                if (result == PasswordVerificationResult.Failed)
                    throw new UnauthorizedAccessException("Mật khẩu không chính xác");

                // Create token response
                var tokenResponse = await CreateTokenResponseAsync(userAccount);
                return new ResponseResult<TokenResponseDto>(tokenResponse, ResultCode.SuccessResult);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Ném lại để GlobalExceptionHandler xử lý
                throw;
            }
            catch (Exception ex)
            {
                return new ResponseResult<TokenResponseDto>(null, ResultCode.ExceptionResult, $"Login failed: {ex.Message}!!!");
            }
        }

        private string CreateJwtToken(UserAccount userAccount)
        {
            //Payload
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userAccount.UserId),
                new Claim(ClaimTypes.Email, userAccount.Email),
                new Claim(ClaimTypes.Role, userAccount.Role)
            };

            //Signature
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512); //Header

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
                );
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(UserAccount userAccount)
        {
            var refreshToken = GenerateRefreshToken();
            userAccount.RefreshToken = refreshToken;
            userAccount.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();
            return refreshToken;
        }

        public async Task<ResponseResult<TokenResponseDto>> RefreshTokenAsync(TokenRequestDto tokenRequest)
        {
            try
            {
                var user = await ValidateRefreshTokenAsync(tokenRequest.UserId, tokenRequest.RefreshToken);
                if (user == null)
                {
                    return new ResponseResult<TokenResponseDto>(null, ResultCode.InvalidDataResult, "Invalid refresh token or user ID");
                }
                var userAccount = await _context.UserAccounts.FirstOrDefaultAsync(ua => ua.UserId == user.Id);
                if (userAccount == null)
                {
                    return new ResponseResult<TokenResponseDto>(null, ResultCode.InvalidDataResult, "User account not found");
                }
                var tokenResponse = await CreateTokenResponseAsync(userAccount);
                return new ResponseResult<TokenResponseDto>(tokenResponse, ResultCode.SuccessResult);
            }
            catch (Exception ex)
            {
                return new ResponseResult<TokenResponseDto>(null, ResultCode.ExceptionResult, $"Refresh token failed: {ex.Message}");
            }
        }

        public async Task<TokenResponseDto> CreateTokenResponseAsync(UserAccount userAccount)
        {
            var newAccessToken = CreateJwtToken(userAccount);
            var newRefreshToken = await GenerateAndSaveRefreshTokenAsync(userAccount);
            var response = new TokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
            return response;
        }

        private async Task<User?> ValidateRefreshTokenAsync(string userId, string refreshToken)
        {
            var userAccount = await _context.UserAccounts
                .Include(ua => ua.User)
                .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.RefreshToken == refreshToken);
            if (userAccount == null || userAccount.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return null;
            }
            return userAccount.User;
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
