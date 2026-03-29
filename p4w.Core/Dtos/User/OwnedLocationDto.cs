namespace p4w.Core.Dtos.User;

public class OwnedLocationDto
{
    public Guid Id { get; set; }
    public string LocationName { get; set; } = null!;
    public string Address { get; set; } = null!;
    public int Status { get; set; }
    public string StatusName { get; set; } = null!;
}
