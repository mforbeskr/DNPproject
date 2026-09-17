using CLI.UI;
using FileRepositories;
using RepositoryContracts;

namespace CLI;

public class Program
{
    public static async Task Main(string[] args)
    {
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