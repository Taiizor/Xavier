using Microsoft.AspNetCore.Mvc;

namespace Xavier.Sample.ControllersApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private static readonly List<User> Users =
    [
        new User { Id = 1, Name = "Alice" },
        new User { Id = 2, Name = "Bob" }
    ];

    [HttpGet]
    public ActionResult<IEnumerable<User>> GetAll()
    {
        return Ok(Users);
    }

    [HttpGet("{id}")]
    public ActionResult<User> GetById(int id)
    {
        User? user = Users.FirstOrDefault(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpPost]
    public ActionResult<User> Create([FromBody] CreateUserRequest request)
    {
        User user = new()
        {
            Id = Users.Count + 1,
            Name = request.Name
        };
        Users.Add(user);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CreateUserRequest
{
    public string Name { get; set; } = string.Empty;
}
