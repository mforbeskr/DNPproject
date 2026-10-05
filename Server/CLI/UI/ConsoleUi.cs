using Entities;
using Spectre.Console;

namespace CLI.UI;

public static class ConsoleUi
{
    public const string Back = "← Back";
    public const string Accent = "#b026ff";

    private static readonly Color BannerColor = new(106, 0, 244);
    private static readonly Style Highlight = new(new Color(176, 38, 255), decoration: Decoration.Bold);

    public static void Header(string title, User? user = null)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("DNP Forum").Color(BannerColor));

        string text = $"[bold]{Markup.Escape(title)}[/]";
        if (user is not null)
        {
            text += $" [grey]· logged in as[/] [bold {Accent}]{Markup.Escape(user.Username)}[/]";
        }

        AnsiConsole.Write(new Rule(text).LeftJustified().RuleStyle(new Style(BannerColor)));
        AnsiConsole.WriteLine();
    }

    public static string Menu(string title, params string[] options)
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(title)
                .HighlightStyle(Highlight)
                .AddChoices(options));
    }

    public static T? Pick<T>(string title, IEnumerable<T> items, Func<T, string> display) where T : class
    {
        SelectionPrompt<object> prompt = new SelectionPrompt<object>()
            .Title(title)
            .PageSize(12)
            .EnableSearch()
            .HighlightStyle(Highlight)
            .UseConverter(o => o is T item ? display(item) : Back);

        prompt.AddChoices(items);
        prompt.AddChoice(Back);

        return AnsiConsole.Prompt(prompt) as T;
    }

    public static string? AskText(string label)
    {
        string input = AnsiConsole.Prompt(
            new TextPrompt<string>($"{label} [grey](empty to go back)[/]").AllowEmpty());

        return string.IsNullOrWhiteSpace(input) ? null : input.Trim();
    }

    public static string? AskSecret(string label)
    {
        string input = AnsiConsole.Prompt(
            new TextPrompt<string>($"{label} [grey](empty to go back)[/]").AllowEmpty().Secret());

        return string.IsNullOrWhiteSpace(input) ? null : input;
    }

    public static string AskEdit(string label, string current)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>($"{label} [grey](enter keeps current)[/]")
                .DefaultValue(current)
                .DefaultValueStyle(new Style(Color.Grey))).Trim();
    }

    public static bool Confirm(string question) => AnsiConsole.Confirm(question, false);

    public static Table NewTable(params string[] columns)
    {
        Table table = new Table().Border(TableBorder.Rounded).BorderColor(BannerColor);
        foreach (string column in columns)
        {
            table.AddColumn($"[{Accent}]{column}[/]");
        }
        return table;
    }

    public static void Success(string message) => Pause($"[green]✓[/] {message}");

    public static void Error(string message) => Pause($"[red]✗[/] {message}");

    public static void Pause(string? message = null)
    {
        if (message is not null)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine(message);
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
        Console.ReadKey(true);
    }
}
