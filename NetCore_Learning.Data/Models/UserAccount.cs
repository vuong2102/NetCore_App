using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore_Learning.Data.Models
{
    public class UserAccount : BaseEntity
    {
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public string IsActive { get; set; }
    }
}
