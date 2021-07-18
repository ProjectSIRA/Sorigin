namespace Sorigin.Authorization
{
    public interface IPasswordHasher
    {
        string Hash(string rawPassword);
        bool Verify(string rawPassword, string hash);
    }
}