using System.Text.Json;
using Entities;
using RepositoryContracts;

namespace FileRepositories;

public class PostFileRepository : IPostRepository
{
    private readonly string dataDir = Path.Combine("..", "Data");
    private readonly string filePath;

    public PostFileRepository()
    {
        filePath = Path.Combine(dataDir, "posts.json");

        Directory.CreateDirectory(dataDir);

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "[]");
        }
    }

    private async Task<List<Post>> LoadPosts()
    {
        string json = await File.ReadAllTextAsync(filePath);

        return JsonSerializer.Deserialize<List<Post>>(json)
            ?? new List<Post>();
    }

    private async Task SavePosts(List<Post> posts)
    {
        string json = JsonSerializer.Serialize(posts);

        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<Post> AddAsync(Post post)
    {
        List<Post> posts = await LoadPosts();

        int maxId = posts.Count > 0 ? posts.Max(u => u.Id) : 0;

        post.Id = maxId + 1;

        posts.Add(post);

        await SavePosts(posts);

        return post;
    }

    public async Task UpdateAsync(Post post)
    {
        List<Post> posts = await LoadPosts();

        Post? existingPost = posts.SingleOrDefault(u => u.Id == post.Id);

        if (existingPost is null)
        {
            throw new InvalidOperationException($"Post with id {post.Id} not found.");
        }

        posts.Remove(existingPost);
        posts.Add(post);

        await SavePosts(posts);
    }

    public async Task DeleteAsync(int id)
    {
        List<Post> posts = await LoadPosts();

        Post? postToRemove = posts.SingleOrDefault(u => u.Id == id);
        if (postToRemove is null)
        {
            throw new InvalidOperationException($"Post with id {id} not found.");
        }

        posts.Remove(postToRemove);

        await SavePosts(posts);
    }

    public async Task<Post> GetSingleAsync(int id)
    {
        List<Post> posts = await LoadPosts();

        Post? post = posts.SingleOrDefault(u => u.Id == id);

        if (post is null)
        {
            throw new InvalidOperationException($"Post with id {id} not found.");
        }
        return post;
    }

    public IQueryable<Post> GetManyAsync()
    {
        string json =
            File.ReadAllTextAsync(filePath).Result;
        
        List<Post> posts = 
            JsonSerializer.Deserialize<List<Post>>(json)
            ?? new List<Post>();
        
        return posts.AsQueryable();
    }
}