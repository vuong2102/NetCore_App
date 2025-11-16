namespace NetCore_Learning.Application.Services.Interface
{
    public interface IPasswordService
    {
        string HashPassword(string password);
        bool VerifyPassword(string hashedPassword, string password);
    }
}