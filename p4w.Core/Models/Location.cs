using System;
using System.Collections.Generic;

namespace p4w.Core.Models
{
    public partial class Location
    {
        public Location()
        {
            Reviews = new HashSet<Review>();
        }

        public Guid Id { get; set; }
        public Guid? OwnerId { get; set; }
        public string LocationName { get; set; } = null!;
        public string? Description { get; set; }
        public string Address { get; set; } = null!;
        public string? AddressLink { get; set; }
        public TimeSpan? OpeningHours { get; set; }
        public TimeSpan? ClosingHours { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public bool HasPendingUpdate { get; set; }
        public string? PendingLocationName { get; set; }
        public string? PendingDescription { get; set; }
        public string? PendingAddress { get; set; }
        public string? PendingAddressLink { get; set; }
        public TimeSpan? PendingOpeningHours { get; set; }
        public TimeSpan? PendingClosingHours { get; set; }
        public int? PendingType { get; set; }
        public DateTime? PendingUpdatedAt { get; set; }

        public virtual User? Owner { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }
}

