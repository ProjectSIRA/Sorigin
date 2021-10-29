namespace Sorigin.Models
{
    public struct TokenSignContract
    {
        public readonly string jwt;
        public readonly string refreshToken;

        public TokenSignContract(string jwt, string refreshToken)
        {
            this.jwt = jwt;
            this.refreshToken = refreshToken;
        }
    }
}