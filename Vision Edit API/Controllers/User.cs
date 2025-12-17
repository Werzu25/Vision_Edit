using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using ORM.Services;
using Tools;

namespace Vision_Edit_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class User : ControllerBase
    {
        private readonly UserService _userService;

        public User(DbManager dbManager)
        {
            _userService = new UserService(dbManager); 
        }
        
        // GET: api/<User>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<UserModel> allUsers =  await _userService.GetAllUsers();
            if(allUsers.Count == 0)
                return NoContent();
            return Ok(allUsers);
        }

        // GET api/<User>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<User>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserModel user)
        {
            var users = await _userService.GetAllUsers();
            if (!Validation.ValidUsername(user.Username,users.Select(u => u.Username).ToList()))
                ModelState.AddModelError("Username", "Username already exists");
            if (!Validation.ValidString(user.Username, 3))
                ModelState.AddModelError("Username", "Username must be at least 3 characters long");
            
            
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var response = await _userService.CreateUser(user);
            if (user == null)
                return NoContent();
            return Created(new Uri($"{Request.Scheme}://{Request.Host}{Request.Path}/{response.Id}"), response);
        }

        // PUT api/<User>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<User>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
