using CLI.UI.Comments;
using Entities;
using RepositoryContracts;
using Spectre.Console;

namespace CLI.UI.Posts;

public class ManagePostsView
{
    private readonly IPostRepository postRepo;
    private readonly IUserRepository userRepo;
    private readonly ICommentRepository commentRepo;
    private readonly User currentUser;
    private readonly ManageCommentsView commentsView;

    private const string BrowseOption = "Browse all posts";
    private const string MyPostsOption = "My posts";
    private const string CreateOption = "Create post";

    private const string AddCommentOption = "Add comment";
    private const string EditPostOption = "Edit post";
    private const string DeletePostOption = "Delete post";
    private const string EditCommentOption = "Edit my comment";
    private const string DeleteCommentOption = "Delete my comment";

    public ManagePostsView(
        IPostRepository postRepo,
        IUserRepository userRepo,
        ICommentRepository commentRepo,
        User currentUser)
    {
        this.postRepo = postRepo;
        this.userRepo = userRepo;
        this.commentRepo = commentRepo;
        this.currentUser = currentUser;
        commentsView = new ManageCommentsView(commentRepo, currentUser);
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleUi.Header("Posts", currentUser);

            string choice = ConsoleUi.Menu("Posts",
                BrowseOption, MyPostsOption, CreateOption, ConsoleUi.Back);

            switch (choice)
            {
                case BrowseOption:
                    await BrowseAsync(onlyMine: false);
                    break;
                case MyPostsOption:
                    await BrowseAsync(onlyMine: true);
                    break;
                case CreateOption:
                    await CreatePostAsync();
                    break;
                case ConsoleUi.Back:
                    return;
            }
        }
    }

    private async Task BrowseAsync(bool onlyMine)
    {
        while (true)
        {
            ConsoleUi.Header(onlyMine ? "My posts" : "Posts overview", currentUser);

            List<Post> posts = postRepo.GetManyAsync()
                .Where(p => !onlyMine || p.UserId == currentUser.Id)
                .OrderBy(p => p.Id)
                .ToList();

            if (posts.Count == 0)
            {
                ConsoleUi.Pause("[yellow]No posts yet.[/]");
                return;
            }

            Dictionary<int, string> usernames = Usernames();
            ILookup<int, Comment> comments = commentRepo.GetManyAsync().ToLookup(c => c.PostId);

            Table table = ConsoleUi.NewTable("ID", "Title", "Author", "Comments");
            foreach (Post post in posts)
            {
                table.AddRow(
                    post.Id.ToString(),
                    Markup.Escape(post.Title),
                    Markup.Escape(usernames.GetValueOrDefault(post.UserId, "[deleted]")),
                    comments[post.Id].Count().ToString());
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();

            Post? selected = ConsoleUi.Pick("Open a post [grey](type to search)[/]", posts,
                p => $"{Markup.Escape(p.Title)} [grey]#{p.Id}[/]");

            if (selected is null) return;
            await ShowPostAsync(selected.Id);
        }
    }

    private async Task CreatePostAsync()
    {
        ConsoleUi.Header("Create post", currentUser);

        string? title = ConsoleUi.AskText("Title:");
        if (title is null) return;

        string? body = ConsoleUi.AskText("Body:");
        if (body is null) return;

        Post created = await postRepo.AddAsync(new Post
        {
            Title = title,
            Body = body,
            UserId = currentUser.Id
        });

        ConsoleUi.Success($"Created post [bold]{Markup.Escape(created.Title)}[/] with ID {created.Id}");
    }

    private async Task ShowPostAsync(int postId)
    {
        while (true)
        {
            Post post = await postRepo.GetSingleAsync(postId);
            Dictionary<int, string> usernames = Usernames();
            List<Comment> comments = commentRepo.GetManyAsync()
                .Where(c => c.PostId == postId)
                .OrderBy(c => c.Id)
                .ToList();

            ConsoleUi.Header($"Post #{post.Id}", currentUser);
            RenderPost(post, comments, usernames);

            bool isOwner = post.UserId == currentUser.Id;
            bool hasOwnComments = comments.Any(c => c.UserId == currentUser.Id);

            List<string> options = [AddCommentOption];
            if (isOwner) options.AddRange([EditPostOption, DeletePostOption]);
            if (hasOwnComments) options.AddRange([EditCommentOption, DeleteCommentOption]);
            options.Add(ConsoleUi.Back);

            AnsiConsole.WriteLine();
            string choice = ConsoleUi.Menu("What now?", options.ToArray());

            switch (choice)
            {
                case AddCommentOption:
                    await commentsView.AddAsync(post);
                    break;
                case EditPostOption:
                    await EditPostAsync(post);
                    break;
                case DeletePostOption:
                    if (await DeletePostAsync(post, comments)) return;
                    break;
                case EditCommentOption:
                    await commentsView.EditAsync(comments);
                    break;
                case DeleteCommentOption:
                    await commentsView.DeleteAsync(comments);
                    break;
                case ConsoleUi.Back:
                    return;
            }
        }
    }

    private void RenderPost(Post post, List<Comment> comments, Dictionary<int, string> usernames)
    {
        string author = usernames.GetValueOrDefault(post.UserId, "[deleted]");

        AnsiConsole.Write(new Panel(Markup.Escape(post.Body))
            .Header($" [bold]{Markup.Escape(post.Title)}[/] [grey]by {Markup.Escape(author)}[/] ")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Purple)
            .Padding(2, 1)
            .Expand());

        Tree tree = new Tree($"[bold]Comments[/] [grey]({comments.Count})[/]");

        if (comments.Count == 0)
        {
            tree.AddNode("[grey]No comments yet.[/]");
        }

        foreach (Comment comment in comments)
        {
            string commenter = usernames.GetValueOrDefault(comment.UserId, "[deleted]");
            tree.AddNode($"[{ConsoleUi.Accent}]{Markup.Escape(commenter)}[/]: {Markup.Escape(comment.Body)}");
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(tree);
    }

    private async Task EditPostAsync(Post post)
    {
        ConsoleUi.Header($"Edit post #{post.Id}", currentUser);

        post.Title = ConsoleUi.AskEdit("Title", post.Title);
        post.Body = ConsoleUi.AskEdit("Body", post.Body);

        if (!ConsoleUi.Confirm("Save changes?")) return;

        await postRepo.UpdateAsync(post);
        ConsoleUi.Success("Post updated");
    }

    private async Task<bool> DeletePostAsync(Post post, List<Comment> comments)
    {
        if (!ConsoleUi.Confirm($"[red]Delete '{Markup.Escape(post.Title)}' and its {comments.Count} comments?[/]"))
        {
            return false;
        }

        foreach (Comment comment in comments) await commentRepo.DeleteAsync(comment.Id);
        await postRepo.DeleteAsync(post.Id);

        ConsoleUi.Success("Post deleted");
        return true;
    }

    private Dictionary<int, string> Usernames() =>
        userRepo.GetManyAsync().ToDictionary(u => u.Id, u => u.Username);
}
