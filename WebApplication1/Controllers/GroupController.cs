using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Context;
using WebApplication1.Models;
using WebApplication1.Models.Update;

namespace WebApplication1.Controllers
{
    [Route("api/group")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GroupController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/group
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Group>>> Get()
        {
            return await _context.Groups
                .Where(g => g.Actived)
                .Include(g => g.Systems)
                .Include(g => g.Permissions)
                .ToListAsync();
        }

        // GET api/group/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Group>> Get(int id)
        {
            var group = await _context.Groups
                .Include(g => g.Systems)
                .Include(g => g.Permissions)
                .FirstOrDefaultAsync(g => g.Id == id && g.Actived);

            if (group == null)
            {
                return NotFound(new ErrorResponse(StatusCodes.Status404NotFound, "Grupo não encontrado ou está inativo."));
            }

            return group;
        }

        // POST api/group
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Group>> Post([FromBody] GroupSave groupSave)
        {
            // Verifica se já existe um grupo com o mesmo nome
            if (await _context.Groups.AnyAsync(g => g.Name == groupSave.Name))
            {
                return BadRequest(new ErrorResponse(StatusCodes.Status400BadRequest, "Já existe um grupo com este nome."));
            }

            // Valida os sistemas fornecidos
            var validSystems = await _context.Systems
                .Where(s => groupSave.Systems.Contains(s.Id) && s.Actived)
                .Select(s => s.Id)
                .ToListAsync();
            var invalidSystems = groupSave.Systems.Except(validSystems).ToList();

            // Valida as permissões fornecidas
            var validPermissions = await _context.Permissions
                .Where(p => groupSave.Permissions.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();
            var invalidPermissions = groupSave.Permissions.Except(validPermissions).ToList();

            if (invalidSystems.Any() || invalidPermissions.Any())
            {
                var errorMessage = new List<string>();
                if (invalidSystems.Any())
                {
                    errorMessage.Add($"Sistemas inválidos: {string.Join(", ", invalidSystems)}.");
                }
                if (invalidPermissions.Any())
                {
                    errorMessage.Add($"Permissões inválidas: {string.Join(", ", invalidPermissions)}.");
                }

                return BadRequest(new ErrorResponse(StatusCodes.Status400BadRequest, string.Join(" ", errorMessage)));
            }

            var group = new Group
            {
                Name = groupSave.Name,
                Description = groupSave.Description,
                Actived = true,
                Systems = await _context.Systems.Where(s => validSystems.Contains(s.Id)).ToListAsync(),
                Permissions = await _context.Permissions.Where(p => validPermissions.Contains(p.Id)).ToListAsync()
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = group.Id }, group);
        }

        // PUT api/group/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Put(int id, [FromBody] GroupSave groupSave)
        {
            var existingGroup = await _context.Groups
                .Include(g => g.Systems)
                .Include(g => g.Permissions)
                .FirstOrDefaultAsync(g => g.Id == id && g.Actived);

            if (existingGroup == null)
            {
                return NotFound(new ErrorResponse(StatusCodes.Status404NotFound, "Grupo não encontrado ou está inativo."));
            }

            // Verifica se o nome já está em uso por outro grupo
            if (await _context.Groups.AnyAsync(g => g.Name == groupSave.Name && g.Id != id))
            {
                return BadRequest(new ErrorResponse(StatusCodes.Status400BadRequest, "Já existe um grupo com este nome."));
            }

            // Valida os sistemas fornecidos
            var validSystems = await _context.Systems
                .Where(s => groupSave.Systems.Contains(s.Id) && s.Actived)
                .Select(s => s.Id)
                .ToListAsync();
            var invalidSystems = groupSave.Systems.Except(validSystems).ToList();

            // Valida as permissões fornecidas
            var validPermissions = await _context.Permissions
                .Where(p => groupSave.Permissions.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();
            var invalidPermissions = groupSave.Permissions.Except(validPermissions).ToList();

            if (invalidSystems.Any() || invalidPermissions.Any())
            {
                var errorMessage = new List<string>();
                if (invalidSystems.Any())
                {
                    errorMessage.Add($"Sistemas inválidos: {string.Join(", ", invalidSystems)}.");
                }
                if (invalidPermissions.Any())
                {
                    errorMessage.Add($"Permissões inválidas: {string.Join(", ", invalidPermissions)}.");
                }

                return BadRequest(new ErrorResponse(StatusCodes.Status400BadRequest, string.Join(" ", errorMessage)));
            }

            existingGroup.Name = groupSave.Name;
            existingGroup.Description = groupSave.Description;
            existingGroup.Systems = await _context.Systems.Where(s => validSystems.Contains(s.Id)).ToListAsync();
            existingGroup.Permissions = await _context.Permissions.Where(p => validPermissions.Contains(p.Id)).ToListAsync();

            _context.Entry(existingGroup).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE api/group/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var group = await _context.Groups
                .Include(g => g.Systems)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
            {
                return NotFound(new ErrorResponse(StatusCodes.Status404NotFound, "Grupo não encontrado."));
            }

            if (group.Systems.Any())
            {
                group.Actived = false; // Marca como inativo
                _context.Entry(group).State = EntityState.Modified;
            }
            else
            {
                _context.Groups.Remove(group); // Remove o grupo se não houver sistemas vinculados
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
