using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NetCore_Learning.Application.Models.DTO;
using NetCore_Learning.Application.Services.Interface;
using NetCore_Learning.Data.Configuration;
using NetCore_Learning.Data.Core.YourApp.Core.Interfaces;
using NetCore_Learning.Data.Models;
using NetCore_Learning.Data.Repositories.Interface;
using NetCore_Learning.Share.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Net_Learning.Models.Models;
using NetCore_Learning.Infrastructure.Services.Caching;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using NetCore_Learning.Share.Helper;

namespace NetCore_Learning.Application.Services.Implement
{
    public class AccountService : BaseService, IAccountService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IRedisCacheService _redisCacheService;

        public AccountService(
            ApplicationDbContext context,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IUserAccountRepository userAccountRepository,
            IRedisCacheService redisCacheService) : base(configuration, httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userAccountRepository = userAccountRepository;
            _redisCacheService = redisCacheService;
        }

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
                var headerInfo = GetHeaderInfo();
                var tokenResponse = await CreateTokenResponseAsync(userAccount, headerInfo, true);
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
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512); //Header

            var tokenDescriptor = new JwtSecurityToken(
                issuer: Configuration["JWT:ValidIssuer"],
                audience: Configuration["JWT:ValidAudience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(Configuration["JWT:TokenvalidityInMinutes"])),
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

        public async Task<ResponseResult<TokenResponseDto>> RefreshTokenAsync(TokenRefreshRequestDto tokenRefreshRequest)
        {
            try
            {
                var headerInfo = GetHeaderInfo();

                // Lấy session Redis
                var redisKey = $"{RedisCachingOptionsEnum.Token}_{tokenRefreshRequest.UserId}_{RedisCachingOptionsEnum.Device}_{headerInfo.DeviceId}";
                var data = await _redisCacheService.GetDataAsync<byte[]>(redisKey);
                if (data == null)
                    return new ResponseResult<TokenResponseDto>(null, ResultCode.InvalidDataResult, "Session not found or expired");

                var session = JsonConvert.DeserializeObject<UserSessions>(Encoding.UTF8.GetString(data))!;

                // Validate refresh token
                if (session.RefreshToken != tokenRefreshRequest.RefreshToken)
                    return new ResponseResult<TokenResponseDto>(null, ResultCode.InvalidDataResult, "Invalid refresh token");
                if (session.IsRevoke || session.ExpiredAt < DateTime.UtcNow)
                    return new ResponseResult<TokenResponseDto>(null, ResultCode.InvalidDataResult, "Refresh token revoked or expired");


                var userAccount = await _context.UserAccounts.FirstOrDefaultAsync(ua => ua.UserId == tokenRefreshRequest.UserId);
                if (userAccount == null)
                {
                    return new ResponseResult<TokenResponseDto>(null, ResultCode.InvalidDataResult, "User account not found");
                }
                var tokenResponse = await CreateTokenResponseAsync(userAccount, headerInfo);
                return new ResponseResult<TokenResponseDto>(tokenResponse, ResultCode.SuccessResult);
            }
            catch (Exception ex)
            {
                return new ResponseResult<TokenResponseDto>(null, ResultCode.ExceptionResult, $"Refresh token failed: {ex.Message}");
            }
        }

        public async Task<TokenResponseDto> CreateTokenResponseAsync(UserAccount userAccount, HeaderInfo headerInfo, bool isGetNewRefreshToken = false)
        {
            var newAccessToken = CreateJwtToken(userAccount);
            var newRefreshToken = isGetNewRefreshToken ? GenerateRefreshToken() : userAccount.RefreshToken;

            // Save to Caching
            var userSessionCachingKey = $"{RedisCachingOptionsEnum.Token.ToString()}_{userAccount.UserId}_{RedisCachingOptionsEnum.Device.ToString()}_{headerInfo.DeviceId}";
            // Set data to Caching
            var userSessions = new UserSessions()
            {
                UserId = userAccount.UserId,
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddDays(int.Parse(Configuration["JWT:RefreshTokenValidityInDays"])),
                IsRevoke = false,
                HeaderInfo = headerInfo
            };
            var dataCacheJson = JsonConvert.SerializeObject(userSessions);
            var dataToCache = Encoding.UTF8.GetBytes(dataCacheJson);
            await _redisCacheService.SetDataAsync(userSessionCachingKey, dataToCache, (int.Parse(Configuration["JWT:RefreshTokenValidityInDays"]) * 24 * 60));
            
            var response = new TokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
            return response;
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
        
        public async Task<ResponseResult<string>> LogOut(TokenRequestDto tokenRequest)
        {
            var principal = GetUserPrincipal(tokenRequest.Token);
            var userId = principal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return new ResponseResult<string>(null, ResultCode.InvalidDataResult, "Invalid token");

            var headerInfo = GetHeaderInfo();

            var userAccount = await _context.UserAccounts.FirstOrDefaultAsync(ua => ua.UserId == userId);
            if (userAccount == null)
            {
                return new ResponseResult<string>(null, ResultCode.InvalidDataResult, "User account not found");
            }

            // Xóa session trong Redis (blacklist token cho device này)
            var redisKey = $"{RedisCachingOptionsEnum.Token}_{userAccount.UserId}_{RedisCachingOptionsEnum.Device}_{headerInfo.DeviceId}";
            await _redisCacheService.RemoveDataAsync(redisKey);

            // Thêm access token vào blacklist
            if (!string.IsNullOrWhiteSpace(tokenRequest.Token))
            {
                await AddTokenToBlacklistAsync(tokenRequest.Token);
            }

            return new ResponseResult<string>("", ResultCode.SuccessResult);
        }

        public async Task<ResponseResult<string>> RevokeAllUserTokensAsync(string userId)
        {
            try
            {
                // Kiểm tra user có tồn tại không
                var userAccount = await _context.UserAccounts.FirstOrDefaultAsync(ua => ua.UserId == userId);
                if (userAccount == null)
                {
                    return new ResponseResult<string>(null, ResultCode.InvalidDataResult, "User not found");
                }

                // Tìm tất cả session keys của user theo pattern: Token_{userId}_*
                var sessionPattern = $"{RedisCachingOptionsEnum.Token}_{userId}_*";
                var sessionKeys = await _redisCacheService.GetKeysByPatternAsync(sessionPattern);

                if (sessionKeys.Count == 0)
                {
                    return new ResponseResult<string>("No active sessions found", ResultCode.SuccessResult);
                }

                var tokensToBlacklist = new List<string>();
                var tokenHandler = new JwtSecurityTokenHandler();

                // Lấy tất cả access tokens từ sessions và chuẩn bị blacklist
                foreach (var sessionKey in sessionKeys)
                {
                    try
                    {
                        var sessionData = await _redisCacheService.GetDataAsync<byte[]>(sessionKey);
                        if (sessionData != null && sessionData.Length > 0)
                        {
                            var sessionJson = Encoding.UTF8.GetString(sessionData);
                            var session = JsonConvert.DeserializeObject<UserSessions>(sessionJson);
                            
                            if (session != null && !string.IsNullOrWhiteSpace(session.Token))
                            {
                                // Kiểm tra token còn hợp lệ không
                                if (tokenHandler.CanReadToken(session.Token))
                                {
                                    var jwtToken = tokenHandler.ReadJwtToken(session.Token);
                                    if (jwtToken.ValidTo > DateTime.UtcNow)
                                    {
                                        tokensToBlacklist.Add(session.Token);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log lỗi nhưng tiếp tục xử lý các session khác
                        System.Diagnostics.Debug.WriteLine($"Error processing session {sessionKey}: {ex.Message}");
                    }
                }

                // Blacklist tất cả tokens
                foreach (var token in tokensToBlacklist)
                {
                    await AddTokenToBlacklistAsync(token);
                }

                // Xóa tất cả session keys
                await _redisCacheService.RemoveMultipleKeysAsync(sessionKeys);

                return new ResponseResult<string>(
                    $"Successfully revoked {sessionKeys.Count} session(s) and blacklisted {tokensToBlacklist.Count} token(s)",
                    ResultCode.SuccessResult);
            }
            catch (Exception ex)
            {
                return new ResponseResult<string>(null, ResultCode.ExceptionResult, $"Failed to revoke tokens: {ex.Message}");
            }
        }

        private async Task AddTokenToBlacklistAsync(string token)
        {
            try
            {
                // Parse JWT để lấy expiration time
                var tokenHandler = new JwtSecurityTokenHandler();
                if (!tokenHandler.CanReadToken(token))
                    return;

                var jwtToken = tokenHandler.ReadJwtToken(token);
                var expirationTime = jwtToken.ValidTo;

                // Nếu token đã hết hạn, không cần blacklist
                if (expirationTime <= DateTime.UtcNow)
                    return;

                // Hash token để làm key (vì token có thể rất dài)
                var tokenHash = HashHelper.HashToken(token);
                var blacklistKey = $"{RedisCachingOptionsEnum.BlacklistToken}_{tokenHash}";

                // Tính TTL = thời gian còn lại của token (tính bằng phút)
                var ttlMinutes = (int)(expirationTime - DateTime.UtcNow).TotalMinutes;
                if (ttlMinutes > 0)
                {
                    // Lưu vào Redis với TTL = thời gian còn lại của token
                    await _redisCacheService.SetDataAsync(blacklistKey, Encoding.UTF8.GetBytes("blacklisted"), ttlMinutes);
                }
            }
            catch
            {
                // Ignore errors khi blacklist token
            }
        }
    }
}
