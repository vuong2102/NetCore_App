namespace Net_Learning.Models.Models;

public class UserSessions
{
    public required string UserId { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiredAt { get; set; }
    public bool IsRevoke { get; set; }
    public required HeaderInfo HeaderInfo { get; set; }
}

public class HeaderInfo
{
    public required string DeviceId { get; set; }
    public required string IpAddress { get; set; }
    public required string UserAgent { get; set; }
}