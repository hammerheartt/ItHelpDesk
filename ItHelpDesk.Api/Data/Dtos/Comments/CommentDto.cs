namespace ItHelpDesk.Api.Data.Dtos.Comments
{
    public record CommentDto(int Id, string Text, bool IsInternal,
                             string AuthorId, string AuthorName, DateTimeOffset CreatedAt);
}