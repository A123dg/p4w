using System;

namespace p4w.Core.Models
{
    public partial class MediaLink
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string EntityType { get; set; } = null!;
        public Guid EntityId { get; set; }
        public string MediaType { get; set; } = null!;
        public int SortOrder { get; set; }
        public Guid MediaId { get; set; }

        public virtual Media Media { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}

