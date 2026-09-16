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
        Console.WriteLine("Forum app started");

        await Task.CompletedTask;
    }
}