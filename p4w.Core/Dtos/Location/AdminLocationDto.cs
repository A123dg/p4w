namespace p4w.Core.Dtos.Location;

public class AdminLocationDto
{
    public Guid Id { get; set; }
    public Guid? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public string LocationName { get; set; } = null!;
    public string? Description { get; set; }
    public string Address { get; set; } = null!;
    public string? AddressLink { get; set; }
    public int Type { get; set; }
    public string? OpeningHours { get; set; }
    public string? ClosingHours { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = null!;
}
