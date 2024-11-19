using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Context;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication1.Models;
using WebApplication1.Models.Update;

namespace WebApplication1.Controllers
{
    [Route("api/system")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SystemController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/system
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<WebApplication1.Models.System>>> Get()
        {
            return await _context.Systems
                .Where(s => s.Actived)
                .Include(s => s.Groups)
                .ToListAsync();
        }

        // GET api/system/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<WebApplication1.Models.System>> Get(int id)
        {
            var system = await _context.Systems
                .Include(s => s.Groups)
                .FirstOrDefaultAsync(s => s.Id == id && s.Actived);

            if (system == null)
            {
                return NotFound(new ErrorResponse(StatusCodes.Status404NotFound, "Sistema não encontrado ou está inativo."));
            }

            return system;
        }

        // POST api/system
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<WebApplication1.Models.System>> Post([FromBody] SystemSave systemSave)
        {
            if (await _context.Systems.AnyAsync(s => s.Name == systemSave.Name))
            {
                return BadRequest(new ErrorResponse(StatusCodes.Status400BadRequest, "Já existe um sistema com este nome."));
            }

            var system = new WebApplication1.Models.System
            {
                Name = systemSave.Name,
                Actived = true
            };

            _context.Systems.Add(system);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = system.Id }, system);
        }

        // PUT api/system/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Put(int id, [FromBody] SystemSave systemSave)
        {
            var existingSystem = await _context.Systems.FirstOrDefaultAsync(s => s.Id == id && s.Actived);
            if (existingSystem == null)
            {
                return NotFound(new ErrorResponse(StatusCodes.Status404NotFound, "Sistema não encontrado ou está inativo."));
            }

            if (await _context.Systems.AnyAsync(s => s.Name == systemSave.Name && s.Id != id))
            {
                return BadRequest(new ErrorResponse(StatusCodes.Status400BadRequest, "Já existe um sistema com este nome."));
            }

            existingSystem.Name = systemSave.Name;
            _context.Entry(existingSystem).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE api/system/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var system = await _context.Systems
                .Include(s => s.Groups)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (system == null)
            {
                return NotFound(new ErrorResponse(StatusCodes.Status404NotFound, "Sistema não encontrado."));
            }

            if (system.Groups.Any())
            {
                system.Actived = false;
                _context.Entry(system).State = EntityState.Modified;
            }
            else
            {
                _context.Systems.Remove(system);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET api/system/5/groups
        [HttpGet("{id}/groups")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Group>>> GetSystemGroups(int id)
        {
            var system = await _context.Systems
                .Include(s => s.Groups)
                .FirstOrDefaultAsync(s => s.Id == id && s.Actived);

            if (system == null)
            {
                return NotFound(new ErrorResponse(StatusCodes.Status404NotFound, "Sistema não encontrado ou está inativo."));
            }

            return Ok(system.Groups);
        }
    }
}
