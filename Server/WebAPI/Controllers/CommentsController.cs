namespace WebAPI.Controllers;

using ApiContracts;
using Entities;
using RepositoryContracts;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentRepository commentRepo;
    private readonly IUserRepository userRepo;
    private readonly IPostRepository postRepo;

    public CommentsController(ICommentRepository commentRepo, IUserRepository userRepo, IPostRepository postRepo)
    {
        this.commentRepo = commentRepo;
        this.userRepo = userRepo;
        this.postRepo = postRepo;
    }

    [HttpPost]
    public async Task<ActionResult<CommentDto>> AddComment([FromBody] CreateCommentDto request)
    {
        User? author = userRepo.GetManyAsync().SingleOrDefault(u => u.Id == request.UserId);
        if (author is null)
        {
            return BadRequest($"User with id {request.UserId} does not exist");
        }

        bool postExists = postRepo.GetManyAsync().Any(p => p.Id == request.PostId);
        if (!postExists)
        {
            return BadRequest($"Post with id {request.PostId} does not exist");
        }

        Comment comment = new Comment { Body = request.Body, PostId = request.PostId, UserId = request.UserId };
        Comment created = await commentRepo.AddAsync(comment);

        CommentDto dto = ToDto(created, author.Username);
        return Created($"/comments/{dto.Id}", dto);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CommentDto>> GetCommentById(int id)
    {
        Comment comment = await commentRepo.GetSingleAsync(id);
        return Ok(ToDto(comment, GetUsername(comment.UserId)));
    }

    [HttpGet]
    public ActionResult<List<CommentDto>> GetComments(
        [FromQuery] int? userId,
        [FromQuery] string? username,
        [FromQuery] int? postId)
    {
        Dictionary<int, string> usernames = userRepo.GetManyAsync().ToDictionary(u => u.Id, u => u.Username);
        IQueryable<Comment> comments = commentRepo.GetManyAsync();

        if (userId is not null)
        {
            comments = comments.Where(c => c.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(username))
        {
            comments = comments.Where(c => usernames.ContainsKey(c.UserId)
                                           && usernames[c.UserId].ToLower() == username.ToLower());
        }

        if (postId is not null)
        {
            comments = comments.Where(c => c.PostId == postId);
        }

        List<CommentDto> dtos = comments
            .Select(c => ToDto(c, usernames.GetValueOrDefault(c.UserId, "")))
            .ToList();
        return Ok(dtos);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CommentDto>> UpdateComment(int id, [FromBody] UpdateCommentDto request)
    {
        Comment comment = await commentRepo.GetSingleAsync(id);
        comment.Body = request.Body;
        await commentRepo.UpdateAsync(comment);
        return Ok(ToDto(comment, GetUsername(comment.UserId)));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<CommentDto>> DeleteComment(int id)
    {
        Comment comment = await commentRepo.GetSingleAsync(id);
        await commentRepo.DeleteAsync(id);
        return Ok(ToDto(comment, GetUsername(comment.UserId)));
    }

    private string GetUsername(int userId)
    {
        return userRepo.GetManyAsync().SingleOrDefault(u => u.Id == userId)?.Username ?? "";
    }

    private static CommentDto ToDto(Comment comment, string authorUsername)
    {
        return new CommentDto
        {
            Id = comment.Id,
            Body = comment.Body,
            PostId = comment.PostId,
            UserId = comment.UserId,
            AuthorUsername = authorUsername
        };
    }
}
