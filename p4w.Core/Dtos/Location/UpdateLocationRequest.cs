namespace p4w.Core.Dtos.Location;

public class UpdateLocationRequest
{
    public string? LocationName { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? AddressLink { get; set; }
    public List<string> MediaLinkUrls { get; set; } = [];
    public string? OpeningHours { get; set; }
    public string? ClosingHours { get; set; }
    public int? Type { get; set; }
    public int? Status { get; set; }
}
