namespace WebAPI.Controllers;

using ApiContracts;
using Entities;
using RepositoryContracts;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostRepository postRepo;
    private readonly IUserRepository userRepo;
    private readonly ICommentRepository commentRepo;

    public PostsController(IPostRepository postRepo, IUserRepository userRepo, ICommentRepository commentRepo)
    {
        this.postRepo = postRepo;
        this.userRepo = userRepo;
        this.commentRepo = commentRepo;
    }

    [HttpPost]
    public async Task<ActionResult<PostDto>> AddPost([FromBody] CreatePostDto request)
    {
        User? author = userRepo.GetManyAsync().SingleOrDefault(u => u.Id == request.UserId);
        if (author is null)
        {
            return BadRequest($"User with id {request.UserId} does not exist");
        }

        Post post = new Post { Title = request.Title, Body = request.Body, UserId = request.UserId };
        Post created = await postRepo.AddAsync(post);

        PostDto dto = ToDto(created, author.Username);
        return Created($"/posts/{dto.Id}", dto);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostDto>> GetPostById(int id)
    {
        Post post = await postRepo.GetSingleAsync(id);
        return Ok(ToDto(post, GetUsername(post.UserId)));
    }

    [HttpGet]
    public ActionResult<List<PostDto>> GetPosts(
        [FromQuery] string? title,
        [FromQuery] int? userId,
        [FromQuery] string? username)
    {
        Dictionary<int, string> usernames = userRepo.GetManyAsync().ToDictionary(u => u.Id, u => u.Username);
        IQueryable<Post> posts = postRepo.GetManyAsync();

        if (!string.IsNullOrWhiteSpace(title))
        {
            posts = posts.Where(p => p.Title.ToLower().Contains(title.ToLower()));
        }

        if (userId is not null)
        {
            posts = posts.Where(p => p.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(username))
        {
            posts = posts.Where(p => usernames.ContainsKey(p.UserId)
                                     && usernames[p.UserId].ToLower() == username.ToLower());
        }

        List<PostDto> dtos = posts
            .Select(p => ToDto(p, usernames.GetValueOrDefault(p.UserId, "")))
            .ToList();
        return Ok(dtos);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PostDto>> UpdatePost(int id, [FromBody] UpdatePostDto request)
    {
        Post post = await postRepo.GetSingleAsync(id);
        post.Title = request.Title;
        post.Body = request.Body;
        await postRepo.UpdateAsync(post);
        return Ok(ToDto(post, GetUsername(post.UserId)));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<PostDto>> DeletePost(int id)
    {
        Post post = await postRepo.GetSingleAsync(id);

        // Also delete all comments on the post
        List<int> commentIds = commentRepo.GetManyAsync()
            .Where(c => c.PostId == id)
            .Select(c => c.Id)
            .ToList();
        foreach (int commentId in commentIds) await commentRepo.DeleteAsync(commentId);

        await postRepo.DeleteAsync(id);
        return Ok(ToDto(post, GetUsername(post.UserId)));
    }

    private string GetUsername(int userId)
    {
        return userRepo.GetManyAsync().SingleOrDefault(u => u.Id == userId)?.Username ?? "";
    }

    private static PostDto ToDto(Post post, string authorUsername)
    {
        return new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Body = post.Body,
            UserId = post.UserId,
            AuthorUsername = authorUsername
        };
    }
}
