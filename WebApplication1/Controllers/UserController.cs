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
            // Inclui os grupos, sistemas e permissões associados
            return await _context.Users
                .Include(u => u.Groups)
                    .ThenInclude(g => g.Systems) // Inclui os sistemas dos grupos
                .Include(u => u.Groups)
                    .ThenInclude(g => g.Permissions) // Inclui as permissões dos grupos
                .ToListAsync();
        }

        // GET api/user/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User>> Get(int id)
        {
            // Inclui os grupos, sistemas e permissões associados
            var user = await _context.Users
                .Include(u => u.Groups)
                    .ThenInclude(g => g.Systems) // Inclui os sistemas dos grupos
                .Include(u => u.Groups)
                    .ThenInclude(g => g.Permissions) // Inclui as permissões dos grupos
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

            var user = new User
            {
                Name = userSave.Name,
                Email = userSave.Email,
                Actived = true,
                Groups = new List<Group>()
            };

            // Verifica os grupos
            if (userSave.GroupIds.Any())
            {

                var groups = await _context.Groups
                    .Where(g => userSave.GroupIds.Contains(g.Id) && g.Actived)
                    .ToListAsync();

                var validGroupIds = groups.Select(g => g.Id).ToList();

                // Identifica os IDs de grupos inválidos
                var invalidGroupIds = userSave.GroupIds.Except(validGroupIds).ToList();
                if (invalidGroupIds.Any())
                {
                    var message = $"Um ou mais IDs de grupos são inválidos ou estão inativos: {string.Join(", ", invalidGroupIds)}.";
                    return BadRequest(new ErrorResponse(StatusCodes.Status400BadRequest, message));
                }

                // Associa os grupos encontrados ao usuário
                user.Groups = groups;
            }

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
            var user = await _context.Users
                .Include(u => u.Groups)
                .FirstOrDefaultAsync(u => u.Id == id);

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

            // Verifica os grupos
            if (updatedUser.GroupIds.Any())
            {
                // Busca os grupos ativos com os IDs fornecidos
                var groups = await _context.Groups
                    .Where(g => updatedUser.GroupIds.Contains(g.Id) && g.Actived)
                    .ToListAsync();

                var validGroupIds = groups.Select(g => g.Id).ToList();

                // Identifica os IDs de grupos inválidos
                var invalidGroupIds = updatedUser.GroupIds.Except(validGroupIds).ToList();
                if (invalidGroupIds.Any())
                {
                    var message = $"Um ou mais IDs de grupos são inválidos ou estão inativos: {string.Join(", ", invalidGroupIds)}.";
                    return BadRequest(new ErrorResponse(StatusCodes.Status400BadRequest, message));
                }

                // Atualiza os grupos associados ao usuário
                user.Groups = groups;
            }
            else
            {
                // Se nenhum grupo for fornecido, remove todas as associações
                user.Groups.Clear();
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
                user.Actived = false;
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
