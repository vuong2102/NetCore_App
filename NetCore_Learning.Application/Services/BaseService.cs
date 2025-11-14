using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Net_Learning.Models.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NetCore_Learning.Application.Services
{
    public abstract class BaseService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        protected BaseService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            Configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        protected IConfiguration Configuration { get; }

        public ClaimsPrincipal? GetUserPrincipal(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            var secretKey = Configuration["JWT:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                return null;
            }

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
                if (securityToken is JwtSecurityToken jwtToken &&
                    jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.OrdinalIgnoreCase))
                {
                    return principal;
                }
            }
            catch
            {
                // ignored - return null on validation failure
            }

            return null;
        }

        public HeaderInfo GetHeaderInfo()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            HeaderInfo headerInfo = new HeaderInfo
            {
                DeviceId = httpContext?.Items["DeviceId"]?.ToString(),
                UserAgent = httpContext?.Items["UserAgent"]?.ToString(),
                IpAddress = httpContext?.Items["IpAddress"]?.ToString()
            };
            return headerInfo;
        }
    }
}
