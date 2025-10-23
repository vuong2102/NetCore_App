using System.ComponentModel.DataAnnotations;

namespace NetCore_Learning.Application.Models.DTO
{
    public class AccountDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [MinLength(6)]
        public required string Password { get; set; }
    }
}
