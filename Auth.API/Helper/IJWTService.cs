using Auth.API.Entities;

namespace Auth.API.Helper
{
    public interface IJWTService
    {
        public string GenerateToken(User user);
    }
}
