using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Context;
using WebApplication1.Models;

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
        public async Task<ActionResult<IEnumerable<Group>>> Get()
        {
            return await _context.Groups.Include(g => g.Systems).ToListAsync();
        }

        // GET api/group/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Group>> Get(int id)
        {
            var group = await _context.Groups.Include(g => g.Systems)
                                             .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
            {
                return NotFound();
            }

            return group;
        }

        // POST api/group
        [HttpPost]
        public async Task<ActionResult<Group>> Post([FromBody] Group group)
        {
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = group.Id }, group);
        }

        // PUT api/group/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Group updatedGroup)
        {
            if (id != updatedGroup.Id)
            {
                return BadRequest();
            }

            _context.Entry(updatedGroup).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE api/group/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null)
            {
                return NotFound();
            }

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET api/group/5/systems
        [HttpGet("{id}/systems")]
        public async Task<ActionResult<IEnumerable<WebApplication1.Models.System>>> GetGroupSystems(int id)
        {
            var group = await _context.Groups.Include(g => g.Systems).FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
            {
                return NotFound();
            }

            return Ok(group.Systems);
        }

        // POST api/group/5/systems/1
        [HttpPost("{id}/systems/{systemId}")]
        public async Task<IActionResult> AddGroupSystem(int id, int systemId)
        {
            var group = await _context.Groups.Include(g => g.Systems).FirstOrDefaultAsync(g => g.Id == id);
            var system = await _context.Systems.FindAsync(systemId);

            if (group == null || system == null)
            {
                return NotFound();
            }

            group.Systems.Add(system);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE api/group/5/systems/1
        [HttpDelete("{id}/systems/{systemId}")]
        public async Task<IActionResult> RemoveGroupSystem(int id, int systemId)
        {
            var group = await _context.Groups.Include(g => g.Systems).FirstOrDefaultAsync(g => g.Id == id);
            var system = group?.Systems.FirstOrDefault(s => s.Id == systemId);

            if (group == null || system == null)
            {
                return NotFound();
            }

            group.Systems.Remove(system);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}