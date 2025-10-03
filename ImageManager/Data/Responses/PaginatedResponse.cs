namespace ImageManager.Data.Responses;

public class PaginatedResponse<T>
{
    public required ICollection<T> Data { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalItems { get; init; }
    public required int TotalPages { get; init; }
    public int Next { get; init; }
    public int Previous { get; init; }
}