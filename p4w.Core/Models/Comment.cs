using System;
using System.Collections.Generic;

namespace p4w.Core.Models
{
    public partial class Comment
    {
        public Comment()
        {
            InverseParent = new HashSet<Comment>();
        }

        public Guid Id { get; set; }
        public Guid ReviewId { get; set; }
        public Guid UserId { get; set; }
        public Guid? ParentId { get; set; }
        public string Content { get; set; } = null!;
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Comment? Parent { get; set; }
        public virtual Review Review { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Comment> InverseParent { get; set; }
    }
}

