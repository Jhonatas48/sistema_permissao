using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Context;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication1.Models;

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
        public async Task<ActionResult<IEnumerable<WebApplication1.Models.System>>> Get()
        {
            return await _context.Systems.Include(s => s.Groups).ToListAsync();
        }

        // GET api/system/5
        [HttpGet("{id}")]
        public async Task<ActionResult<WebApplication1.Models.System>> Get(int id)
        {
            var system = await _context.Systems.Include(s => s.Groups)
                                               .FirstOrDefaultAsync(s => s.Id == id);

            if (system == null)
            {
                return NotFound();
            }

            return system;
        }

        // POST api/system
        [HttpPost]
        public async Task<ActionResult<WebApplication1.Models.System>> Post([FromBody] WebApplication1.Models.System system)
        {
            _context.Systems.Add(system);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = system.Id }, system);
        }

        // PUT api/system/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] WebApplication1.Models.System updatedSystem)
        {
            if (id != updatedSystem.Id)
            {
                return BadRequest();
            }

            _context.Entry(updatedSystem).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE api/system/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var system = await _context.Systems.FindAsync(id);
            if (system == null)
            {
                return NotFound();
            }

            _context.Systems.Remove(system);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET api/system/5/groups
        [HttpGet("{id}/groups")]
        public async Task<ActionResult<IEnumerable<Group>>> GetSystemGroups(int id)
        {
            var system = await _context.Systems.Include(s => s.Groups).FirstOrDefaultAsync(s => s.Id == id);
            if (system == null)
            {
                return NotFound();
            }

            return Ok(system.Groups);
        }
    }
}
