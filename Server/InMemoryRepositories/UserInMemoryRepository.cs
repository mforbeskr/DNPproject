using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class UserInMemoryRepository : IUserRepository
{
    private readonly List<User> users = [];

    public UserInMemoryRepository()
    {
        users.Add(new User
        {
            Id = 1,
            Username = "Magnus",
            Password = "4444",
        });

        users.Add(new User
        {
            Id = 2,
            Username = "Marley",
            Password = "4444",
        });
        users.Add(new User
        {
            Id = 3,
            Username = "James",
            Password = "4444",
        });
        users.Add(new User
        {
            Id = 4,
            Username = "Bobby",
            Password = "4444",
        });
    }

    public Task<User> AddAsync(User user)
    {
        user.Id = users.Any() ? users.Max(u => u.Id) + 1 : 1;
        users.Add(user);
        return Task.FromResult(user);
    }

    public Task UpdateAsync(User user)
    {
        User? existingUser = users.SingleOrDefault(u => u.Id == user.Id);
        if (existingUser is null)
        {
            throw new InvalidOperationException(
                $"User with ID '{user.Id}' not found");
        }

        users.Remove(existingUser);

        users.Add(user);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        User? userToRemove = users.SingleOrDefault(u => u.Id == id);
        if (userToRemove is null)
        {
            throw new InvalidOperationException($"User with ID '{id}' not found");
        }

        users.Remove(userToRemove);
        return Task.CompletedTask;
    }

    public Task<User> GetSingleAsync(int id)
    {
        User? user = users.SingleOrDefault(u => u.Id == id);
        if (user is null)
        {
            throw new InvalidOperationException($"User with ID '{id}' not found");
        }

        return Task.FromResult(user);
    }

    public IQueryable<User> GetManyAsync()
    {
        return users.AsQueryable();
    }
}