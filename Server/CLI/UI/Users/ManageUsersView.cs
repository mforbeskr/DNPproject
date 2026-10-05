using Entities;
using RepositoryContracts;
using Spectre.Console;

namespace CLI.UI.Users;

public class ManageUsersView
{
    private readonly IUserRepository userRepo;
    private readonly IPostRepository postRepo;
    private readonly ICommentRepository commentRepo;
    private readonly User currentUser;

    private const string BrowseOption = "Browse users";
    private const string MyProfileOption = "My profile";
    private const string MyAccountOption = "Edit my account";

    private const string ChangeUsernameOption = "Change username";
    private const string ChangePasswordOption = "Change password";
    private const string DeleteAccountOption = "Delete account";

    public ManageUsersView(
        IUserRepository userRepo,
        IPostRepository postRepo,
        ICommentRepository commentRepo,
        User currentUser)
    {
        this.userRepo = userRepo;
        this.postRepo = postRepo;
        this.commentRepo = commentRepo;
        this.currentUser = currentUser;
    }

    public async Task<bool> ShowAsync()
    {
        while (true)
        {
            ConsoleUi.Header("Users", currentUser);

            string choice = ConsoleUi.Menu("Users",
                BrowseOption, MyProfileOption, MyAccountOption, ConsoleUi.Back);

            switch (choice)
            {
                case BrowseOption:
                    BrowseUsers();
                    break;
                case MyProfileOption:
                    ShowProfile(currentUser);
                    break;
                case MyAccountOption:
                    bool deleted = await EditAccountAsync();
                    if (deleted) return true;
                    break;
                case ConsoleUi.Back:
                    return false;
            }
        }
    }

    public static string? AskUsername(IUserRepository userRepo, User? existing = null)
    {
        string label = existing is null
            ? "Username: [grey](empty to go back)[/]"
            : $"New username [grey](was {Markup.Escape(existing.Username)}, empty to go back)[/]:";

        string input = AnsiConsole.Prompt(
            new TextPrompt<string>(label)
                .AllowEmpty()
                .Validate(name =>
                {
                    if (string.IsNullOrWhiteSpace(name)) return ValidationResult.Success();

                    bool taken = userRepo.GetManyAsync()
                        .Any(u => u.Id != (existing == null ? 0 : existing.Id)
                                  && u.Username.ToLower() == name.Trim().ToLower());

                    return taken
                        ? ValidationResult.Error($"[red]'{Markup.Escape(name)}' is already taken[/]")
                        : ValidationResult.Success();
                }));

        return string.IsNullOrWhiteSpace(input) ? null : input.Trim();
    }

    private void BrowseUsers()
    {
        while (true)
        {
            ConsoleUi.Header("Browse users", currentUser);

            ILookup<int, Post> posts = postRepo.GetManyAsync().ToLookup(p => p.UserId);
            ILookup<int, Comment> comments = commentRepo.GetManyAsync().ToLookup(c => c.UserId);

            User? user = ConsoleUi.Pick("Pick a user [grey](type to search)[/]",
                userRepo.GetManyAsync().OrderBy(u => u.Id).ToList(),
                u => $"{Markup.Escape(u.Username)} [grey]#{u.Id} · {posts[u.Id].Count()} posts · {comments[u.Id].Count()} comments[/]");

            if (user is null) return;
            ShowProfile(user);
        }
    }

    private void ShowProfile(User user)
    {
        ConsoleUi.Header($"Profile: {user.Username}", currentUser);

        List<Post> allPosts = postRepo.GetManyAsync().ToList();
        Dictionary<int, string> postTitles = allPosts.ToDictionary(p => p.Id, p => p.Title);

        Table postsTable = ConsoleUi.NewTable("ID", "Post title");
        foreach (Post post in allPosts.Where(p => p.UserId == user.Id).OrderBy(p => p.Id))
        {
            postsTable.AddRow(post.Id.ToString(), Markup.Escape(post.Title));
        }

        Table commentsTable = ConsoleUi.NewTable("On post", "Comment");
        foreach (Comment comment in commentRepo.GetManyAsync().Where(c => c.UserId == user.Id).OrderBy(c => c.Id))
        {
            commentsTable.AddRow(
                Markup.Escape(postTitles.GetValueOrDefault(comment.PostId, "[deleted]")),
                Markup.Escape(comment.Body));
        }

        AnsiConsole.MarkupLine($"[bold]Posts[/] [grey]({postsTable.Rows.Count})[/]");
        AnsiConsole.Write(postsTable);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]Comments[/] [grey]({commentsTable.Rows.Count})[/]");
        AnsiConsole.Write(commentsTable);
        ConsoleUi.Pause();
    }

    private async Task<bool> EditAccountAsync()
    {
        while (true)
        {
            ConsoleUi.Header("Edit my account", currentUser);

            string choice = ConsoleUi.Menu("What do you want to change?",
                ChangeUsernameOption, ChangePasswordOption, DeleteAccountOption, ConsoleUi.Back);

            switch (choice)
            {
                case ChangeUsernameOption:
                    await ChangeUsernameAsync();
                    break;
                case ChangePasswordOption:
                    await ChangePasswordAsync();
                    break;
                case DeleteAccountOption:
                    if (await DeleteAccountAsync()) return true;
                    break;
                case ConsoleUi.Back:
                    return false;
            }
        }
    }

    private async Task ChangeUsernameAsync()
    {
        string? username = AskUsername(userRepo, currentUser);
        if (username is null) return;

        currentUser.Username = username;
        await userRepo.UpdateAsync(currentUser);
        ConsoleUi.Success($"Username changed to [bold]{Markup.Escape(username)}[/]");
    }

    private async Task ChangePasswordAsync()
    {
        string? password = ConsoleUi.AskSecret("New password:");
        if (password is null) return;

        currentUser.Password = password;
        await userRepo.UpdateAsync(currentUser);
        ConsoleUi.Success("Password changed");
    }

    private async Task<bool> DeleteAccountAsync()
    {
        if (!ConsoleUi.Confirm("[red]Delete your account, posts and comments? This cannot be undone.[/]"))
        {
            return false;
        }

        List<int> postIds = postRepo.GetManyAsync()
            .Where(p => p.UserId == currentUser.Id)
            .Select(p => p.Id)
            .ToList();

        List<int> commentIds = commentRepo.GetManyAsync()
            .Where(c => c.UserId == currentUser.Id || postIds.Contains(c.PostId))
            .Select(c => c.Id)
            .ToList();

        foreach (int id in commentIds) await commentRepo.DeleteAsync(id);
        foreach (int id in postIds) await postRepo.DeleteAsync(id);
        await userRepo.DeleteAsync(currentUser.Id);

        ConsoleUi.Success("Account deleted. You have been logged out.");
        return true;
    }
}
