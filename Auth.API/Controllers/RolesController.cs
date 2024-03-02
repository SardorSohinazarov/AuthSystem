using Auth.API.Domain;
using Auth.API.DTOs;
using Auth.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RolesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetRolesAsync()
        {
            return Ok(_context.Roles.Include(x => x.Permissions).ToList());
        }

        [HttpPost]
        public async Task<IActionResult> PostRoleAsync(RoleDTO roleDTO)
        {
            var entry = _context.Roles.Add(new Role { Name = roleDTO.Name });
            await _context.SaveChangesAsync();

            foreach (var p in roleDTO.Permissions)
            {
                _context.RolePermissions.Add(new RolePermission()
                {
                    RoleId = entry.Entity.Id,
                    PermissionId = p
                });
                await _context.SaveChangesAsync();
            }

            return Ok(entry.Entity);
        }
    }
}
