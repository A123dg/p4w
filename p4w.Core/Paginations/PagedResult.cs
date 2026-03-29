namespace p4w.Core.Paginations;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public MetaData MetaData { get; set; } = new();
}
