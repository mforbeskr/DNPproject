using System.Text.Json;
using Entities;
using RepositoryContracts;

namespace FileRepositories;

public class UserFileRepository : IUserRepository
{
    private readonly string dataDir = Path.Combine("..", "Data");
    private readonly string filePath;

    public UserFileRepository()
    {
        filePath = Path.Combine(dataDir, "users.json");

        Directory.CreateDirectory(dataDir);

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "[]");
        }
    }

    private async Task<List<User>> LoadUsers()
    {
        string json = await File.ReadAllTextAsync(filePath);

        return JsonSerializer.Deserialize<List<User>>(json)
            ?? new List<User>();
    }

    private async Task SaveUsers(List<User> users)
    {
        string json = JsonSerializer.Serialize(users);

        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<User> AddAsync(User user)
    {
        List<User> users = await LoadUsers();

        int maxId = users.Count > 0 ? users.Max(u => u.Id) : 0;

        user.Id = maxId + 1;

        users.Add(user);

        await SaveUsers(users);

        return user;
    }

    public async Task UpdateAsync(User user)
    {
        List<User> users = await LoadUsers();

        User? existingUser = users.SingleOrDefault(u => u.Id == user.Id);

        if (existingUser is null)
        {
            throw new InvalidOperationException($"User with id {user.Id} not found.");
        }

        users.Remove(existingUser);
        users.Add(user);

        await SaveUsers(users);
    }

    public async Task DeleteAsync(int id)
    {
        List<User> users = await LoadUsers();

        User? userToRemove = users.SingleOrDefault(u => u.Id == id);
        if (userToRemove is null)
        {
            throw new InvalidOperationException($"User with id {id} not found.");
        }

        users.Remove(userToRemove);

        await SaveUsers(users);
    }

    public async Task<User> GetSingleAsync(int id)
    {
        List<User> users = await LoadUsers();

        User? user = users.SingleOrDefault(u => u.Id == id);

        if (user is null)
        {
            throw new InvalidOperationException($"User with id {id} not found.");
        }
        return user;
    }

    public IQueryable<User> GetManyAsync()
    {
        string json =
            File.ReadAllTextAsync(filePath).Result;
        
        List<User> users = 
            JsonSerializer.Deserialize<List<User>>(json)
            ?? new List<User>();
        
        return users.AsQueryable();
    }
}