using System;

namespace p4w.Core.Models
{
    public partial class Report
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Reason { get; set; } = null!;
        public string TargetType { get; set; } = null!;
        public string TargetId { get; set; } = null!;
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual User User { get; set; } = null!;
    }
}


