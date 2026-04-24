namespace ShiftMate.Application.Common;

// En generisk klass för att representera paginerade resultat, inklusive metadata om total antal poster och sidindelning.
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 1;
}
