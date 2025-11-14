using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore_Learning.Application.Models.DTO
{
    public class TokenResponseDto
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
    }

    public class TokenRequestDto
    {
        public string? UserId { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public string? DeviceId { get; set; } 
    }
}
