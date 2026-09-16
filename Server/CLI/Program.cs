using CLI.UI;
using RepositoryContracts;
using InMemoryRepositories;

namespace CLI;

public class Program
{
    public static async Task Main(string[] args)
    {
        IPostRepository postRepo = new PostInMemoryRepository();
        IUserRepository userRepo = new UserInMemoryRepository();
        ICommentRepository commentRepo = new CommentInMemoryRepository();

        CliApp app = new CliApp(
            postRepo,
            userRepo,
            commentRepo);

        await app.StartAsync();
    }
}