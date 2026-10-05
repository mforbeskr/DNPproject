using Entities;
using RepositoryContracts;
using Spectre.Console;

namespace CLI.UI.Comments;

public class ManageCommentsView
{
    private readonly ICommentRepository commentRepo;
    private readonly User currentUser;

    public ManageCommentsView(ICommentRepository commentRepo, User currentUser)
    {
        this.commentRepo = commentRepo;
        this.currentUser = currentUser;
    }

    public async Task AddAsync(Post post)
    {
        string? body = ConsoleUi.AskText("Comment:");
        if (body is null) return;

        Comment created = await commentRepo.AddAsync(new Comment
        {
            Body = body,
            PostId = post.Id,
            UserId = currentUser.Id
        });

        ConsoleUi.Success($"Added comment with ID {created.Id}");
    }

    public async Task EditAsync(List<Comment> postComments)
    {
        Comment? comment = PickOwn("Which comment?", postComments);
        if (comment is null) return;

        comment.Body = ConsoleUi.AskEdit("Comment", comment.Body);
        await commentRepo.UpdateAsync(comment);
        ConsoleUi.Success("Comment updated");
    }

    public async Task DeleteAsync(List<Comment> postComments)
    {
        Comment? comment = PickOwn("Which comment?", postComments);
        if (comment is null) return;

        if (!ConsoleUi.Confirm("[red]Delete this comment?[/]")) return;

        await commentRepo.DeleteAsync(comment.Id);
        ConsoleUi.Success("Comment deleted");
    }

    private Comment? PickOwn(string title, List<Comment> postComments)
    {
        return ConsoleUi.Pick(title,
            postComments.Where(c => c.UserId == currentUser.Id),
            c => $"{Markup.Escape(c.Body)} [grey]#{c.Id}[/]");
    }
}
