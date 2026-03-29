namespace p4w.Core.Dtos.User;

public class RecentLocationDto
{
    public Guid Id { get; set; }
    public string LocationName { get; set; } = null!;
    public string? Description { get; set; }
    public string Address { get; set; } = null!;
    public string? AddressLink { get; set; }
    public string? OpeningHours { get; set; }
    public string? ClosingHours { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime LastInteractionAt { get; set; }
    public string LastInteractionType { get; set; } = null!;
}
