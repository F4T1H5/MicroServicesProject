using CORE.APP.Models.Authentication;
using System.Security.Claims;

namespace CORE.APP.Services.Authentication
{
    public interface ITokenAuthService
    {
        public TokenResponse GetTokenResponse(int userId, string userName, string[] userRoleNames, DateTime expiration,
            string securityKey, string issuer, string audience, string refreshToken); // public may not be written

        public string GetRefreshToken();

        public IEnumerable<Claim> GetClaims(string token, string securityKey);
    }
}