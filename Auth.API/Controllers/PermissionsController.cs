using Auth.API.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PermissionsController(AppDbContext context)
            => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetPermissionAsync()
        {
            return Ok(_context.Permissions.ToList());
        }

        //[HttpPost]
        //public async Task<IActionResult> PostPermissionAsync(string name)
        //{
        //    var permission = _context.Permissions.Add(new Permission { Name = name });
        //    await _context.SaveChangesAsync();

        //    return Ok(permission.Entity);
        //}
    }
}
