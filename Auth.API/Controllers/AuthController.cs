using Auth.API.Domain;
using Auth.API.DTOs;
using Auth.API.Entities;
using Auth.API.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly IJWTService _jWTService;

        public AuthController(AppDbContext appDbContext, IJWTService jWTService)
        {
            _appDbContext = appDbContext;
            _jWTService = jWTService;
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            var refreshToken = Guid.NewGuid().ToString("n");
            var expiredDate = DateTime.Now.AddDays(7);

            var user = new User()
            {
                Name = registerDTO.Name,
                Email = registerDTO.Email,
                RefreshToken = refreshToken,
                PasswordHash = registerDTO.Password,
                RoleId = registerDTO.RoleId,
                ExpiredDate = expiredDate,
            };

            var entry = await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            user = entry.Entity;
            user.Role = _appDbContext.Roles.Include(x => x.Permissions).FirstOrDefault(x => x.Id == registerDTO.RoleId);

            var accesstoken = _jWTService.GenerateToken(user);

            return Ok(new TokenDTO()
            {
                AccessToken = accesstoken,
                RefreshToken = refreshToken,
                ExpiredDate = expiredDate
            });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            var storageUser = _appDbContext.Users
                .Include(x => x.Role)
                .ThenInclude(x => x.Permissions)
                .FirstOrDefault(x => x.Email == loginDTO.Email && x.PasswordHash == loginDTO.Password);

            if (storageUser == null) { }

            var accessToken = _jWTService.GenerateToken(storageUser);

            return Ok(new TokenDTO()
            {
                AccessToken = accessToken,
                RefreshToken = storageUser.RefreshToken,
                ExpiredDate = storageUser.ExpiredDate,
            });
        }

        [HttpPost]
        public async Task<IActionResult> RefreshToken(TokenDTO tokenDTO)
        {
            return Ok(ValidateToken(tokenDTO));
        }

        private TokenDTO ValidateToken(TokenDTO tokenDTO)
        {
            var tvp = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuer = "issuer",
                ValidateAudience = true,
                ValidAudience = "audience",
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("bu-menig-keyim-8-mart-bilan-mustaqillik bayamiyam"))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var claimPrinciple = tokenHandler.ValidateToken(tokenDTO.AccessToken, tvp, out SecurityToken newToken);

            var id = claimPrinciple.FindFirst(ClaimTypes.NameIdentifier).Value;

            var user = _appDbContext.Users
               .Include(x => x.Role)
               .ThenInclude(x => x.Permissions)
               .FirstOrDefault(x => x.Id.ToString() == id);

            if (user.RefreshToken != tokenDTO.RefreshToken)
            {
                throw new Exception("Token unavailable")!;
            }

            var accessToken = _jWTService.GenerateToken(user);

            return new TokenDTO()
            {
                AccessToken = accessToken,
                RefreshToken = tokenDTO.RefreshToken,
                ExpiredDate = DateTime.Now.AddDays(7),
            };
        }
    }
}
