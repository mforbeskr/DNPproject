using System.Text.Json;
using Entities;
using RepositoryContracts;

namespace FileRepositories;

public class CommentFileRepository : ICommentRepository
{
    private readonly string filePath;

    public CommentFileRepository()
    {
        filePath = Path.Combine(DataDirectory.Path, "comments.json");

        if (!File.Exists(filePath))
        {
            File.Copy(Path.Combine(AppContext.BaseDirectory, "SeedData", "comments.json"), filePath);
        }
    }

    private async Task<List<Comment>> LoadComments()
    {
        string json = await File.ReadAllTextAsync(filePath);

        return JsonSerializer.Deserialize<List<Comment>>(json)
            ?? new List<Comment>();
    }

    private async Task SaveComments(List<Comment> comments)
    {
        string json = JsonSerializer.Serialize(comments);

        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<Comment> AddAsync(Comment comment)
    {
        List<Comment> comments = await LoadComments();

        int maxId = comments.Count > 0 ? comments.Max(u => u.Id) : 0;

        comment.Id = maxId + 1;

        comments.Add(comment);

        await SaveComments(comments);

        return comment;
    }

    public async Task UpdateAsync(Comment comment)
    {
        List<Comment> comments = await LoadComments();

        Comment? existingComment = comments.SingleOrDefault(u => u.Id == comment.Id);

        if (existingComment is null)
        {
            throw new InvalidOperationException($"Comment with id {comment.Id} not found.");
        }

        comments.Remove(existingComment);
        comments.Add(comment);

        await SaveComments(comments);
    }

    public async Task DeleteAsync(int id)
    {
        List<Comment> comments = await LoadComments();

        Comment? commentToRemove = comments.SingleOrDefault(u => u.Id == id);
        if (commentToRemove is null)
        {
            throw new InvalidOperationException($"Comment with id {id} not found.");
        }

        comments.Remove(commentToRemove);

        await SaveComments(comments);
    }

    public async Task<Comment> GetSingleAsync(int id)
    {
        List<Comment> comments = await LoadComments();

        Comment? comment = comments.SingleOrDefault(u => u.Id == id);

        if (comment is null)
        {
            throw new InvalidOperationException($"Comment with id {id} not found.");
        }
        return comment;
    }

    public IQueryable<Comment> GetManyAsync()
    {
        string json =
            File.ReadAllTextAsync(filePath).Result;
        
        List<Comment> comments = 
            JsonSerializer.Deserialize<List<Comment>>(json)
            ?? new List<Comment>();
        
        return comments.AsQueryable();
    }
}