using CLI.UI.Login;
using CLI.UI.Posts;
using CLI.UI.Users;
using Entities;
using RepositoryContracts;
using Spectre.Console;

namespace CLI.UI;

public class CliApp
{
    private readonly IPostRepository postRepo;
    private readonly IUserRepository userRepo;
    private readonly ICommentRepository commentRepo;

    private const string PostsOption = "Posts";
    private const string UsersOption = "Users";
    private const string LogOutOption = "Log out";
    private const string ExitOption = "Exit";

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
        LoginView loginView = new LoginView(userRepo);

        while (true)
        {
            User? user = await loginView.ShowAsync();
            if (user is null) break;

            bool exit = await MainMenuAsync(user);
            if (exit) break;
        }

        AnsiConsole.MarkupLine("[grey]Bye![/]");
    }

    private async Task<bool> MainMenuAsync(User user)
    {
        ManagePostsView postsView = new ManagePostsView(postRepo, userRepo, commentRepo, user);
        ManageUsersView usersView = new ManageUsersView(userRepo, postRepo, commentRepo, user);

        while (true)
        {
            ConsoleUi.Header("Main menu", user);

            string choice = ConsoleUi.Menu("What do you want to do?",
                PostsOption, UsersOption, LogOutOption, ExitOption);

            switch (choice)
            {
                case PostsOption:
                    await postsView.ShowAsync();
                    break;
                case UsersOption:
                    bool accountDeleted = await usersView.ShowAsync();
                    if (accountDeleted) return false;
                    break;
                case LogOutOption:
                    return false;
                case ExitOption:
                    return true;
            }
        }
    }
}
