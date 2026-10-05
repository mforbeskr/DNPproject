using CLI.UI;
using FileRepositories;
using RepositoryContracts;

namespace CLI;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        IPostRepository postRepo = new PostFileRepository();
        IUserRepository userRepo = new UserFileRepository();
        ICommentRepository commentRepo = new CommentFileRepository();

        CliApp app = new CliApp(
            postRepo,
            userRepo,
            commentRepo);

        await app.StartAsync();
    }
}