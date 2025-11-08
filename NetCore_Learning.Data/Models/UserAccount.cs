using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore_Learning.Data.Models
{
    public class UserAccount : BaseEntity
    {
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public string? PhoneNumber { get; set; }
        public required string Role { get; set; }
        public required string UserId { get; set; }
        public required User User { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public required string IsActive { get; set; }
    }
}
