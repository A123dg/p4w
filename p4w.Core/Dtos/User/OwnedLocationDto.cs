namespace p4w.Core.Dtos.User;

public class OwnedLocationDto
{
    public Guid Id { get; set; }
    public string LocationName { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? AddressLink { get; set; }
    public List<string> MediaLinkUrls { get; set; } = [];
    public int Status { get; set; }
    public string StatusName { get; set; } = null!;
    public int? PreviousStatus { get; set; }
    public string? PreviousStatusName { get; set; }
}
