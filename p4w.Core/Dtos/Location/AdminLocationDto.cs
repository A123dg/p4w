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
    public List<string> MediaLinkUrls { get; set; } = [];
    public int Type { get; set; }
    public string? OpeningHours { get; set; }
    public string? ClosingHours { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = null!;
    public int? PreviousStatus { get; set; }
    public string? PreviousStatusName { get; set; }
    public bool HasPendingUpdate { get; set; }
    public string? PendingLocationName { get; set; }
    public string? PendingDescription { get; set; }
    public string? PendingAddress { get; set; }
    public string? PendingAddressLink { get; set; }
    public List<string> PendingMediaLinkUrls { get; set; } = [];
    public int? PendingType { get; set; }
    public string? PendingOpeningHours { get; set; }
    public string? PendingClosingHours { get; set; }
    public DateTime? PendingUpdatedAt { get; set; }
}
