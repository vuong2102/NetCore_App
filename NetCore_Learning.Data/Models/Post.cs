using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore_Learning.Data.Models
{
    public class Post : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? ThumbnailUrl { get; set; }
        public bool IsPublished { get; set; } = false;
        public DateTime? PublishedAt { get; set; }

        // Quan hệ
        public string AuthorId { get; set; }
        public User Author { get; set; } = null!;

        public string CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
