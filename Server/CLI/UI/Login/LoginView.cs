using CLI.UI.Users;
using Entities;
using RepositoryContracts;
using Spectre.Console;

namespace CLI.UI.Login;

public class LoginView
{
    private readonly IUserRepository userRepo;

    private const string LogInOption = "Log in";
    private const string SignUpOption = "Create new user";
    private const string ExitOption = "Exit";

    public LoginView(IUserRepository userRepo)
    {
        this.userRepo = userRepo;
    }

    public async Task<User?> ShowAsync()
    {
        while (true)
        {
            ConsoleUi.Header("Welcome");

            string choice = ConsoleUi.Menu("Log in or create a user to get started",
                LogInOption, SignUpOption, ExitOption);

            User? user = choice switch
            {
                LogInOption => LogIn(),
                SignUpOption => await SignUpAsync(),
                _ => null
            };

            if (user is not null) return user;
            if (choice == ExitOption) return null;
        }
    }

    private User? LogIn()
    {
        ConsoleUi.Header("Log in");

        User? user = ConsoleUi.Pick("Log in as",
            userRepo.GetManyAsync().OrderBy(u => u.Username).ToList(),
            u => $"{Markup.Escape(u.Username)} [grey]#{u.Id}[/]");

        if (user is null) return null;

        while (true)
        {
            string? password = ConsoleUi.AskSecret($"Password for [bold]{Markup.Escape(user.Username)}[/]:");
            if (password is null) return null;
            if (password == user.Password) return user;

            AnsiConsole.MarkupLine("[red]Wrong password, try again.[/]");
        }
    }

    private async Task<User?> SignUpAsync()
    {
        ConsoleUi.Header("Create new user");

        string? username = ManageUsersView.AskUsername(userRepo);
        if (username is null) return null;

        string? password = ConsoleUi.AskSecret("Password:");
        if (password is null) return null;

        User created = await userRepo.AddAsync(new User
        {
            Username = username,
            Password = password
        });

        ConsoleUi.Success($"Created user [bold]{Markup.Escape(created.Username)}[/] with ID {created.Id}. You are now logged in.");
        return created;
    }
}
