using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Context;
using WebApplication1.Models;
using WebApplication1.Models.Update;

namespace WebApplication1.Controllers
{
    [Route("api/permission")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PermissionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/permission
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Permission>>> Get()
        {
            return await _context.Permissions.ToListAsync();
        }

        // GET api/permission/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Permission>> Get(int id)
        {
            var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Id == id);

            if (permission == null)
            {
                return NotFound(new ErrorResponse(StatusCodes.Status404NotFound, "Permissão não encontrada."));
            }

            return permission;
        }

        // POST api/permission
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Permission>> Post([FromBody] PermissionSave permissionSave)
        {
            // Verifica se já existe uma permissão com o mesmo nome
            if (await _context.Permissions.AnyAsync(p => p.Name == permissionSave.Name))
            {
                return BadRequest(new ErrorResponse(StatusCodes.Status400BadRequest, "Já existe uma permissão com este nome."));
            }

            var permission = new Permission
            {
                Name = permissionSave.Name,
                Description = permissionSave.Description
            };

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = permission.Id }, permission);
        }

        // PUT api/permission/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Put(int id, [FromBody] PermissionSave permissionSave)
        {
            var existingPermission = await _context.Permissions.FindAsync(id);

            if (existingPermission == null)
            {
                return NotFound(new ErrorResponse(StatusCodes.Status404NotFound, "Permissão não encontrada."));
            }

            // Verifica se o nome já está em uso por outra permissão
            if (await _context.Permissions.AnyAsync(p => p.Name == permissionSave.Name && p.Id != id))
            {
                return BadRequest(new ErrorResponse(StatusCodes.Status400BadRequest, "Já existe uma permissão com este nome."));
            }

            existingPermission.Name = permissionSave.Name;
            existingPermission.Description = permissionSave.Description;

            _context.Entry(existingPermission).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE api/permission/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);

            if (permission == null)
            {
                return NotFound(new ErrorResponse(StatusCodes.Status404NotFound, "Permissão não encontrada."));
            }

            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
