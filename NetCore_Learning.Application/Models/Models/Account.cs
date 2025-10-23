namespace Net_Learning.Models.Models
{
    public class Account
    {
        public required Guid Id { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
