using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Context;
using WebApplication1.Models;
using WebApplication1.Models.Update;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/user
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<User>>> Get()
        {
            return await _context.Users.Include(u => u.Groups).ToListAsync();
        }

        // GET api/user/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User>> Get(int id)
        {
            var user = await _context.Users.Include(u => u.Groups)
                                           .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new ErrorResponse(StatusCodes.Status404NotFound, "Usuário não encontrado."));
            }

            return user;
        }

        // POST api/user
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User>> Post([FromBody] UserSave userSave)
        {
            if (await _context.Users.AnyAsync(u => u.Email == userSave.Email))
            {
                return BadRequest(new ErrorResponse(StatusCodes.Status400BadRequest, "O e-mail informado já está em uso."));
            }

            if (userSave.GroupIds.Any())
            {
                var groups = await _context.Groups
                    .Where(g => userSave.GroupIds.Contains(g.Id))
                    .ToListAsync();

                if (groups.Count != userSave.GroupIds.Count)
                {
                    return BadRequest(new ErrorResponse(StatusCodes.Status400BadRequest, "Um ou mais IDs de grupos são inválidos."));
                }
            }

            var user = new User
            {
                Name = userSave.Name,
                Email = userSave.Email,
                Status = true,
                Groups = userSave.GroupIds.Select(id => new Group { Id = id }).ToList()
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
        }

        // PUT api/user/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Put(int id, [FromBody] UserSave updatedUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new ErrorResponse(StatusCodes.Status404NotFound, "Usuário não encontrado."));
            }

            if (await _context.Users.AnyAsync(u => u.Email == updatedUser.Email && u.Id != id))
            {
                return BadRequest(new ErrorResponse(StatusCodes.Status400BadRequest, "O e-mail informado já está em uso por outro usuário."));
            }

            user.Name = updatedUser.Name;
            user.Email = updatedUser.Email;

            if (updatedUser.GroupIds.Any())
            {
                var groups = await _context.Groups
                    .Where(g => updatedUser.GroupIds.Contains(g.Id))
                    .ToListAsync();

                if (groups.Count != updatedUser.GroupIds.Count)
                {
                    return BadRequest(new ErrorResponse(StatusCodes.Status400BadRequest, "Um ou mais IDs de grupos são inválidos."));
                }

                user.Groups = groups;
            }

            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE api/user/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users
                .Include(u => u.Groups)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new ErrorResponse(StatusCodes.Status404NotFound, "Usuário não encontrado."));
            }

            if (user.Groups.Any())
            {
                user.Status = false;
                _context.Entry(user).State = EntityState.Modified;
            }
            else
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
