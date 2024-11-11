using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApplication1.Controllers
{
    [Route("api/permission")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        // GET: api/<PermissionController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<PermissionController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<PermissionController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<PermissionController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<PermissionController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
