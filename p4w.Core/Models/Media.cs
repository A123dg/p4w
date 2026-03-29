using System;
using System.Collections.Generic;

namespace p4w.Core.Models
{
    public partial class Media
    {
        public Media()
        {
            MediaLinks = new HashSet<MediaLink>();
        }

        public Guid Id { get; set; }
        public string Url { get; set; } = null!;
        public string MimeType { get; set; } = null!;
        public long Size { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<MediaLink> MediaLinks { get; set; }
    }
}

