namespace WebAPI.Controllers;

using ApiContracts;
using Entities;
using RepositoryContracts;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository userRepo;
    private readonly IPostRepository postRepo;
    private readonly ICommentRepository commentRepo;

    public UsersController(IUserRepository userRepo, IPostRepository postRepo, ICommentRepository commentRepo)
    {
        this.userRepo = userRepo;
        this.postRepo = postRepo;
        this.commentRepo = commentRepo;
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> AddUser([FromBody] CreateUserDto request)
    {
        bool usernameTaken = userRepo.GetManyAsync().Any(u => u.Username.ToLower() == request.Username.ToLower());
        if (usernameTaken)
        {
            return Conflict($"Username '{request.Username}' is already taken");
        }

        User user = new User { Username = request.Username, Password = request.Password };
        User created = await userRepo.AddAsync(user);

        UserDto dto = new UserDto { Id = created.Id, Username = created.Username };


        return Created($"/users/{dto.Id}", dto);
    }


    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetUserById(int id)
    {
        User user = await userRepo.GetSingleAsync(id);
        UserDto dto = new UserDto { Id = user.Id, Username = user.Username };
        return Ok(dto);
    }

    [HttpGet]
    public ActionResult<List<UserDto>> GetUsers([FromQuery] string? username)
    {
        IQueryable<User> users = userRepo.GetManyAsync();

        if (!string.IsNullOrWhiteSpace(username))
        {
            users = users.Where(u => u.Username.ToLower().Contains(username.ToLower()));
        }

        List<UserDto> dtos = users
            .Select(u => new UserDto { Id = u.Id, Username = u.Username })
            .ToList();
        return Ok(dtos);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserDto request)
    {
        bool usernameTaken = userRepo.GetManyAsync()
            .Any(u => u.Id != id && u.Username.ToLower() == request.Username.ToLower());
        if (usernameTaken)
        {
            return Conflict($"Username '{request.Username}' is already taken");
        }

        User user = new User { Id = id, Username = request.Username, Password = request.Password };
        await userRepo.UpdateAsync(user);
        return Ok(new UserDto { Id = user.Id, Username = user.Username });
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<UserDto>> DeleteUser(int id)
    {
        User user = await userRepo.GetSingleAsync(id);

        // Also delete the user's posts and all comments on them
        List<int> postIds = postRepo.GetManyAsync()
            .Where(p => p.UserId == id)
            .Select(p => p.Id)
            .ToList();
        List<int> commentIds = commentRepo.GetManyAsync()
            .Where(c => c.UserId == id || postIds.Contains(c.PostId))
            .Select(c => c.Id)
            .ToList();

        foreach (int commentId in commentIds) await commentRepo.DeleteAsync(commentId);
        foreach (int postId in postIds) await postRepo.DeleteAsync(postId);
        await userRepo.DeleteAsync(id);

        return Ok(new UserDto { Id = user.Id, Username = user.Username });
    }
}
