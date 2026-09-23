using Entities;
using RepositoryContracts;

namespace CLI.UI;

public class CliApp
{
    private readonly IPostRepository postRepo;
    private readonly IUserRepository userRepo;
    private readonly ICommentRepository commentRepo;

    public CliApp(
        IPostRepository postRepo,
        IUserRepository userRepo,
        ICommentRepository commentRepo)
    {
        this.postRepo = postRepo;
        this.userRepo = userRepo;
        this.commentRepo = commentRepo;
    }

    public async Task StartAsync()
    {
        bool running = true;

        while (running)
        {
            Console.Clear();
            Console.WriteLine("\n //// Welcome to the DNP Forum App //// ");
            Console.WriteLine("1. Create User");
            Console.WriteLine("2. Create Post");
            Console.WriteLine("3. Create Comment");
            Console.WriteLine("4. Posts Overview");
            Console.WriteLine("5. View Specific Post");
            Console.WriteLine("0. Exit Application");

            Console.Write("\nChoose option: ");
            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await CreateUser();
                    break;

                case "2":
                    await CreatePost();
                    break;

                case "3":
                    await CreateComment();
                    break;

                case "4":
                    await ViewPostsOverview();
                    break;

                case "5":
                    await ViewSpecificPost();
                    break;

                case "0":
                    running = false;
                    break;

                default:
                    Console.WriteLine("Invalid choice");
                    break;
            }
        }
    }

    private async Task CreateComment()
    {
        Console.Clear();

        Console.Write("Enter comment body: ");
        string? body = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(body))
        {
            Console.WriteLine("Comment body is required.");
            Console.ReadLine();
            return;
        }

        Console.Write("Enter user id: ");

        if (!int.TryParse(Console.ReadLine(), out int userId))
        {
            Console.WriteLine("Invalid user id.");
            Console.ReadLine();
            return;
        }

        Console.Write("Enter post id: ");

        if (!int.TryParse(Console.ReadLine(), out int postId))
        {
            Console.WriteLine("Invalid post id.");
            Console.ReadLine();
            return;
        }

        try
        {
            await userRepo.GetSingleAsync(userId);
            await postRepo.GetSingleAsync(postId);

            Comment comment = new Comment
            {
                Body = body,
                UserId = userId,
                PostId = postId
            };

            Comment createdComment =
                await commentRepo.AddAsync(comment);

            Console.WriteLine();
            Console.WriteLine(
                $"Created comment with ID {createdComment.Id}");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
        }

        Console.WriteLine();
        Console.WriteLine("Press enter to continue");
        Console.ReadLine();
    }


    private async Task CreatePost()
    {
            Console.Clear();

            Console.Write("Enter title: ");
            string? title = Console.ReadLine();

            Console.Write("Enter body: ");
            string? body = Console.ReadLine();

            Console.Write("Enter user id: ");

            if (!int.TryParse(Console.ReadLine(), out int userId))
            {
                Console.WriteLine("Invalid user id");
                Console.ReadLine();
                return;
            }

            try
            {
                await userRepo.GetSingleAsync(userId);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("User does not exist.");
                Console.ReadLine();
                return;
            }

            if (string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(body))
            {
                Console.WriteLine("Title and body are required.");
                Console.ReadLine();
                return;
            }

            Post post = new Post
            {
                Title = title,
                Body = body,
                UserId = userId
            };

            Post createdPost = await postRepo.AddAsync(post);

            Console.WriteLine();
            Console.WriteLine(
                $"Created post '{createdPost.Title}' with ID {createdPost.Id}");

            Console.WriteLine();
            Console.WriteLine("Press enter to continue");
            Console.ReadLine();
    }

    private async Task CreateUser()
    {
        Console.Clear();

        Console.Write("Enter username: ");
        string? username = Console.ReadLine();

        Console.Write("Enter password: ");
        string? password = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            Console.WriteLine("Username and password are required");
            Console.ReadLine();
            return;
        }

        bool usernameTaken = userRepo.GetManyAsync()
            .Any(u => u.Username.ToLower() == username.ToLower());

        if (usernameTaken)
        {
            Console.WriteLine($"Username '{username}' is already taken");
            Console.ReadLine();
            return;
        }

        User user = new User
        {
            Username = username ?? "",
            Password = password ?? ""
        };

        User createdUser = await userRepo.AddAsync(user);

        Console.WriteLine();
        Console.WriteLine(
            $"Created user: '{createdUser.Username}' created with ID {createdUser.Id}");
        Console.WriteLine($"Total users: {userRepo.GetManyAsync().Count()}");
        Console.WriteLine();
        Console.WriteLine("Press enter to continue");
        Console.ReadLine();
    }

    private async Task ViewSpecificPost()
    {
        Console.Clear();
        Console.Write("Enter post id: ");

        if (!int.TryParse(Console.ReadLine(), out int lookUpId))
        {
            Console.WriteLine("Invalid id");
            Console.ReadLine();
            return;
        }

        try
        {
            var post = await postRepo.GetSingleAsync(lookUpId);

            Console.WriteLine();
            Console.WriteLine($"Title: {post.Title}");
            Console.WriteLine($"Body: {post.Body}");
            Console.WriteLine();
            Console.WriteLine("Comments:");

            var comments = commentRepo
                .GetManyAsync()
                .Where(c => c.PostId == lookUpId);

            if (!comments.Any())
            {
                Console.WriteLine("No comments.");
            }
            else
            {
                foreach (var comment in comments)
                {
                    Console.WriteLine($"- {comment.Body} (User {comment.UserId})");
                }
            }
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
        }

        Console.WriteLine();
        Console.WriteLine("Press enter to continue");
        Console.ReadLine();
    }

    private Task ViewPostsOverview()
    {
        Console.Clear();
        Console.Write("=== Posts Overview ===");
        Console.WriteLine();

        var posts = postRepo.GetManyAsync();

        foreach (var post in posts)
        {
            Console.WriteLine(
                $"Id: {post.Id}, Title: {post.Title}, UserId: {post.UserId}");
        }

        Console.WriteLine();
        Console.WriteLine("Press enter to continue");
        Console.ReadLine();

        return Task.CompletedTask;
    }
}