namespace ImageManager.Data.Responses;

public class PaginatedResponse<T>
{
    public ICollection<T> Data { get; init; }
    public int Page { get; init; }
    public int PerPage { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
    public int Next { get; init; }
    public int Previous { get; init; }
}