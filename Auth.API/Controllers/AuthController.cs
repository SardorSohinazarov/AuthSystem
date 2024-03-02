using Auth.API.Domain;
using Auth.API.DTOs;
using Auth.API.Entities;
using Auth.API.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var user = new User()
            {
                Name = registerDTO.Name,
                Email = registerDTO.Email,
                PasswordHash = registerDTO.Password,
                RoleId = registerDTO.RoleId,
            };

            var entry = await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            user = entry.Entity;
            user.Role = _appDbContext.Roles.Include(x => x.Permissions).FirstOrDefault(x => x.Id == registerDTO.RoleId);

            var token = _jWTService.GenerateToken(user);

            return Ok(token);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            var storageUser = _appDbContext.Users
                .Include(x => x.Role)
                .ThenInclude(x => x.Permissions)
                .FirstOrDefault(x => x.Email == loginDTO.Email && x.PasswordHash == loginDTO.Password);

            if (storageUser == null) { }

            var token = _jWTService.GenerateToken(storageUser);

            return Ok(token);
        }
    }
}
