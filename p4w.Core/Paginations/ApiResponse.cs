namespace p4w.Core.Paginations;
public class ApiResponse<T>
{
    public int Code { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public MetaData? MetaData { get; set; }
}